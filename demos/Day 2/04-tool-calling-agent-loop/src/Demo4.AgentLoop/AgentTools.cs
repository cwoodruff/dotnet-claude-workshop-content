using System.Text.Json;
using Anthropic.Models.Messages;

namespace Demo4.AgentLoop;

/// <summary>
/// The agent's tool surface: static definitions (name + description + JSON input schema) plus a
/// dispatch method. The description is what makes a tool reliable — write it like a skill
/// description and say <em>when</em> to use it, not just what it does.
/// </summary>
public class AgentTools
{
    private readonly BookStore _store;

    public AgentTools(BookStore store) => _store = store;

    public static readonly Tool FindBook = new()
    {
        Name = "find_book",
        Description = "Search the BookTracker catalog for books whose title matches a query. "
            + "Use this whenever the user names a specific book, to look up the book (and its id) "
            + "before acting on it — for example before updating its reading progress.",
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

    public static readonly Tool UpdateReadingProgress = new()
    {
        Name = "update_reading_progress",
        Description = "Update a book's reading progress (status and optionally current page). "
            + "Use when the user wants to record progress or mark a book want-to-read, reading, or "
            + "completed. Requires the numeric book id — call find_book first if you only have a title. "
            + "Status must be one of WantToRead, Reading, Completed.",
        InputSchema = new()
        {
            Properties = new Dictionary<string, JsonElement>
            {
                ["bookId"] = JsonSerializer.SerializeToElement(
                    new { type = "integer", description = "Book id (from find_book)" }),
                ["status"] = JsonSerializer.SerializeToElement(
                    new { type = "string", @enum = new[] { "WantToRead", "Reading", "Completed" } }),
                ["currentPage"] = JsonSerializer.SerializeToElement(
                    new { type = "integer", description = "Current page (optional)" }),
            },
            Required = ["bookId", "status"],
        },
    };

    /// <summary>All tool definitions exposed to the model.</summary>
    public static readonly IReadOnlyList<Tool> Definitions = [FindBook, UpdateReadingProgress];

    /// <summary>Dispatch a tool call by name. Returns a JSON string the model reads as the tool_result.</summary>
    public Task<string> ExecuteAsync(
        string name,
        IReadOnlyDictionary<string, JsonElement> input,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult(name switch
        {
            "find_book" => JsonSerializer.Serialize(
                _store.Search(input["query"].GetString() ?? string.Empty)),
            "update_reading_progress" => JsonSerializer.Serialize(
                _store.UpdateProgress(
                    input["bookId"].GetInt32(),
                    input["status"].GetString() ?? string.Empty,
                    input.TryGetValue("currentPage", out var cp) ? cp.GetInt32() : null)),
            _ => throw new InvalidOperationException($"Unknown tool: {name}"),
        });
    }
}
