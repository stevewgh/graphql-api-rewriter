using graphql_api_rewriter.Extensions;
using HotChocolate.Language;
using HotChocolate.Language.Visitors;

namespace graphql_api_rewriter.Rewriter.Queries;

[RewriteQuery("bad_request")]
public class BadRequestQuery : IRewriteQueries
{
    private readonly string[] _fieldsToRemove = {"this_field_doesnt_exist"};

    public bool FieldShouldBeRemoved(FieldNode node, NavigatorContext navigatorContext)
    {
        //  simple implementation will remove the fields at any level
        //  but using the navigatorContext it is possible to check where the
        //  field is within the request, allowing us to target a field that belongs
        //  to a particular type instead of all types.
        //  e.g. navigatorContext.Navigator.GetAncestor<SelectionSet>()

        var parentField = navigatorContext.Navigator.GetAncestorExcludingSelf(node);
        
        return
            parentField is { Name.Value: "book" } && 
            _fieldsToRemove.Contains(node.Name.Value);
    }

    public ArgumentNode ArgumentRewrite(ArgumentNode node, NavigatorContext navigatorContext) => node;

    public VariableDefinitionNode VariableDefinitionRewrite(VariableDefinitionNode node, NavigatorContext navigatorContext)
    {
        //  example of changing the argument from nullable to non-nullable
        return node.Variable.Name.Value == "arg"
            ? node.WithType(new NonNullTypeNode(new NamedTypeNode(node.Type.Name())))
            : node;
    }
}