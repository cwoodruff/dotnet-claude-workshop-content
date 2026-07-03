using BookTracker.Api.Options;
using BookTracker.Api.Security;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace BookTracker.Api.Middleware;

/// <summary>
/// Resolves the caller identity (X-User-Id header, default "anonymous") into HttpContext.Items, and on
/// AI routes rejects with 429 once the user's daily token budget is exhausted. Spend is recorded by
/// AiAuditService after each successful call.
/// </summary>
public class TokenBudgetMiddleware
{
    public const string UserItemKey = "AiUserId";

    private static readonly string[] AiPaths = ["/api/chat", "/api/agent", "/api/recommend"];

    private readonly RequestDelegate _next;

    public TokenBudgetMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IDistributedCache cache, IOptions<AnthropicOptions> options)
    {
        var user = context.Request.Headers["X-User-Id"].FirstOrDefault();
        user = string.IsNullOrWhiteSpace(user) ? "anonymous" : user;
        context.Items[UserItemKey] = user;

        if (IsAiPath(context.Request.Path))
        {
            var spent = await TokenBudget.GetAsync(cache, user, context.RequestAborted);
            if (spent >= options.Value.DailyTokenCap)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Daily AI token budget exceeded. Try again tomorrow.");
                return;
            }
        }

        await _next(context);
    }

    private static bool IsAiPath(PathString path)
        => AiPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
}
