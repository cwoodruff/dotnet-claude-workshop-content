namespace BookTracker.Core.Dtos;

/// <summary>Read model returned across the API boundary. Endpoints never return EF entities.</summary>
public record BookDto(
    int Id,
    string Title,
    AuthorDto Author,
    string? Isbn,
    int TotalPages,
    string? Genre,
    DateTimeOffset CreatedAt);

public record CreateBookRequest(
    string Title,
    int AuthorId,
    string? Isbn,
    int TotalPages,
    string? Genre);

public record UpdateBookRequest(
    string Title,
    int AuthorId,
    string? Isbn,
    int TotalPages,
    string? Genre);
