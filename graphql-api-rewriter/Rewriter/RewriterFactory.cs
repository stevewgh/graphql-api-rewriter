using HotChocolate.Language;

namespace graphql_api_rewriter.Rewriter;

public static class RewriterFactory
{
    private static readonly Dictionary<string, IRewriteQueries> ReWriters = Init();
    private static readonly IRewriteQueries NoOp = new NoOp();

    public static IRewriteQueries GetRewriterForOperation(OperationDefinitionNode? operationDefinitionNode)
    {
        var key = operationDefinitionNode?.Name?.Value ?? string.Empty;
        var rewriter = ReWriters.GetValueOrDefault(key, NoOp);

        return rewriter;
    }
    
    private static Dictionary<string, IRewriteQueries> Init()
    {
        var types =
            typeof(RewriterFactory).Assembly.GetExportedTypes()
                .Where(type => type is { IsClass: true, IsAbstract: false, IsPublic: true })
                .Select(type => new { atts = type.GetCustomAttributes(typeof(RewriteQueryAttribute), false), type = type })
                .Where(arg => arg.atts.Length > 0)
                .Select(arg => new { ((RewriteQueryAttribute)arg.atts.First()).QueryOperationName, arg.type });

        var tmp = new Dictionary<string, IRewriteQueries>();
        foreach (var tuple in types)
        {
            var rewriter = (IRewriteQueries)Activator.CreateInstance(tuple.type)!;
            
            if (tmp.TryAdd(tuple.QueryOperationName, rewriter) == false)
            {
                throw new Exception($"Unable to add key '{tuple.QueryOperationName}' to {nameof(RewriterFactory)}, is there a duplicate {nameof(RewriteQueryAttribute.QueryOperationName)} registered elsewhere?");
            }
        }

        return tmp;
    }
}