using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using BookTracker.Api.Options;
using BookTracker.Api.Tools;
using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using Microsoft.Extensions.Options;

namespace BookTracker.Api.Services;

/// <summary>
/// The tool-calling agent loop (manual, not BetaToolRunner — the teaching reference). Claude plans,
/// requests tools, we dispatch them against real BookTracker data, feed results back, and loop until
/// it produces a final answer. Guarded by a max-iteration cap and per-tool try/catch.
/// </summary>
public class AgentService : IAgentService
{
    private const int MaxIterations = 5;

    private readonly AnthropicClient _client;
    private readonly AnthropicOptions _options;
    private readonly BookTrackerTools _tools;

    public AgentService(AnthropicClient client, IOptions<AnthropicOptions> options, BookTrackerTools tools)
    {
        _client = client;
        _options = options.Value;
        _tools = tools;
    }

    public async Task<AgentResult> RunAsync(string message, CancellationToken ct = default)
    {
        var messages = new List<MessageParam>
        {
            new() { Role = Role.User, Content = message },
        };
        var toolCalls = new List<AgentToolCall>();

        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            var response = await _client.Messages.Create(new MessageCreateParams
            {
                Model = _options.Model,
                MaxTokens = _options.MaxTokens,
                System = new List<TextBlockParam>
                {
                    new() { Text = _options.SystemPrompt, CacheControl = new CacheControlEphemeral() },
                },
                Messages = messages,
                Tools = BookTrackerTools.Definitions.Select(t => (ToolUnion)t).ToList(),
            }, cancellationToken: ct);

            if (response.StopReason != "tool_use")
            {
                var finalText = string.Concat(
                    response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));
                return new AgentResult(finalText, toolCalls);
            }

            // Rebuild the assistant turn and execute each requested tool.
            var assistantContent = new List<ContentBlockParam>();
            var toolResults = new List<ContentBlockParam>();

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

                    string result;
                    var isError = false;
                    try
                    {
                        result = await _tools.ExecuteAsync(toolUse.Name, toolUse.Input, ct);
                    }
                    catch (Exception ex)
                    {
                        // Surface rule violations (e.g. the C4 state machine) back to the model
                        // as a tool error rather than crashing the request.
                        result = $"Error: {ex.Message}";
                        isError = true;
                    }

                    toolCalls.Add(new AgentToolCall(
                        toolUse.Name, JsonSerializer.Serialize(toolUse.Input), result, isError));
                    toolResults.Add(new ToolResultBlockParam
                    {
                        ToolUseID = toolUse.ID,
                        Content = result,
                        IsError = isError,
                    });
                }
            }

            messages.Add(new MessageParam { Role = Role.Assistant, Content = assistantContent });
            messages.Add(new MessageParam { Role = Role.User, Content = toolResults });
        }

        return new AgentResult(
            "Stopped after reaching the maximum number of tool iterations.", toolCalls);
    }
}
