using System.ComponentModel;
using System.Text.Json;
using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using BookTracker.Mcp.Security;
using ModelContextProtocol.Server;

namespace BookTracker.Mcp.Tools;

/// <summary>
/// The MCP tool surface. Each tool is a thin governed gateway: it validates input, audits the call,
/// and delegates to the same Core services the HTTP API uses — Claude never touches the database.
/// Services and the audit logger are DI-injected into the tool methods by the MCP SDK.
/// </summary>
[McpServerToolType]
public class BookMcpTools
{
    [McpServerTool]
    [Description("Search BookTracker for books whose title matches a query. Returns matching books as JSON.")]
    public async Task<string> SearchBooks(
        IBookService books,
        AuditLogger audit,
        [Description("Text to match against book titles")] string query,
        CancellationToken ct)
    {
        audit.ToolInvoked("search_books", $"query={query}");
        if (string.IsNullOrWhiteSpace(query))
        {
            return "Error: query is required.";
        }

        return Serialize(await books.SearchAsync(query, ct));
    }

    [McpServerTool]
    [Description("Get a book's reading progress (page, status, percent complete) by book id. "
        + "Returns null if the book has no progress record.")]
    public async Task<string> GetReadingProgress(
        IReadingProgressService progress,
        AuditLogger audit,
        [Description("The book id")] int bookId,
        CancellationToken ct)
    {
        audit.ToolInvoked("get_reading_progress", $"bookId={bookId}");
        // Reuses the C4 service — its business rules hold over MCP just as over HTTP.
        return Serialize(await progress.GetForBookAsync(bookId, ct));
    }

    [McpServerTool]
    [Description("Add a new book to BookTracker. The author must already exist. Returns the created book "
        + "as JSON, or an error if the author is unknown or input is invalid.")]
    public async Task<string> AddBook(
        IBookService books,
        AuditLogger audit,
        [Description("Book title")] string title,
        [Description("Id of an existing author")] int authorId,
        [Description("Total page count")] int totalPages,
        [Description("ISBN (optional)")] string? isbn,
        [Description("Genre (optional)")] string? genre,
        CancellationToken ct)
    {
        audit.ToolInvoked("add_book", $"title={title}, authorId={authorId}");

        if (string.IsNullOrWhiteSpace(title))
        {
            return "Error: title is required.";
        }

        if (totalPages < 0)
        {
            return "Error: totalPages must be zero or greater.";
        }

        var result = await books.CreateAsync(new CreateBookRequest(title, authorId, isbn, totalPages, genre), ct);
        return result.Status == BookMutationStatus.Success
            ? Serialize(result.Book)
            : $"Error: author {authorId} does not exist.";
    }

    private static string Serialize(object? value) => JsonSerializer.Serialize(value);
}
