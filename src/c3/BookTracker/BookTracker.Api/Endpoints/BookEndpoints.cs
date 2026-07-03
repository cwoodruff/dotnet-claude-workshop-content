using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookTracker.Api.Endpoints;

public static class BookEndpoints
{
    public static IEndpointRouteBuilder MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/", async (IBookService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(ct)));

        group.MapGet("/search", async (string q, IBookService service, CancellationToken ct) =>
            TypedResults.Ok(await service.SearchAsync(q, ct)));

        group.MapGet("/{id:int}", async Task<Results<Ok<BookDto>, NotFound>> (
            int id, IBookService service, CancellationToken ct) =>
        {
            var book = await service.GetByIdAsync(id, ct);
            return book is null ? TypedResults.NotFound() : TypedResults.Ok(book);
        });

        group.MapPost("/", async Task<Results<Created<BookDto>, ValidationProblem>> (
            CreateBookRequest request, IBookService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return result.Status switch
            {
                BookMutationStatus.Success => TypedResults.Created($"/api/books/{result.Book!.Id}", result.Book),
                _ => AuthorNotFoundProblem(request.AuthorId),
            };
        });

        group.MapPut("/{id:int}", async Task<Results<Ok<BookDto>, NotFound, ValidationProblem>> (
            int id, UpdateBookRequest request, IBookService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result.Status switch
            {
                BookMutationStatus.Success => TypedResults.Ok(result.Book!),
                BookMutationStatus.BookNotFound => TypedResults.NotFound(),
                _ => AuthorNotFoundProblem(request.AuthorId),
            };
        });

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id, IBookService service, CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, ct);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        });

        return app;
    }

    private static ValidationProblem AuthorNotFoundProblem(int authorId) =>
        TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            ["authorId"] = [$"Author {authorId} does not exist."],
        });
}
