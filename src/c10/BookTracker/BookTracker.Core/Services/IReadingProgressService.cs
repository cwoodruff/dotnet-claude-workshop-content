using BookTracker.Core.Dtos;

namespace BookTracker.Core.Services;

/// <summary>
/// Reading-progress operations. This is the Day 2 integration surface: the agent tool
/// <c>update_reading_progress</c> (C6) and the MCP tool <c>get_reading_progress</c> (C8) call these
/// methods, so keep the signatures stable.
/// </summary>
public interface IReadingProgressService
{
    /// <summary>The book's progress, or <c>null</c> if the book or its progress record doesn't exist.</summary>
    Task<ReadingProgressDto?> GetForBookAsync(int bookId, CancellationToken ct = default);

    /// <summary>
    /// Apply an update (creating the record on first write). Throws
    /// <see cref="System.ComponentModel.DataAnnotations.ValidationException"/> on a rule violation and
    /// <see cref="KeyNotFoundException"/> if the book doesn't exist.
    /// </summary>
    Task<ReadingProgressDto> UpdateAsync(int bookId, UpdateReadingProgressRequest request, CancellationToken ct = default);
}
