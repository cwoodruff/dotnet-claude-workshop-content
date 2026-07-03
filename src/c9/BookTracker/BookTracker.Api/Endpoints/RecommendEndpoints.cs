using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookTracker.Api.Endpoints;

public static class RecommendEndpoints
{
    public static IEndpointRouteBuilder MapRecommendEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/recommend", async Task<Results<Ok<RecommendResult>, ValidationProblem>> (
            RecommendRequest request, IRagService rag, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["query"] = ["Query is required."],
                });
            }

            return TypedResults.Ok(await rag.RecommendAsync(request.Query, ct));
        }).WithTags("Recommend");

        return app;
    }
}
