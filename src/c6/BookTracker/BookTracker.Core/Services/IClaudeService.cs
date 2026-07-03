using BookTracker.Core.Dtos;

namespace BookTracker.Core.Services;

/// <summary>
/// Chat with Claude, scoped to BookTracker. Pure interface — no Anthropic SDK types in the
/// signature, so Core stays free of the SDK (only BookTracker.Api references the package).
/// </summary>
public interface IClaudeService
{
    Task<ChatResponse> ChatAsync(string userMessage, IReadOnlyList<ChatTurn> history, CancellationToken ct = default);
}
