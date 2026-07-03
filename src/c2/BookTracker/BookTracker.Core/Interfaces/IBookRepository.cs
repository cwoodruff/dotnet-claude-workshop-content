using BookTracker.Core.Entities;

namespace BookTracker.Core.Interfaces;

/// <summary>
/// Persistence port for books. Implemented in the Data layer so Core stays free of EF Core.
/// </summary>
public interface IBookRepository
{
    Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken ct = default);

    Task<Book?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Book>> SearchByTitleAsync(string term, CancellationToken ct = default);

    Task AddAsync(Book book, CancellationToken ct = default);

    Task<bool> UpdateAsync(Book book, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
