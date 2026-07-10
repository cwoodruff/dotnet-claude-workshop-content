using Anthropic;
using Demo3.SseStreaming;

var builder = WebApplication.CreateBuilder(args);

// API key from user-secrets (Anthropic:ApiKey) or the ANTHROPIC_API_KEY environment variable.
// Never hardcode it and never commit it.
var apiKey = builder.Configuration["Anthropic:ApiKey"]
    ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
    ?? throw new InvalidOperationException(
        "No API key found. Run 'dotnet user-secrets set Anthropic:ApiKey <key>' "
        + "or set the ANTHROPIC_API_KEY environment variable.");

// Singleton: AnthropicClient holds an HttpClient; Scoped/Transient risks socket exhaustion.
builder.Services.AddSingleton(new AnthropicClient { ApiKey = apiKey });
builder.Services.AddSingleton<StreamingService>();

var app = builder.Build();

app.MapGet("/", () => "Demo3.SseStreaming — try: curl -N \"http://localhost:5055/api/chat/stream?q=hello\"");

// Streams Claude tokens to the client as SSE `data:` frames, flushing per chunk so the
// client sees tokens the moment they arrive instead of one buffered blob at the end.
app.MapGet("/api/chat/stream", async (string q, StreamingService svc, HttpResponse res, CancellationToken ct) =>
{
    res.Headers.ContentType = "text/event-stream";

    await foreach (var chunk in svc.StreamAsync(q, ct))
    {
        await res.WriteAsync($"data: {chunk}\n\n", ct);
        await res.Body.FlushAsync(ct);   // flush so the client sees tokens immediately
    }
});

app.Run();
