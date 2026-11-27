using System.ComponentModel;
using System.Text.Json;
using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using BookTracker.Mcp.Security;
using ModelContextProtocol.Protocol;
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
    [Description("Add a new book to BookTracker. The author must already exist. The user is asked to "
        + "confirm before anything is written. Returns the created book as JSON, or an error if the "
        + "author is unknown, input is invalid, or the user declines.")]
    public async Task<string> AddBook(
        McpServer server,
        RequestContext<CallToolRequestParams> context,
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

        // A write is the one call that needs a human yes. First pass: ask the user through the client
        // and stop. Second pass (the client retries with the answer): write only on an explicit yes.
        var confirmed = ReadConfirmation(context);
        if (confirmed is null)
        {
            if (!server.IsMrtrSupported && server.ClientCapabilities?.Elicitation is null)
            {
                audit.WriteDeclined("add_book", $"title={title}, authorId={authorId}, client cannot confirm");
                return "Not added: this MCP client can't ask the user to confirm a write, so nothing was written.";
            }

            throw ConfirmationRequired($"Add \"{title}\" ({totalPages} pages, author id {authorId}) to BookTracker?");
        }

        if (confirmed == false)
        {
            audit.WriteDeclined("add_book", $"title={title}, authorId={authorId}");
            return "Not added: the user did not confirm, so nothing was written.";
        }

        audit.WriteConfirmed("add_book", $"title={title}, authorId={authorId}");
        var result = await books.CreateAsync(new CreateBookRequest(title, authorId, isbn, totalPages, genre), ct);
        return result.Status == BookMutationStatus.Success
            ? Serialize(result.Book)
            : $"Error: author {authorId} does not exist.";
    }

    private const string ConfirmKey = "confirm_write";

    /// <summary>
    /// Multi round-trip request (MRTR): returns an input-required result that asks the client to show
    /// the user a yes/no form, then call the tool again with the answer. Works on stateless HTTP; for
    /// older clients the SDK sends the same question as a classic elicitation request.
    /// </summary>
    private static InputRequiredException ConfirmationRequired(string message)
        => new(
            new Dictionary<string, InputRequest>
            {
                [ConfirmKey] = InputRequest.ForElicitation(new ElicitRequestParams
                {
                    Message = message,
                    RequestedSchema = new()
                    {
                        Properties = new Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition>
                        {
                            ["confirm"] = new ElicitRequestParams.BooleanSchema { Description = "Add this book?" },
                        },
                        Required = ["confirm"],
                    },
                }),
            },
            null);

    /// <summary>
    /// The user's answer from the retried call: null when the user hasn't been asked yet, true only for
    /// an accepted form with confirm = true. Decline, cancel and anything else are false (fail closed).
    /// </summary>
    private static bool? ReadConfirmation(RequestContext<CallToolRequestParams> context)
    {
        if (context.Params?.InputResponses is not { } responses
            || !responses.TryGetValue(ConfirmKey, out var response))
        {
            return null;
        }

        var result = response.Deserialize(InputResponse.ElicitResultJsonTypeInfo);
        return result is { IsAccepted: true }
            && result.Content is { } content
            && content.TryGetValue("confirm", out var value)
            && value.ValueKind == JsonValueKind.True;
    }

    private static string Serialize(object? value) => JsonSerializer.Serialize(value);
}
