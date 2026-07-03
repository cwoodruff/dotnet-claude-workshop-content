using System.Runtime.CompilerServices;
using Anthropic;
using Anthropic.Models.Messages;
using BookTracker.Api.Options;
using BookTracker.Core.Services;
using Microsoft.Extensions.Options;

namespace BookTracker.Api.Services;

/// <summary>
/// Streams a Claude reply as text deltas using the SDK's CreateStreaming. The endpoint forwards each
/// delta as an SSE frame.
/// </summary>
public class StreamingService : IStreamingService
{
    private readonly AnthropicClient _client;
    private readonly AnthropicOptions _options;

    public StreamingService(AnthropicClient client, IOptions<AnthropicOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async IAsyncEnumerable<string> StreamAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var parameters = new MessageCreateParams
        {
            Model = _options.Model,
            MaxTokens = _options.MaxTokens,
            System = new List<TextBlockParam>
            {
                new() { Text = _options.SystemPrompt, CacheControl = new CacheControlEphemeral() },
            },
            Messages = [new() { Role = Role.User, Content = prompt }],
        };

        await foreach (var ev in _client.Messages.CreateStreaming(parameters, cancellationToken: ct))
        {
            if (ev.TryPickContentBlockDelta(out var delta) && delta.Delta.TryPickText(out var text))
            {
                yield return text.Text;
            }
        }
    }
}
