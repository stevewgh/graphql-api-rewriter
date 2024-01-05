using HotChocolate.Language;
using HotChocolate.Language.Visitors;

namespace graphql_api_rewriter.Rewriter;

public class NoOp : IRewriteQueries
{
    public bool FieldShouldBeRemoved(FieldNode node, NavigatorContext navigatorContext) => false;
    public ArgumentNode ArgumentRewrite(ArgumentNode node, NavigatorContext navigatorContext) => node;
    public VariableDefinitionNode VariableDefinitionRewrite(VariableDefinitionNode node, NavigatorContext navigatorContext) => node;
}