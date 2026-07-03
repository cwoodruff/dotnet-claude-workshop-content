using BookTracker.Core.Entities;
using BookTracker.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Data.Repositories;

public class ReadingProgressRepository : IReadingProgressRepository
{
    private readonly BookTrackerDbContext _context;

    public ReadingProgressRepository(BookTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<ReadingProgress?> GetForBookAsync(int bookId, CancellationToken ct = default)
        => await _context.ReadingProgress.FirstOrDefaultAsync(p => p.BookId == bookId, ct);

    public async Task AddAsync(ReadingProgress progress, CancellationToken ct = default)
    {
        _context.ReadingProgress.Add(progress);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ReadingProgress progress, CancellationToken ct = default)
    {
        _context.ReadingProgress.Update(progress);
        await _context.SaveChangesAsync(ct);
    }
}
