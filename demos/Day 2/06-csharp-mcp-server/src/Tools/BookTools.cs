using System.ComponentModel;
using System.Text.Json;
using Demo6.McpServer.Services;
using ModelContextProtocol.Server;

namespace Demo6.McpServer.Tools;

/// <summary>
/// The MCP tool surface. Each tool is a thin governed gateway: it validates input, logs the call,
/// and delegates to the catalog service — Claude calls your tools, never the data store.
/// Services are DI-injected into the tool methods by the MCP SDK.
/// </summary>
[McpServerToolType]
public class BookTools
{
    [McpServerTool(Name = "search_books")]
    [Description("Search BookTracker for books whose title, author, genre, or summary matches a query. "
        + "Returns matching books as JSON.")]
    public string SearchBooks(
        IBookCatalog catalog,
        ILogger<BookTools> logger,
        [Description("Text to match against book titles, authors, genres, and summaries")] string query)
    {
        logger.LogInformation("Tool invoked: search_books query={Query}", query);
        if (string.IsNullOrWhiteSpace(query))
        {
            return "Error: query is required.";
        }

        return Serialize(catalog.Search(query));
    }

    [McpServerTool(Name = "get_book_details")]
    [Description("Get the full details of a single BookTracker book (title, author, genre, year, "
        + "page count, summary) by its id. Returns null if no book has that id.")]
    public string GetBookDetails(
        IBookCatalog catalog,
        ILogger<BookTools> logger,
        [Description("The book id")] int bookId)
    {
        logger.LogInformation("Tool invoked: get_book_details bookId={BookId}", bookId);
        return Serialize(catalog.GetById(bookId));
    }

    [McpServerTool(Name = "add_book")]
    [Description("Add a new book to BookTracker. Returns the created book as JSON, "
        + "or an error if input is invalid.")]
    public string AddBook(
        IBookCatalog catalog,
        ILogger<BookTools> logger,
        [Description("Book title")] string title,
        [Description("Author name")] string author,
        [Description("Genre")] string genre,
        [Description("Total page count")] int totalPages)
    {
        logger.LogInformation("Tool invoked: add_book title={Title}, author={Author}", title, author);

        if (string.IsNullOrWhiteSpace(title))
        {
            return "Error: title is required.";
        }

        if (string.IsNullOrWhiteSpace(author))
        {
            return "Error: author is required.";
        }

        if (totalPages <= 0)
        {
            return "Error: totalPages must be greater than zero.";
        }

        return Serialize(catalog.Add(title.Trim(), author.Trim(), genre.Trim(), totalPages));
    }

    private static string Serialize(object? value) => JsonSerializer.Serialize(value);
}
