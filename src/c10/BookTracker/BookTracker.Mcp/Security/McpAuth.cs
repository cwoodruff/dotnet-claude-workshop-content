namespace BookTracker.Mcp.Security;

/// <summary>
/// Bearer-token gate for the MCP server. A teaching stub: if "Mcp:ApiKey" is configured, every request
/// must carry "Authorization: Bearer &lt;key&gt;"; if it is not configured, requests are allowed so the
/// server runs locally without setup. Never expose a real gateway unauthenticated.
/// </summary>
public class McpAuth
{
    private readonly RequestDelegate _next;
    private readonly string? _apiKey;

    public McpAuth(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _apiKey = configuration["Mcp:ApiKey"];
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!string.IsNullOrEmpty(_apiKey))
        {
            var header = context.Request.Headers.Authorization.ToString();
            if (header != $"Bearer {_apiKey}")
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: missing or invalid bearer token.");
                return;
            }
        }

        await _next(context);
    }
}
