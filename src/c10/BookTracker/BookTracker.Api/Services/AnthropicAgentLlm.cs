using Anthropic;
using Anthropic.Models.Messages;
using BookTracker.Api.Options;
using BookTracker.Api.Tools;
using BookTracker.Core.Services;
using Microsoft.Extensions.Options;

namespace BookTracker.Api.Services;

/// <summary>
/// The Anthropic SDK implementation of the agent LLM seam. All SDK-specific message/block handling
/// lives here, so AgentService's loop stays provider-neutral and testable.
/// </summary>
public class AnthropicAgentLlm : IAgentLlm
{
    private readonly AnthropicClient _client;
    private readonly AnthropicOptions _options;

    public AnthropicAgentLlm(AnthropicClient client, IOptions<AnthropicOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public IAgentConversation StartConversation(string userMessage)
        => new AnthropicAgentConversation(_client, _options, userMessage);

    private sealed class AnthropicAgentConversation : IAgentConversation
    {
        private readonly AnthropicClient _client;
        private readonly AnthropicOptions _options;
        private readonly List<MessageParam> _messages;
        private Message? _lastResponse;

        public AnthropicAgentConversation(AnthropicClient client, AnthropicOptions options, string userMessage)
        {
            _client = client;
            _options = options;
            _messages = [new() { Role = Role.User, Content = userMessage }];
        }

        public Task<AgentTurn> SendUserMessageAsync(CancellationToken ct = default) => CreateAsync(ct);

        public Task<AgentTurn> SubmitToolOutcomesAsync(
            IReadOnlyList<AgentToolOutcome> outcomes,
            CancellationToken ct = default)
        {
            // Echo the assistant's previous turn (text + tool_use blocks) ...
            var assistantContent = new List<ContentBlockParam>();
            foreach (var block in _lastResponse!.Content)
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

            _messages.Add(new MessageParam { Role = Role.Assistant, Content = assistantContent });

            // ... then the matching tool_result blocks as the next user turn.
            var toolResults = outcomes
                .Select(o => (ContentBlockParam)new ToolResultBlockParam
                {
                    ToolUseID = o.Id,
                    Content = o.Result,
                    IsError = o.IsError,
                })
                .ToList();
            _messages.Add(new MessageParam { Role = Role.User, Content = toolResults });

            return CreateAsync(ct);
        }

        private async Task<AgentTurn> CreateAsync(CancellationToken ct)
        {
            var response = await _client.Messages.Create(new MessageCreateParams
            {
                Model = _options.Model,
                MaxTokens = _options.MaxTokens,
                System = new List<TextBlockParam>
                {
                    new() { Text = _options.SystemPrompt, CacheControl = new CacheControlEphemeral() },
                },
                Messages = _messages,
                Tools = BookTrackerTools.Definitions.Select(t => (ToolUnion)t).ToList(),
            }, cancellationToken: ct);

            _lastResponse = response;

            var finalText = string.Concat(
                response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));

            var toolRequests = new List<AgentToolRequest>();
            foreach (var block in response.Content)
            {
                if (block.TryPickToolUse(out ToolUseBlock? toolUse))
                {
                    toolRequests.Add(new AgentToolRequest(toolUse.ID, toolUse.Name, toolUse.Input));
                }
            }

            return new AgentTurn(
                finalText, toolRequests,
                (int)response.Usage.InputTokens, (int)response.Usage.OutputTokens);
        }
    }
}
