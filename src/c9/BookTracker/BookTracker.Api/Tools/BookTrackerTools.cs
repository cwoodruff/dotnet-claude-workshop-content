using System.Text.Json;
using Anthropic.Models.Messages;
using BookTracker.Core.Dtos;
using BookTracker.Core.Services;

namespace BookTracker.Api.Tools;

/// <summary>
/// The agent's tool surface. Definitions are static (no dependencies); dispatch is an instance method
/// that calls the real Core services — kept separable from the agent loop so it's unit-testable
/// without the SDK (matters at C9).
/// </summary>
public class BookTrackerTools
{
    private readonly IBookService _books;
    private readonly IReadingProgressService _progress;

    public BookTrackerTools(IBookService books, IReadingProgressService progress)
    {
        _books = books;
        _progress = progress;
    }

    public static readonly Tool FindBook = new()
    {
        Name = "find_book",
        Description = "Search the BookTracker catalog for books whose title matches a query. "
            + "Use this to look up a book (and its id) before acting on it.",
        InputSchema = new()
        {
            Properties = new Dictionary<string, JsonElement>
            {
                ["query"] = JsonSerializer.SerializeToElement(
                    new { type = "string", description = "Text to match against book titles" }),
            },
            Required = ["query"],
        },
    };

    public static readonly Tool GetReadingProgress = new()
    {
        Name = "get_reading_progress",
        Description = "Get a book's current reading progress (page, status, percent complete). "
            + "Use when the user asks how far along they are in a book.",
        InputSchema = new()
        {
            Properties = new Dictionary<string, JsonElement>
            {
                ["bookId"] = JsonSerializer.SerializeToElement(new { type = "integer", description = "Book id" }),
            },
            Required = ["bookId"],
        },
    };

    public static readonly Tool UpdateReadingProgress = new()
    {
        Name = "update_reading_progress",
        Description = "Update a book's reading progress (current page and status). Use when the user "
            + "wants to record progress or mark a book reading/completed/want-to-read. Status must be "
            + "one of WantToRead, Reading, Completed, and only forward transitions are allowed.",
        InputSchema = new()
        {
            Properties = new Dictionary<string, JsonElement>
            {
                ["bookId"] = JsonSerializer.SerializeToElement(new { type = "integer", description = "Book id" }),
                ["currentPage"] = JsonSerializer.SerializeToElement(
                    new { type = "integer", description = "Current page (optional; defaults to 0)" }),
                ["status"] = JsonSerializer.SerializeToElement(
                    new { type = "string", @enum = new[] { "WantToRead", "Reading", "Completed" } }),
            },
            Required = ["bookId", "status"],
        },
    };

    /// <summary>All tool definitions exposed to the model.</summary>
    public static readonly IReadOnlyList<Tool> Definitions = [FindBook, GetReadingProgress, UpdateReadingProgress];

    /// <summary>Dispatch a tool call by name to the matching Core service. Returns a JSON string result.</summary>
    public async Task<string> ExecuteAsync(
        string name,
        IReadOnlyDictionary<string, JsonElement> input,
        CancellationToken ct = default)
        => name switch
        {
            "find_book" => Serialize(await _books.SearchAsync(input["query"].GetString() ?? string.Empty, ct)),
            "get_reading_progress" => Serialize(await _progress.GetForBookAsync(input["bookId"].GetInt32(), ct)),
            "update_reading_progress" => Serialize(await _progress.UpdateAsync(
                input["bookId"].GetInt32(),
                new UpdateReadingProgressRequest(
                    input.TryGetValue("currentPage", out var cp) ? cp.GetInt32() : 0,
                    input["status"].GetString() ?? string.Empty),
                ct)),
            _ => $"Unknown tool: {name}",
        };

    private static string Serialize(object? value) => JsonSerializer.Serialize(value);
}
