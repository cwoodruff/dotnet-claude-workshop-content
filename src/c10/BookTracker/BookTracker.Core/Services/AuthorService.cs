using BookTracker.Core.Dtos;
using BookTracker.Core.Entities;
using BookTracker.Core.Interfaces;

namespace BookTracker.Core.Services;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _repository;

    public AuthorService(IAuthorRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AuthorDto>> GetAllAsync(CancellationToken ct = default)
    {
        var authors = await _repository.GetAllAsync(ct);
        return authors.Select(ToDto).ToList();
    }

    public async Task<AuthorDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var author = await _repository.GetByIdAsync(id, ct);
        return author is null ? null : ToDto(author);
    }

    public async Task<AuthorDto> CreateAsync(CreateAuthorRequest request, CancellationToken ct = default)
    {
        var author = new Author { Name = request.Name };
        await _repository.AddAsync(author, ct);
        return ToDto(author);
    }

    public async Task<AuthorDto?> UpdateAsync(int id, UpdateAuthorRequest request, CancellationToken ct = default)
    {
        var author = await _repository.GetByIdAsync(id, ct);
        if (author is null)
        {
            return null;
        }

        author.Name = request.Name;
        await _repository.UpdateAsync(author, ct);
        return ToDto(author);
    }

    public async Task<AuthorDeleteStatus> DeleteAsync(int id, CancellationToken ct = default)
    {
        var author = await _repository.GetByIdAsync(id, ct);
        if (author is null)
        {
            return AuthorDeleteStatus.NotFound;
        }

        if (await _repository.HasBooksAsync(id, ct))
        {
            return AuthorDeleteStatus.HasBooks;
        }

        await _repository.DeleteAsync(id, ct);
        return AuthorDeleteStatus.Deleted;
    }

    private static AuthorDto ToDto(Author author) => new(author.Id, author.Name);
}
