using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookTracker.Api.Endpoints;

public static class AuthorEndpoints
{
    public static IEndpointRouteBuilder MapAuthorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/authors").WithTags("Authors");

        group.MapGet("/", async (IAuthorService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(ct)));

        group.MapGet("/{id:int}", async Task<Results<Ok<AuthorDto>, NotFound>> (
            int id, IAuthorService service, CancellationToken ct) =>
        {
            var author = await service.GetByIdAsync(id, ct);
            return author is null ? TypedResults.NotFound() : TypedResults.Ok(author);
        });

        group.MapPost("/", async Task<Created<AuthorDto>> (
            CreateAuthorRequest request, IAuthorService service, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/authors/{created.Id}", created);
        });

        group.MapPut("/{id:int}", async Task<Results<Ok<AuthorDto>, NotFound>> (
            int id, UpdateAuthorRequest request, IAuthorService service, CancellationToken ct) =>
        {
            var updated = await service.UpdateAsync(id, request, ct);
            return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
        });

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound, Conflict<string>>> (
            int id, IAuthorService service, CancellationToken ct) =>
        {
            var status = await service.DeleteAsync(id, ct);
            return status switch
            {
                AuthorDeleteStatus.Deleted => TypedResults.NoContent(),
                AuthorDeleteStatus.NotFound => TypedResults.NotFound(),
                _ => TypedResults.Conflict($"Author {id} has books and cannot be deleted."),
            };
        });

        return app;
    }
}
