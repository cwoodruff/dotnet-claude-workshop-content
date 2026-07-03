namespace BookTracker.Core.Entities;

/// <summary>
/// A book in the catalog. Reading-progress tracking is added in a later lab.
/// </summary>
public class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int AuthorId { get; set; }

    public Author? Author { get; set; }

    public string? Isbn { get; set; }

    public int TotalPages { get; set; }

    public string? Genre { get; set; }

    /// <summary>A blurb describing the book. The primary text corpus for RAG (Day 2 Lab 3).</summary>
    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
