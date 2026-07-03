using BookTracker.Core.Entities;

namespace BookTracker.Core.Interfaces;

/// <summary>
/// Persistence port for reviews. Implemented in the Data layer so Core stays free of EF Core.
/// </summary>
public interface IReviewRepository
{
    Task<IReadOnlyList<Review>> GetForBookAsync(int bookId, CancellationToken ct = default);
}
