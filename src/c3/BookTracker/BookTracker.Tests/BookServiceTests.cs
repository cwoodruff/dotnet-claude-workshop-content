using BookTracker.Core.Dtos;
using BookTracker.Core.Entities;
using BookTracker.Core.Interfaces;
using BookTracker.Core.Services;

namespace BookTracker.Tests;

public class BookServiceTests
{
    [Fact]
    public async Task CreateAsync_WithExistingAuthor_MapsRequestAndReturnsDto()
    {
        var books = new InMemoryBookRepository();
        var authors = new InMemoryAuthorRepository();
        await authors.AddAsync(new Author { Name = "Martin Fowler" });
        var service = new BookService(books, authors);

        var result = await service.CreateAsync(
            new CreateBookRequest("Refactoring", AuthorId: 1, "9780134757599", 448, "Software"));

        Assert.Equal(BookMutationStatus.Success, result.Status);
        Assert.NotNull(result.Book);
        Assert.Equal("Refactoring", result.Book!.Title);
        Assert.Equal("Martin Fowler", result.Book.Author.Name);
        Assert.Single(await service.GetAllAsync());
    }

    [Fact]
    public async Task CreateAsync_WithUnknownAuthor_ReturnsAuthorNotFound()
    {
        var service = new BookService(new InMemoryBookRepository(), new InMemoryAuthorRepository());

        var result = await service.CreateAsync(
            new CreateBookRequest("Orphan", AuthorId: 999, null, 100, null));

        Assert.Equal(BookMutationStatus.AuthorNotFound, result.Status);
        Assert.Null(result.Book);
    }

    private sealed class InMemoryBookRepository : IBookRepository
    {
        private readonly List<Book> _books = [];
        private int _nextId = 1;

        public Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Book>>(_books);

        public Task<Book?> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_books.FirstOrDefault(b => b.Id == id));

        public Task<IReadOnlyList<Book>> SearchByTitleAsync(string term, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Book>>(
                _books.Where(b => b.Title.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList());

        public Task AddAsync(Book book, CancellationToken ct = default)
        {
            book.Id = _nextId++;
            _books.Add(book);
            return Task.CompletedTask;
        }

        public Task<bool> UpdateAsync(Book book, CancellationToken ct = default) => Task.FromResult(true);

        public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_books.RemoveAll(b => b.Id == id) > 0);
    }

    private sealed class InMemoryAuthorRepository : IAuthorRepository
    {
        private readonly List<Author> _authors = [];
        private int _nextId = 1;

        public Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Author>>(_authors);

        public Task<Author?> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_authors.FirstOrDefault(a => a.Id == id));

        public Task AddAsync(Author author, CancellationToken ct = default)
        {
            author.Id = _nextId++;
            _authors.Add(author);
            return Task.CompletedTask;
        }

        public Task<bool> UpdateAsync(Author author, CancellationToken ct = default) => Task.FromResult(true);

        public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_authors.RemoveAll(a => a.Id == id) > 0);

        public Task<bool> HasBooksAsync(int authorId, CancellationToken ct = default) => Task.FromResult(false);
    }
}
