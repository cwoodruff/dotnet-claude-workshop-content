namespace Demo6.McpServer.Services;

/// <summary>A book in the demo catalog. A record because it crosses the tool boundary as a DTO.</summary>
public record Book(int Id, string Title, string Author, string Genre, int Year, int TotalPages, string Summary);

/// <summary>
/// The service the MCP tools delegate to. Tools never touch the data store directly —
/// this is the "thin governed gateway" shape from Section 4, minus the database.
/// </summary>
public interface IBookCatalog
{
    IReadOnlyList<Book> Search(string query);
    Book? GetById(int id);
    Book Add(string title, string author, string genre, int totalPages);
}

/// <summary>
/// In-memory stand-in for BookTracker's SQLite database, seeded so the demo prompt
/// "Search BookTracker for books about space" returns several hits with no setup.
/// </summary>
public class BookCatalog : IBookCatalog
{
    private readonly Lock _gate = new();
    private readonly List<Book> _books =
    [
        new(1, "The Martian", "Andy Weir", "Science Fiction", 2011, 369,
            "An astronaut stranded alone on Mars must science his way to survival in space."),
        new(2, "Project Hail Mary", "Andy Weir", "Science Fiction", 2021, 476,
            "A lone crew member wakes in deep space on a last-chance mission to save Earth."),
        new(3, "Dune", "Frank Herbert", "Science Fiction", 1965, 412,
            "Political intrigue and ecology collide on the desert planet Arrakis."),
        new(4, "Packing for Mars", "Mary Roach", "Nonfiction", 2010, 334,
            "The curious science of life in the void: what space travel does to the human body."),
        new(5, "A Brief History of Time", "Stephen Hawking", "Science", 1988, 212,
            "From the Big Bang to black holes — the universe, space, and time explained."),
        new(6, "Cosmos", "Carl Sagan", "Science", 1980, 396,
            "A grand tour of space, the stars, and humanity's place among them."),
        new(7, "The Pragmatic Programmer", "David Thomas & Andrew Hunt", "Programming", 1999, 352,
            "Timeless practices and mindsets for effective software craftsmanship."),
        new(8, "Pride and Prejudice", "Jane Austen", "Classic", 1813, 432,
            "Elizabeth Bennet and Mr. Darcy navigate manners, marriage, and misjudgment."),
        new(9, "The Hobbit", "J.R.R. Tolkien", "Fantasy", 1937, 310,
            "Bilbo Baggins is swept into a quest for a dragon-guarded treasure."),
        new(10, "Educated", "Tara Westover", "Memoir", 2018, 334,
            "A memoir of self-invention: from a survivalist childhood to a Cambridge PhD."),
    ];

    public IReadOnlyList<Book> Search(string query)
    {
        lock (_gate)
        {
            return _books
                .Where(b => b.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || b.Author.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || b.Genre.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || b.Summary.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    public Book? GetById(int id)
    {
        lock (_gate)
        {
            return _books.FirstOrDefault(b => b.Id == id);
        }
    }

    public Book Add(string title, string author, string genre, int totalPages)
    {
        lock (_gate)
        {
            var book = new Book(_books.Max(b => b.Id) + 1, title, author, genre,
                DateTime.UtcNow.Year, totalPages, "Added during the live demo.");
            _books.Add(book);
            return book;
        }
    }
}
