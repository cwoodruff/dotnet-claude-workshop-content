namespace BookTracker.Core.Dtos;

/// <summary>Read model for an author returned across the API boundary.</summary>
public record AuthorDto(int Id, string Name);

public record CreateAuthorRequest(string Name);

public record UpdateAuthorRequest(string Name);
