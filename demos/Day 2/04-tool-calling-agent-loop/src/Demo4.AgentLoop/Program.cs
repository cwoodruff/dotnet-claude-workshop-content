using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using Demo4.AgentLoop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// --- Setup ------------------------------------------------------------------------------------

using var loggerFactory = LoggerFactory.Create(b => b.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "HH:mm:ss ";
}));
var logger = loggerFactory.CreateLogger("AgentLoop");

// API key — never hardcoded, never committed. Prefer a user secret (dotnet user-secrets
// set ANTHROPIC_API_KEY ...) and fall back to the environment variable.
var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var apiKey = config["Anthropic:ApiKey"]
    ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
    ?? throw new InvalidOperationException(
        "Set ANTHROPIC_API_KEY as a user secret (dotnet user-secrets set ANTHROPIC_API_KEY <key>) "
        + "or as an environment variable before running this demo.");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};
var ct = cts.Token;

var client = new AnthropicClient { ApiKey = apiKey };
var store = new BookStore();
var tools = new AgentTools(store);

var prompt = args.Length > 0 ? string.Join(' ', args) : "Find Dune and mark it as completed.";
logger.LogInformation("User prompt: {Prompt}", prompt);

// --- The agent loop ---------------------------------------------------------------------------
// Claude requests a tool -> we execute the C# method -> we return a tool_result -> Claude
// continues, possibly calling more tools, until StopReason is EndTurn (no more tool_use blocks).
// Always guard the loop with a max-iteration cap.

const int MaxIterations = 5;
var inputTokens = 0L;
var outputTokens = 0L;
List<MessageParam> messages = [new() { Role = Role.User, Content = prompt }];

for (var iteration = 1; iteration <= MaxIterations; iteration++)
{
    var response = await client.Messages.Create(new MessageCreateParams
    {
        Model = Model.ClaudeSonnet4_6,
        MaxTokens = 1024,
        System = new List<TextBlockParam>
        {
            new()
            {
                Text = "You are the BookTracker assistant. Use the provided tools to read and "
                    + "update the user's book catalog. Look up a book with find_book before "
                    + "updating it, and confirm what you changed in your final answer.",
            },
        },
        Messages = messages,
        Tools = AgentTools.Definitions.Select(t => (ToolUnion)t).ToList(),
    }, cancellationToken: ct);

    inputTokens += response.Usage.InputTokens;
    outputTokens += response.Usage.OutputTokens;
    logger.LogInformation("Iteration {Iteration}: stop reason {StopReason}", iteration, response.StopReason);

    // Collect the tool_use blocks Claude asked for on this turn.
    var toolUses = new List<ToolUseBlock>();
    foreach (var block in response.Content)
    {
        if (block.TryPickToolUse(out ToolUseBlock? toolUse))
        {
            toolUses.Add(toolUse);
        }
    }

    // No tool requests -> the model ended its turn; print the final answer and stop.
    if (toolUses.Count == 0)
    {
        var finalText = string.Concat(
            response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));

        Console.WriteLine();
        Console.WriteLine("=== Final answer ===");
        Console.WriteLine(finalText);
        Console.WriteLine();
        logger.LogInformation(
            "Done in {Iterations} iteration(s). Tokens: {InputTokens} in / {OutputTokens} out",
            iteration, inputTokens, outputTokens);
        return;
    }

    // Echo the assistant's turn (text + tool_use blocks) back into the history ...
    var assistantContent = new List<ContentBlockParam>();
    foreach (var block in response.Content)
    {
        if (block.TryPickText(out TextBlock? text))
        {
            assistantContent.Add(new TextBlockParam { Text = text.Text });
        }
        else if (block.TryPickToolUse(out ToolUseBlock? toolUse))
        {
            assistantContent.Add(new ToolUseBlockParam
            {
                ID = toolUse.ID,
                Name = toolUse.Name,
                Input = toolUse.Input,
            });
        }
    }

    messages.Add(new MessageParam { Role = Role.Assistant, Content = assistantContent });

    // ... execute each requested tool and send the matching tool_result blocks as the next user turn.
    var toolResults = new List<ContentBlockParam>();
    foreach (var toolUse in toolUses)
    {
        string result;
        var isError = false;
        try
        {
            result = await tools.ExecuteAsync(toolUse.Name, toolUse.Input, ct);
        }
        catch (OperationCanceledException)
        {
            throw; // cancellation propagates — not a tool error
        }
        catch (Exception ex)
        {
            // Return the failure as an is_error tool_result so the model can recover — don't crash.
            result = $"Error: {ex.Message}";
            isError = true;
        }

        logger.LogInformation(
            "Tool call {Tool} input={Input} error={IsError} result={Result}",
            toolUse.Name, JsonSerializer.Serialize(toolUse.Input), isError, result);

        toolResults.Add(new ToolResultBlockParam
        {
            ToolUseID = toolUse.ID,
            Content = result,
            IsError = isError,
        });
    }

    messages.Add(new MessageParam { Role = Role.User, Content = toolResults });
}

logger.LogWarning(
    "Stopped after the {MaxIterations}-iteration cap without reaching EndTurn. Tokens: {InputTokens} in / {OutputTokens} out",
    MaxIterations, inputTokens, outputTokens);
