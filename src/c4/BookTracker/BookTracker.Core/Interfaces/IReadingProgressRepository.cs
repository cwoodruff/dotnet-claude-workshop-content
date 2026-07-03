using BookTracker.Core.Entities;

namespace BookTracker.Core.Interfaces;

/// <summary>
/// Persistence port for reading progress. Implemented in the Data layer so Core stays free of EF Core.
/// </summary>
public interface IReadingProgressRepository
{
    Task<ReadingProgress?> GetForBookAsync(int bookId, CancellationToken ct = default);

    Task AddAsync(ReadingProgress progress, CancellationToken ct = default);

    Task UpdateAsync(ReadingProgress progress, CancellationToken ct = default);
}
