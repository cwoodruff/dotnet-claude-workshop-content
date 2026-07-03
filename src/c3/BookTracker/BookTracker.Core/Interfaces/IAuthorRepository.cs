using BookTracker.Core.Entities;

namespace BookTracker.Core.Interfaces;

/// <summary>
/// Persistence port for authors. Implemented in the Data layer so Core stays free of EF Core.
/// </summary>
public interface IAuthorRepository
{
    Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken ct = default);

    Task<Author?> GetByIdAsync(int id, CancellationToken ct = default);

    Task AddAsync(Author author, CancellationToken ct = default);

    Task<bool> UpdateAsync(Author author, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    Task<bool> HasBooksAsync(int authorId, CancellationToken ct = default);
}
