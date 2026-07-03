using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookTracker.Api.Endpoints;

public static class ReviewsEndpoints
{
    public static IEndpointRouteBuilder MapReviewsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books/{id:int}/reviews").WithTags("Reviews");

        group.MapGet("/", async Task<Results<Ok<IReadOnlyList<ReviewDto>>, NotFound>> (
            int id, IBookService books, IReviewService reviews, CancellationToken ct) =>
        {
            var book = await books.GetByIdAsync(id, ct);
            if (book is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(await reviews.GetForBookAsync(id, ct));
        });

        return app;
    }
}
