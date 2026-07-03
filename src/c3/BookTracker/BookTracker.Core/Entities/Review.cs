namespace BookTracker.Core.Entities;

/// <summary>
/// A reader's review of a book. One book can have many reviews (one-to-many).
/// </summary>
public class Review
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public Book? Book { get; set; }

    public string Reviewer { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Body { get; set; } = string.Empty;

    public DateTimeOffset CreatedOn { get; set; }
}
