using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookTracker.Api.Endpoints;

public static class AgentEndpoints
{
    public static IEndpointRouteBuilder MapAgentEndpoints(this IEndpointRouteBuilder app)
    {
        // SSE streaming — tokens are flushed per chunk so the client sees them live.
        app.MapGet("/api/chat/stream", async (
            string q, IStreamingService streaming, HttpResponse response, CancellationToken ct) =>
        {
            response.Headers.ContentType = "text/event-stream";
            await foreach (var chunk in streaming.StreamAsync(q, ct))
            {
                await response.WriteAsync($"data: {chunk}\n\n", ct);
                await response.Body.FlushAsync(ct);
            }
        }).WithTags("Chat");

        // Tool-calling agent — runs the loop, returns the final text + the tool calls it made.
        app.MapPost("/api/agent", async Task<Results<Ok<AgentResult>, ValidationProblem>> (
            AgentRequest request, IAgentService agent, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["message"] = ["Message is required."],
                });
            }

            return TypedResults.Ok(await agent.RunAsync(request.Message, ct));
        }).WithTags("Agent");

        return app;
    }
}
