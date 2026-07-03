using BookTracker.Api.Middleware;
using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookTracker.Api.Endpoints;

public static class RecommendEndpoints
{
    public static IEndpointRouteBuilder MapRecommendEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/recommend", async Task<Results<Ok<RecommendResult>, ValidationProblem>> (
            RecommendRequest request, IRagService rag,
            IAiAuditService audit, HttpContext http, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["query"] = ["Query is required."],
                });
            }

            var result = await rag.RecommendAsync(request.Query, ct);

            var user = http.Items[TokenBudgetMiddleware.UserItemKey] as string ?? "anonymous";
            await audit.RecordAsync(user, "recommend", result.InputTokens, result.OutputTokens, ct);

            return TypedResults.Ok(result);
        }).WithTags("Recommend");

        return app;
    }
}
