namespace BookTracker.Core.Entities;

/// <summary>
/// Where a reader is with a book. Forward-only: WantToRead → Reading → Completed.
/// </summary>
public enum ReadingStatus
{
    WantToRead,
    Reading,
    Completed,
}
