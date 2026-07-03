using BookTracker.Core.Entities;
using BookTracker.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Data.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly BookTrackerDbContext _context;

    public ReviewRepository(BookTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Review>> GetForBookAsync(int bookId, CancellationToken ct = default)
    {
        // SQLite can't ORDER BY a DateTimeOffset, so sort newest-first client-side after the query.
        var reviews = await _context.Reviews
            .AsNoTracking()
            .Where(r => r.BookId == bookId)
            .ToListAsync(ct);

        return reviews.OrderByDescending(r => r.CreatedOn).ToList();
    }
}
