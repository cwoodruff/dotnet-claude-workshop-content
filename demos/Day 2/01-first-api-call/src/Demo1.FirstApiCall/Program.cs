// Day 2 Demo 1 — First working API call.
// One Messages.Create call, print the reply text and the Usage token counts.
// The Anthropic API is STATELESS: this call carries everything the model sees.

using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Configuration;

// Ctrl+C cancels the in-flight request cleanly instead of tearing the process down.
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

// Key comes from user-secrets (never committed), with the ANTHROPIC_API_KEY env var as fallback.
var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var apiKey = configuration["Anthropic:ApiKey"]
    ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("No API key found. Set one with:");
    Console.Error.WriteLine("  dotnet user-secrets set \"Anthropic:ApiKey\" \"sk-ant-...\"");
    Console.Error.WriteLine("or export ANTHROPIC_API_KEY.");
    return 1;
}

var client = new AnthropicClient { ApiKey = apiKey };

const string prompt = "Recommend a book like Dune.";
Console.WriteLine($"User: {prompt}");
Console.WriteLine();

var response = await client.Messages.Create(new MessageCreateParams
{
    Model = Model.ClaudeSonnet4_6,   // app default; Haiku for cheap, Opus for hard
    MaxTokens = 1024,                // hard ceiling on the reply — always set one
    Messages = [new() { Role = Role.User, Content = prompt }],
}, cancellationToken: cts.Token);

// ContentBlock is a union type — filter to the text blocks.
foreach (var text in response.Content.Select(b => b.Value).OfType<TextBlock>())
    Console.WriteLine(text.Text);

Console.WriteLine();
Console.WriteLine($"Usage — input tokens: {response.Usage.InputTokens}, output tokens: {response.Usage.OutputTokens}");
return 0;
