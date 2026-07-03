using System.Text.Json;
using BookTracker.Api.Middleware;
using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Distributed;

namespace BookTracker.Api.Endpoints;

public static class ChatEndpoints
{
    private const string HistoryKeyPrefix = "chat:history:";

    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat").WithTags("Chat");

        group.MapPost("/", async Task<Results<Ok<ChatResponse>, ValidationProblem>> (
            ChatRequest request, IClaudeService claude, IDistributedCache cache,
            IAiAuditService audit, HttpContext http, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.SessionId) || string.IsNullOrWhiteSpace(request.Message))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["request"] = ["SessionId and Message are required."],
                });
            }

            var key = HistoryKeyPrefix + request.SessionId;

            // History survives across stateless HTTP requests via the distributed cache (keyed by SessionId).
            var stored = await cache.GetStringAsync(key, ct);
            var history = stored is null
                ? new List<ChatTurn>()
                : JsonSerializer.Deserialize<List<ChatTurn>>(stored) ?? [];

            var response = await claude.ChatAsync(request.Message, history, ct);

            history.Add(new ChatTurn("user", request.Message));
            history.Add(new ChatTurn("assistant", response.Reply));
            await cache.SetStringAsync(key, JsonSerializer.Serialize(history), ct);

            var user = http.Items[TokenBudgetMiddleware.UserItemKey] as string ?? "anonymous";
            await audit.RecordAsync(user, "chat", response.InputTokens, response.OutputTokens, ct);

            return TypedResults.Ok(response);
        });

        return app;
    }
}
