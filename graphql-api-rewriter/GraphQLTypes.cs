namespace graphql_api_rewriter;

public class Query
{
    public Book GetBook(string nonOptionalArgument, string? optionalArgument = null) =>
        new Book
        {
            Title = "C# in depth.",
            Author = new Author
            {
                Name = "Jon Skeet"
            }
        };
}

public class Book
{
    public string Title { get; set; }

    public Author Author { get; set; }
}

public class Author
{
    public string Name { get; set; }
}