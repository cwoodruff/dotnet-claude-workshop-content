using BookTracker.Core.Dtos;

namespace BookTracker.Core.Services;

public interface IReviewService
{
    /// <summary>Reviews for a book, newest first.</summary>
    Task<IReadOnlyList<ReviewDto>> GetForBookAsync(int bookId, CancellationToken ct = default);
}
