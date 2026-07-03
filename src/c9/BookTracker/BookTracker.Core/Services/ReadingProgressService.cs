using System.ComponentModel.DataAnnotations;
using BookTracker.Core.Domain;
using BookTracker.Core.Dtos;
using BookTracker.Core.Entities;
using BookTracker.Core.Interfaces;

namespace BookTracker.Core.Services;

public class ReadingProgressService : IReadingProgressService
{
    private readonly IReadingProgressRepository _progress;
    private readonly IBookRepository _books;

    public ReadingProgressService(IReadingProgressRepository progress, IBookRepository books)
    {
        _progress = progress;
        _books = books;
    }

    public async Task<ReadingProgressDto?> GetForBookAsync(int bookId, CancellationToken ct = default)
    {
        var book = await _books.GetByIdAsync(bookId, ct);
        if (book is null)
        {
            return null;
        }

        var progress = await _progress.GetForBookAsync(bookId, ct);
        return progress is null ? null : ToDto(progress, book.TotalPages);
    }

    public async Task<ReadingProgressDto> UpdateAsync(int bookId, UpdateReadingProgressRequest request, CancellationToken ct = default)
    {
        var book = await _books.GetByIdAsync(bookId, ct)
            ?? throw new KeyNotFoundException($"Book {bookId} does not exist.");

        if (!Enum.TryParse<ReadingStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new ValidationException($"Unknown status '{request.Status}'.");
        }

        var progress = await _progress.GetForBookAsync(bookId, ct);
        var isNew = progress is null;
        progress ??= new ReadingProgress { BookId = bookId, Status = ReadingStatus.WantToRead };

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = ReadingProgressRules.Apply(progress, request.CurrentPage, status, book.TotalPages, today);
        if (!result.Ok)
        {
            throw new ValidationException(result.Error);
        }

        if (isNew)
        {
            await _progress.AddAsync(progress, ct);
        }
        else
        {
            await _progress.UpdateAsync(progress, ct);
        }

        return ToDto(progress, book.TotalPages);
    }

    private static ReadingProgressDto ToDto(ReadingProgress progress, int totalPages) => new(
        progress.BookId,
        progress.CurrentPage,
        totalPages,
        progress.Status.ToString(),
        progress.StartedOn,
        progress.FinishedOn,
        totalPages == 0 ? 0 : (int)Math.Round(100.0 * progress.CurrentPage / totalPages));
}
