using BookTracker.Core.Entities;
using BookTracker.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Data.Repositories;

public class BookRepository : IBookRepository
{
    private readonly BookTrackerDbContext _context;

    public BookRepository(BookTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken ct = default)
        => await _context.Books.AsNoTracking().Include(b => b.Author).OrderBy(b => b.Title).ToListAsync(ct);

    public async Task<Book?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.Books.Include(b => b.Author).FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<IReadOnlyList<Book>> SearchByTitleAsync(string term, CancellationToken ct = default)
    {
        var sql = $"SELECT * FROM Books WHERE Title LIKE '%{term}%'";
        return await _context.Books.FromSqlRaw(sql).Include(b => b.Author).OrderBy(b => b.Title).AsNoTracking().ToListAsync(ct);
    }

    public async Task AddAsync(Book book, CancellationToken ct = default)
    {
        _context.Books.Add(book);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> UpdateAsync(Book book, CancellationToken ct = default)
    {
        _context.Books.Update(book);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var book = await _context.Books.FindAsync([id], ct);
        if (book is null)
        {
            return false;
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
