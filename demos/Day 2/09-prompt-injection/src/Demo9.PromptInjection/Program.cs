// Day 2 Demo 9 — Prompt injection (make it visceral).
// Two endpoints for the SAME feature ("summarize my reading list") side by side:
//   POST /api/naive/summarize    — user text concatenated straight into the prompt (VULNERABLE)
//   POST /api/hardened/summarize — user text isolated as <user_input> data, out of the system prompt
// Fire the same injection at both and watch one leak while the other refuses.

using Anthropic;
using Anthropic.Models.Messages;
using Demo9.PromptInjection;

var builder = WebApplication.CreateBuilder(args);

// Stable local URL so the README's curl commands always hit the right port.
builder.WebHost.UseUrls("http://localhost:5099");

// Key comes from user-secrets (never committed), with the ANTHROPIC_API_KEY env var as fallback.
builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);
var apiKey = builder.Configuration["Anthropic:ApiKey"]
    ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("No API key found. Set one with:");
    Console.Error.WriteLine("  dotnet user-secrets set \"Anthropic:ApiKey\" \"sk-ant-...\"");
    Console.Error.WriteLine("or export ANTHROPIC_API_KEY.");
    return 1;
}

// AnthropicClient is a Singleton — it holds an HttpClient; Scoped/Transient risks socket exhaustion.
builder.Services.AddSingleton(new AnthropicClient { ApiKey = apiKey });

var app = builder.Build();

// --- VULNERABLE endpoint -------------------------------------------------------------------------
// The developer lazily dumped every user's data into context and pasted the raw user text right
// after the instructions. There is no boundary between "trusted instruction" and "untrusted input",
// so an injection can override the rule AND the other users' data is sitting there to be leaked.
app.MapPost("/api/naive/summarize", async (
    SummarizeRequest request,
    AnthropicClient client,
    CancellationToken ct) =>
{
    var prompt = Prompts.BuildNaivePrompt(request.Text);

    var response = await client.Messages.Create(new MessageCreateParams
    {
        Model = Model.ClaudeSonnet4_6,
        MaxTokens = 1024,
        Messages = [new() { Role = Role.User, Content = prompt }],
    }, cancellationToken: ct);

    return Results.Ok(new SummarizeResponse(
        "naive",
        ExtractText(response),
        response.Usage.InputTokens,
        response.Usage.OutputTokens,
        SentToModel: $"[single user message, no system prompt]\n\n{prompt}"));
});

// --- HARDENED endpoint ---------------------------------------------------------------------------
// Structural separation (strongest defense): trusted instructions + ONLY the current user's data go
// in the SYSTEM channel; the untrusted user text is sanitized and wrapped in <user_input> in the USER
// turn. The system prompt tells the model that anything inside those tags is data, never instructions.
app.MapPost("/api/hardened/summarize", async (
    SummarizeRequest request,
    AnthropicClient client,
    CancellationToken ct) =>
{
    var systemPrompt = Prompts.BuildHardenedSystemPrompt();
    var wrappedInput = PromptSafety.Wrap(request.Text); // sanitize + <user_input> tags

    var response = await client.Messages.Create(new MessageCreateParams
    {
        Model = Model.ClaudeSonnet4_6,
        MaxTokens = 1024,
        System = new List<TextBlockParam> { new() { Text = systemPrompt } },
        Messages = [new() { Role = Role.User, Content = wrappedInput }],
    }, cancellationToken: ct);

    return Results.Ok(new SummarizeResponse(
        "hardened",
        ExtractText(response),
        response.Usage.InputTokens,
        response.Usage.OutputTokens,
        SentToModel: $"[SYSTEM]\n{systemPrompt}\n\n[USER]\n{wrappedInput}"));
});

app.Run();
return 0;

// ContentBlock is a union type — filter to the text blocks and concatenate.
static string ExtractText(Message response) =>
    string.Concat(response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));

/// <summary>Exposed so the demo project can attach a user-secrets store to this assembly.</summary>
public partial class Program;
