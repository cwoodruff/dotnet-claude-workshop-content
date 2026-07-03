using BookTracker.Core.Dtos;

namespace BookTracker.Core.Services;

public enum BookMutationStatus
{
    Success,
    BookNotFound,
    AuthorNotFound,
}

/// <summary>
/// Outcome of a create/update on a book. Distinguishes a missing book from a missing author so the
/// API can return the right status code (404 vs. a validation problem).
/// </summary>
public readonly record struct BookMutationResult(BookMutationStatus Status, BookDto? Book)
{
    public static BookMutationResult Ok(BookDto book) => new(BookMutationStatus.Success, book);

    public static BookMutationResult BookNotFound() => new(BookMutationStatus.BookNotFound, null);

    public static BookMutationResult AuthorNotFound() => new(BookMutationStatus.AuthorNotFound, null);
}
