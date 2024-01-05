using System.Text.Json;
using System.Text.Json.Nodes;
using HotChocolate.Language;
using HotChocolate.Language.Utilities;
using HotChocolate.Language.Visitors;

namespace graphql_api_rewriter.Rewriter;

public class RewriterMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.ContentLength.HasValue)
        {
            await next(context);
            return;
        }
        
        var buffer = await ReadRequestAndReplaceWithMemoryStream(context);

        var graphQlRequests = Utf8GraphQLRequestParser.Parse(buffer.Span, ParserOptions.Default);

        // Reject the rewrite if..
        // 1. There's more than one request (we don't support batch queries at this point)
        // 2. The query is null or there's more than a single definition in the query (i.e. multiple queries)
        if (graphQlRequests.Count != 1 || graphQlRequests[0].Query == null || graphQlRequests[0].Query!.Definitions.Count != 1)
        {
            await next(context);
            return;
        }

        RewriteRequest(context, buffer, graphQlRequests[0]);

        await next(context);
    }

    private static async Task<Memory<byte>> ReadRequestAndReplaceWithMemoryStream(HttpContext context)
    {
        var requestContentLength = context.Request.ContentLength!.Value;
        var bytesRemaining = requestContentLength;
        var buffer = new Memory<byte>(new byte[requestContentLength]);

        while (bytesRemaining > 0)
        {
            var bytesRead = await context.Request.Body.ReadAsync(buffer, context.RequestAborted);
            bytesRemaining -= bytesRead;
        }

        context.Request.Body = new MemoryStream(buffer.ToArray());
        
        return buffer;
    }

    private static void RewriteRequest(HttpContext context, Memory<byte> originRequestBody, GraphQLRequest graphQlRequest)
    {
        var operationDefinitionNode = graphQlRequest.Query!.Definitions[0] as OperationDefinitionNode;
        var rewriterForOperation = RewriterFactory.GetRewriterForOperation(operationDefinitionNode);

        if (rewriterForOperation is not NoOp)
        {
            var rewriter = SyntaxRewriter.CreateWithNavigator(
                (node, navigatorContext) =>
                {
                    return node switch
                    {
                        VariableDefinitionNode variableDefinition => rewriterForOperation.VariableDefinitionRewrite(variableDefinition, navigatorContext),
                        ArgumentNode argument => rewriterForOperation.ArgumentRewrite(argument, navigatorContext),
                        FieldNode field when rewriterForOperation.FieldShouldBeRemoved(field, navigatorContext) => default,
                        _ => node
                    };
                });

            if (rewriter.Rewrite(graphQlRequest.Query, new NavigatorContext()) is DocumentNode rewrittenQuery)
            {
                graphQlRequest = new GraphQLRequest(query: rewrittenQuery, queryId: graphQlRequest.QueryId,
                    queryHash: graphQlRequest.QueryHash, operationName: graphQlRequest.OperationName,
                    variables: graphQlRequest.Variables, extensions: graphQlRequest.Extensions);
            }
        }

        var serializer = new SyntaxSerializer(options: new SyntaxSerializerOptions());
        var stringSyntaxWriter = new StringSyntaxWriter();
        serializer.Serialize(graphQlRequest.Query!, stringSyntaxWriter);

        RewriteRequestBody(context, originRequestBody, stringSyntaxWriter.ToString());
    }

    private static void RewriteRequestBody(HttpContext context, Memory<byte> originRequestBody, string newQuery)
    {
        var jsonNode = JsonNode.Parse(originRequestBody.Span);
        if (jsonNode == null)
        {
            return;
        }

        jsonNode["query"] = newQuery;
        context.Request.Body.SetLength(0);

        using (var utf8JsonWriter = new Utf8JsonWriter(context.Request.Body))
        {
            jsonNode.WriteTo(utf8JsonWriter);
        }

        context.Request.Body.Position = 0;
    }
}