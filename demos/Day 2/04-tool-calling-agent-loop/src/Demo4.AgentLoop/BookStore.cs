namespace Demo4.AgentLoop;

/// <summary>A book in the in-memory catalog. Domain entities are classes; status mutates.</summary>
public class Book
{
    public required int Id { get; init; }

    public required string Title { get; init; }

    public required string Author { get; init; }

    public required int TotalPages { get; init; }

    public int CurrentPage { get; set; }

    /// <summary>One of WantToRead, Reading, Completed.</summary>
    public string Status { get; set; } = "WantToRead";
}

/// <summary>
/// A tiny in-memory "database" so the demo needs no EF Core, no SQLite, no setup. The agent's tools
/// read and write this list — the point of the demo is the loop, not the persistence.
/// </summary>
public class BookStore
{
    private static readonly string[] ValidStatuses = ["WantToRead", "Reading", "Completed"];

    private readonly List<Book> _books =
    [
        new() { Id = 1, Title = "Dune", Author = "Frank Herbert", TotalPages = 412 },
        new() { Id = 2, Title = "Project Hail Mary", Author = "Andy Weir", TotalPages = 476, Status = "Reading", CurrentPage = 120 },
        new() { Id = 3, Title = "The Left Hand of Darkness", Author = "Ursula K. Le Guin", TotalPages = 304 },
        new() { Id = 4, Title = "Foundation", Author = "Isaac Asimov", TotalPages = 255, Status = "Completed", CurrentPage = 255 },
        new() { Id = 5, Title = "A Memory Called Empire", Author = "Arkady Martine", TotalPages = 462 },
    ];

    public IReadOnlyList<Book> Search(string query) =>
        _books.Where(b => b.Title.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

    public Book UpdateProgress(int bookId, string status, int? currentPage)
    {
        if (!ValidStatuses.Contains(status))
        {
            throw new ArgumentException(
                $"Invalid status '{status}'. Must be one of: {string.Join(", ", ValidStatuses)}.");
        }

        var book = _books.FirstOrDefault(b => b.Id == bookId)
            ?? throw new KeyNotFoundException($"No book with id {bookId}. Use find_book to get a valid id.");

        book.Status = status;
        book.CurrentPage = status == "Completed" ? book.TotalPages : currentPage ?? book.CurrentPage;
        return book;
    }
}
