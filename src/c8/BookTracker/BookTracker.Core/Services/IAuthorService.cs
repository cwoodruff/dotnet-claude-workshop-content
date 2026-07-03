using BookTracker.Core.Dtos;

namespace BookTracker.Core.Services;

public enum AuthorDeleteStatus
{
    Deleted,
    NotFound,
    HasBooks,
}

public interface IAuthorService
{
    Task<IReadOnlyList<AuthorDto>> GetAllAsync(CancellationToken ct = default);

    Task<AuthorDto?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<AuthorDto> CreateAsync(CreateAuthorRequest request, CancellationToken ct = default);

    Task<AuthorDto?> UpdateAsync(int id, UpdateAuthorRequest request, CancellationToken ct = default);

    Task<AuthorDeleteStatus> DeleteAsync(int id, CancellationToken ct = default);
}
