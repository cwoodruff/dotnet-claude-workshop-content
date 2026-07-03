using BookTracker.Core.Dtos;

namespace BookTracker.Core.Services;

public interface IBookService
{
    Task<IReadOnlyList<BookDto>> GetAllAsync(CancellationToken ct = default);

    Task<BookDto?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<BookDto>> SearchAsync(string term, CancellationToken ct = default);

    Task<BookMutationResult> CreateAsync(CreateBookRequest request, CancellationToken ct = default);

    Task<BookMutationResult> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
