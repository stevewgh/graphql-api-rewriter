using HotChocolate.Language;
using HotChocolate.Language.Visitors;

namespace graphql_api_rewriter.Rewriter;

public interface IRewriteQueries
{
    bool FieldShouldBeRemoved(FieldNode node, NavigatorContext navigatorContext);
    ArgumentNode ArgumentRewrite(ArgumentNode node, NavigatorContext navigatorContext);
    VariableDefinitionNode VariableDefinitionRewrite(VariableDefinitionNode node, NavigatorContext navigatorContext);
}