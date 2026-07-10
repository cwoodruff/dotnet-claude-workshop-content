using System.Runtime.CompilerServices;
using Anthropic;
using Anthropic.Models.Messages;

namespace Demo3.SseStreaming;

/// <summary>
/// Surfaces Claude's text deltas as an <see cref="IAsyncEnumerable{T}"/> of strings using the SDK's
/// CreateStreaming. The endpoint forwards each delta to the client as an SSE <c>data:</c> frame.
/// </summary>
public class StreamingService
{
    private readonly AnthropicClient _client;

    public StreamingService(AnthropicClient client) => _client = client;

    public async IAsyncEnumerable<string> StreamAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var parameters = new MessageCreateParams
        {
            Model = Model.ClaudeSonnet4_6,
            MaxTokens = 1024,
            Messages = [new() { Role = Role.User, Content = prompt }],
        };

        // CreateStreaming returns an IAsyncEnumerable of stream events; pick out the text deltas.
        await foreach (var ev in _client.Messages.CreateStreaming(parameters, cancellationToken: ct))
        {
            if (ev.TryPickContentBlockDelta(out var delta) && delta.Delta.TryPickText(out var text))
            {
                yield return text.Text;
            }
        }
    }
}
