using BookTracker.Core.Entities;
using BookTracker.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Data.Repositories;

public class AuthorRepository : IAuthorRepository
{
    private readonly BookTrackerDbContext _context;

    public AuthorRepository(BookTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken ct = default)
        => await _context.Authors.AsNoTracking().OrderBy(a => a.Name).ToListAsync(ct);

    public async Task<Author?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.Authors.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task AddAsync(Author author, CancellationToken ct = default)
    {
        _context.Authors.Add(author);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> UpdateAsync(Author author, CancellationToken ct = default)
    {
        _context.Authors.Update(author);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var author = await _context.Authors.FindAsync([id], ct);
        if (author is null)
        {
            return false;
        }

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public Task<bool> HasBooksAsync(int authorId, CancellationToken ct = default)
        => _context.Books.AnyAsync(b => b.AuthorId == authorId, ct);
}
