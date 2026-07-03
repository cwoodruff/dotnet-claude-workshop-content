using BookTracker.Core.Dtos;
using BookTracker.Core.Entities;
using BookTracker.Core.Interfaces;

namespace BookTracker.Core.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _books;
    private readonly IAuthorRepository _authors;

    public BookService(IBookRepository books, IAuthorRepository authors)
    {
        _books = books;
        _authors = authors;
    }

    public async Task<IReadOnlyList<BookDto>> GetAllAsync(CancellationToken ct = default)
    {
        var books = await _books.GetAllAsync(ct);
        return books.Select(ToDto).ToList();
    }

    public async Task<BookDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var book = await _books.GetByIdAsync(id, ct);
        return book is null ? null : ToDto(book);
    }

    public async Task<IReadOnlyList<BookDto>> SearchAsync(string term, CancellationToken ct = default)
    {
        var books = await _books.SearchByTitleAsync(term, ct);
        return books.Select(ToDto).ToList();
    }

    public async Task<BookMutationResult> CreateAsync(CreateBookRequest request, CancellationToken ct = default)
    {
        var author = await _authors.GetByIdAsync(request.AuthorId, ct);
        if (author is null)
        {
            return BookMutationResult.AuthorNotFound();
        }

        var book = new Book
        {
            Title = request.Title,
            AuthorId = author.Id,
            Author = author,
            Isbn = request.Isbn,
            TotalPages = request.TotalPages,
            Genre = request.Genre,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        await _books.AddAsync(book, ct);
        return BookMutationResult.Ok(ToDto(book));
    }

    public async Task<BookMutationResult> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct = default)
    {
        var book = await _books.GetByIdAsync(id, ct);
        if (book is null)
        {
            return BookMutationResult.BookNotFound();
        }

        var author = await _authors.GetByIdAsync(request.AuthorId, ct);
        if (author is null)
        {
            return BookMutationResult.AuthorNotFound();
        }

        book.Title = request.Title;
        book.AuthorId = author.Id;
        book.Author = author;
        book.Isbn = request.Isbn;
        book.TotalPages = request.TotalPages;
        book.Genre = request.Genre;

        await _books.UpdateAsync(book, ct);
        return BookMutationResult.Ok(ToDto(book));
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        => _books.DeleteAsync(id, ct);

    private static BookDto ToDto(Book book) => new(
        book.Id,
        book.Title,
        new AuthorDto(book.Author!.Id, book.Author.Name),
        book.Isbn,
        book.TotalPages,
        book.Genre,
        book.CreatedAt);
}
