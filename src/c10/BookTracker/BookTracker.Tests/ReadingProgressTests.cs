using BookTracker.Core.Domain;
using BookTracker.Core.Dtos;
using BookTracker.Core.Entities;
using BookTracker.Core.Interfaces;
using BookTracker.Core.Services;

namespace BookTracker.Tests;

public class ReadingProgressTests
{
    private static readonly DateOnly Today = new(2025, 6, 1);
    private const int TotalPages = 300;

    private static ReadingProgress New(ReadingStatus status = ReadingStatus.WantToRead, int page = 0)
        => new() { BookId = 1, Status = status, CurrentPage = page };

    [Fact]
    public void Apply_ValidInRangePage_Succeeds()
    {
        var progress = New(ReadingStatus.Reading);
        progress.StartedOn = new DateOnly(2025, 5, 1);

        var result = ReadingProgressRules.Apply(progress, 150, ReadingStatus.Reading, TotalPages, Today);

        Assert.True(result.Ok);
        Assert.Equal(150, progress.CurrentPage);
    }

    [Fact]
    public void Apply_PageOverTotal_Rejected()
    {
        var progress = New(ReadingStatus.Reading);

        var result = ReadingProgressRules.Apply(progress, TotalPages + 1, ReadingStatus.Reading, TotalPages, Today);

        Assert.False(result.Ok);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void Apply_NegativePage_Rejected()
    {
        var progress = New(ReadingStatus.Reading);

        var result = ReadingProgressRules.Apply(progress, -1, ReadingStatus.Reading, TotalPages, Today);

        Assert.False(result.Ok);
    }

    [Fact]
    public void Apply_WantToReadToReading_SetsStartedOn()
    {
        var progress = New(ReadingStatus.WantToRead);

        var result = ReadingProgressRules.Apply(progress, 10, ReadingStatus.Reading, TotalPages, Today);

        Assert.True(result.Ok);
        Assert.Equal(Today, progress.StartedOn);
        Assert.Equal(ReadingStatus.Reading, progress.Status);
    }

    [Fact]
    public void Apply_ReadingToCompleted_SetsFinishedOn_AndFillsPages()
    {
        var progress = New(ReadingStatus.Reading);
        progress.StartedOn = new DateOnly(2025, 5, 1);

        var result = ReadingProgressRules.Apply(progress, 50, ReadingStatus.Completed, TotalPages, Today);

        Assert.True(result.Ok);
        Assert.Equal(Today, progress.FinishedOn);
        Assert.True(progress.FinishedOn >= progress.StartedOn);
        Assert.Equal(TotalPages, progress.CurrentPage);
    }

    [Fact]
    public void Apply_WantToReadToCompleted_Skip_Rejected()
    {
        var progress = New(ReadingStatus.WantToRead);

        var result = ReadingProgressRules.Apply(progress, 0, ReadingStatus.Completed, TotalPages, Today);

        Assert.False(result.Ok);
    }

    [Fact]
    public void Apply_CompletedToReading_Rejected()
    {
        // The regression test that guards the Lab 4 Part C bug: Completed is terminal.
        var progress = New(ReadingStatus.Completed, TotalPages);

        var result = ReadingProgressRules.Apply(progress, 100, ReadingStatus.Reading, TotalPages, Today);

        Assert.False(result.Ok);
        Assert.Equal(ReadingStatus.Completed, progress.Status);
    }

    [Fact]
    public async Task GetForBookAsync_NoRecord_ReturnsNull()
    {
        var books = new InMemoryBookRepository();
        await books.AddAsync(new Book { Title = "Test", TotalPages = TotalPages });
        var service = new ReadingProgressService(new InMemoryReadingProgressRepository(), books);

        var result = await service.GetForBookAsync(1);

        Assert.Null(result);
    }

    private sealed class InMemoryReadingProgressRepository : IReadingProgressRepository
    {
        private readonly List<ReadingProgress> _items = [];

        public Task<ReadingProgress?> GetForBookAsync(int bookId, CancellationToken ct = default)
            => Task.FromResult(_items.FirstOrDefault(p => p.BookId == bookId));

        public Task AddAsync(ReadingProgress progress, CancellationToken ct = default)
        {
            _items.Add(progress);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ReadingProgress progress, CancellationToken ct = default) => Task.CompletedTask;
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
            => Task.FromResult<IReadOnlyList<Book>>(_books);

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
}
