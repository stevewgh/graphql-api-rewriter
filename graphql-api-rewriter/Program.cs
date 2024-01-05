using graphql_api_rewriter;
using graphql_api_rewriter.Rewriter;
using HotChocolate;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton<RewriterMiddleware>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>();

var app = builder.Build();

app.UseMiddleware<RewriterMiddleware>();

app.MapGraphQL();

app.Run();

return;
