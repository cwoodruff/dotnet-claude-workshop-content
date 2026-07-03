namespace BookTracker.Core.Services;

/// <summary>
/// Streams a Claude reply token-by-token. Pure interface — no Anthropic SDK types — so Core stays
/// SDK-free; the SSE implementation lives in BookTracker.Api.
/// </summary>
public interface IStreamingService
{
    IAsyncEnumerable<string> StreamAsync(string prompt, CancellationToken ct = default);
}
