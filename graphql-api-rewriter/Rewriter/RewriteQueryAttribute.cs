namespace graphql_api_rewriter.Rewriter;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RewriteQueryAttribute : Attribute
{
    public string QueryOperationName { get; }

    public RewriteQueryAttribute(string queryOperationName)
    {
        if (string.IsNullOrEmpty(queryOperationName))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(queryOperationName));
        }

        QueryOperationName = queryOperationName;
    }
}