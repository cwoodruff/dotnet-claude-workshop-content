namespace BookTracker.Core.Dtos;

/// <summary>Read model returned across the API boundary. Endpoints never return EF entities.</summary>
public record ReviewDto(
    int Id,
    int BookId,
    string Reviewer,
    int Rating,
    string Body,
    DateTimeOffset CreatedOn);
