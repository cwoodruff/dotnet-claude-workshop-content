using BookTracker.Core.Entities;
using BookTracker.Core.Interfaces;
using BookTracker.Core.Services;

namespace BookTracker.Tests;

public class ReviewServiceTests
{
    [Fact]
    public async Task GetForBookAsync_ReturnsReviews_NewestFirst()
    {
        var repo = new InMemoryReviewRepository();
        repo.Add(new Review { Id = 1, BookId = 1, Reviewer = "Dana", Rating = 5, Body = "Old", CreatedOn = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) });
        repo.Add(new Review { Id = 2, BookId = 1, Reviewer = "Lee", Rating = 4, Body = "New", CreatedOn = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero) });
        var service = new ReviewService(repo);

        var result = await service.GetForBookAsync(1);

        Assert.Equal(2, result.Count);
        Assert.Equal("New", result[0].Body);
        Assert.Equal("Old", result[1].Body);
    }

    [Fact]
    public async Task GetForBookAsync_OnlyReturnsReviewsForThatBook()
    {
        var repo = new InMemoryReviewRepository();
        repo.Add(new Review { Id = 1, BookId = 1, Reviewer = "Dana", Rating = 5, Body = "For book 1", CreatedOn = default });
        repo.Add(new Review { Id = 2, BookId = 2, Reviewer = "Sam", Rating = 3, Body = "For book 2", CreatedOn = default });
        var service = new ReviewService(repo);

        var result = await service.GetForBookAsync(1);

        var only = Assert.Single(result);
        Assert.Equal(1, only.BookId);
    }

    [Fact]
    public async Task GetForBookAsync_WithNoReviews_ReturnsEmpty()
    {
        var service = new ReviewService(new InMemoryReviewRepository());

        var result = await service.GetForBookAsync(999);

        Assert.Empty(result);
    }

    private sealed class InMemoryReviewRepository : IReviewRepository
    {
        private readonly List<Review> _reviews = [];

        public void Add(Review review) => _reviews.Add(review);

        public Task<IReadOnlyList<Review>> GetForBookAsync(int bookId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Review>>(
                _reviews.Where(r => r.BookId == bookId)
                    .OrderByDescending(r => r.CreatedOn)
                    .ToList());
    }
}
