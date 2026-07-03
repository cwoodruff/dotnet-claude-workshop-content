using System.ComponentModel.DataAnnotations;
using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookTracker.Api.Endpoints;

public static class ReadingProgressEndpoints
{
    public static IEndpointRouteBuilder MapReadingProgressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books/{id:int}/reading-progress").WithTags("ReadingProgress");

        group.MapGet("/", async Task<Results<Ok<ReadingProgressDto>, NotFound>> (
            int id, IReadingProgressService service, CancellationToken ct) =>
        {
            var progress = await service.GetForBookAsync(id, ct);
            return progress is null ? TypedResults.NotFound() : TypedResults.Ok(progress);
        });

        group.MapPut("/", async Task<Results<Ok<ReadingProgressDto>, NotFound, ValidationProblem>> (
            int id, UpdateReadingProgressRequest request, IReadingProgressService service, CancellationToken ct) =>
        {
            try
            {
                return TypedResults.Ok(await service.UpdateAsync(id, request, ct));
            }
            catch (KeyNotFoundException)
            {
                return TypedResults.NotFound();
            }
            catch (ValidationException ex)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["readingProgress"] = [ex.Message],
                });
            }
        });

        return app;
    }
}
