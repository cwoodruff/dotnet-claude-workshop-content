using System.Text.Json;
using Anthropic;
using Anthropic.Helpers.Beta;
using Anthropic.Models.Beta.Messages;
using BookTracker.Api.Options;
using BookTracker.Api.Security;
using BookTracker.Api.Tools;
using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using Microsoft.Extensions.Options;

namespace BookTracker.Api.Services;

/// <summary>
/// The same agent as <see cref="AgentService"/>, with the SDK's tool runner driving the loop. The
/// runner sends the request, runs the tools the model asks for, sends the results back (thinking
/// blocks included) and stops when the model answers. The guards stay visible: a 5-iteration cap,
/// and per-tool try/catch that turns a rule violation into an error result instead of a crash.
/// </summary>
public class ToolRunnerAgentService : IAgentService
{
    private const int MaxIterations = 5;

    private readonly AnthropicClient _client;
    private readonly AnthropicOptions _options;
    private readonly BookTrackerTools _tools;

    public ToolRunnerAgentService(AnthropicClient client, IOptions<AnthropicOptions> options, BookTrackerTools tools)
    {
        _client = client;
        _options = options.Value;
        _tools = tools;
    }

    public async Task<AgentResult> RunAsync(string message, CancellationToken ct = default)
    {
        var toolCalls = new List<AgentToolCall>();

        // Wrap each BookTracker tool so the runner can execute it. The definition is the same JSON
        // schema the manual loop sends; Run records the call and reports failures as tool errors.
        var runnable = BookTrackerTools.Definitions
            .Select(definition => (IBetaRunnableTool)new BetaRunnableTool
            {
                Name = definition.Name,
                Definition = BetaTool.FromRawUnchecked(definition.RawData),
                Run = async (toolUse, token) =>
                {
                    var input = JsonSerializer.Serialize(toolUse.Input);
                    try
                    {
                        var result = await _tools.ExecuteAsync(toolUse.Name, toolUse.Input, token);
                        toolCalls.Add(new AgentToolCall(toolUse.Name, input, result, false));
                        return result;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        var error = $"Error: {ex.Message}";
                        toolCalls.Add(new AgentToolCall(toolUse.Name, input, error, true));
                        throw new BetaToolError(error); // sent back as a tool_result with is_error
                    }
                },
            })
            .ToList();

        var runner = _client.Beta.Messages.ToolRunner(
            new MessageCreateParams
            {
                Model = _options.Model,
                MaxTokens = _options.MaxTokens,
                System = new List<BetaTextBlockParam>
                {
                    new() { Text = _options.SystemPrompt, CacheControl = new BetaCacheControlEphemeral() },
                },
                // Wrap untrusted user text so the model treats it as data, not instructions.
                Messages = [new BetaUserMessageParam(PromptSafety.Wrap(message))],
                OutputConfig = string.IsNullOrWhiteSpace(_options.Effort)
                    ? null
                    : new BetaOutputConfig { Effort = _options.Effort },
            },
            runnable,
            thread: false, // send the whole history each turn, like the manual loop
            maxIterations: MaxIterations);

        BetaMessage? last = null;
        var inputTokens = 0;
        var outputTokens = 0;
        await foreach (var response in runner.WithCancellation(ct))
        {
            last = response;
            inputTokens += (int)response.Usage.InputTokens;
            outputTokens += (int)response.Usage.OutputTokens;
        }

        if (last is null)
        {
            return new AgentResult("The model returned no response.", toolCalls);
        }

        if (last.StopReason == "max_tokens")
        {
            return new AgentResult(
                "Stopped: the reply hit the MaxTokens limit before it finished. Raise Anthropic:MaxTokens.",
                toolCalls, inputTokens, outputTokens);
        }

        if (last.StopReason == "tool_use")
        {
            // The runner hit maxIterations while the model still wanted tools.
            return new AgentResult(
                "Stopped after reaching the maximum number of tool iterations.", toolCalls, inputTokens, outputTokens);
        }

        var reply = string.Concat(last.Content.Select(b => b.Value).OfType<BetaTextBlock>().Select(t => t.Text));
        return new AgentResult(reply, toolCalls, inputTokens, outputTokens);
    }
}
