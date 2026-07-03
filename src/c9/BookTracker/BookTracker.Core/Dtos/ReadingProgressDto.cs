namespace BookTracker.Core.Dtos;

/// <summary>Read model returned across the API boundary. Endpoints never return EF entities.</summary>
public record ReadingProgressDto(
    int BookId,
    int CurrentPage,
    int TotalPages,
    string Status,
    DateOnly? StartedOn,
    DateOnly? FinishedOn,
    int PercentComplete);

/// <summary>Update payload. Dates are derived by the rules, not client-supplied.</summary>
public record UpdateReadingProgressRequest(
    int CurrentPage,
    string Status);
