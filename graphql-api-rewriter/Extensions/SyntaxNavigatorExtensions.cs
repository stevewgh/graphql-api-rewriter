using HotChocolate.Language;
using HotChocolate.Language.Visitors;

namespace graphql_api_rewriter.Extensions;

public static class SyntaxNavigatorExtensions
{
    public static TNode? GetAncestorExcludingSelf<TNode>(this ISyntaxNavigator navigator, TNode self) where TNode : ISyntaxNode
    {
        var ancestors = navigator.GetAncestors<TNode>().Where(node => !Equals(node, self));
        return ancestors.FirstOrDefault();
    }
    
    public static IEnumerable<TNode> GetAncestorsExcludingSelf<TNode>(this ISyntaxNavigator navigator, TNode self) where TNode : ISyntaxNode
    {
        return navigator.GetAncestors<TNode>().Where(node => !Equals(node, self));
    }
}