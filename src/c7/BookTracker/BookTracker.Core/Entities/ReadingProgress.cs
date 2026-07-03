namespace BookTracker.Core.Entities;

/// <summary>
/// A reader's progress through a single book. One row per book (unique on BookId).
/// TotalPages is not stored here — it derives from <see cref="Book.TotalPages"/>.
/// </summary>
public class ReadingProgress
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public Book? Book { get; set; }

    public int CurrentPage { get; set; }

    public ReadingStatus Status { get; set; } = ReadingStatus.WantToRead;

    public DateOnly? StartedOn { get; set; }

    public DateOnly? FinishedOn { get; set; }
}
