using BookTracker.Core.Dtos;
using BookTracker.Core.Entities;
using BookTracker.Core.Interfaces;

namespace BookTracker.Core.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviews;

    public ReviewService(IReviewRepository reviews)
    {
        _reviews = reviews;
    }

    public async Task<IReadOnlyList<ReviewDto>> GetForBookAsync(int bookId, CancellationToken ct = default)
    {
        var reviews = await _reviews.GetForBookAsync(bookId, ct);
        return reviews.Select(ToDto).ToList();
    }

    private static ReviewDto ToDto(Review review) => new(
        review.Id,
        review.BookId,
        review.Reviewer,
        review.Rating,
        review.Body,
        review.CreatedOn);
}
