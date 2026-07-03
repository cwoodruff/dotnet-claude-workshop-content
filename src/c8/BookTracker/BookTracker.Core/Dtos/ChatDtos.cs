namespace BookTracker.Core.Dtos;

/// <summary>One stored turn of chat history. Role is "user" or "assistant".</summary>
public record ChatTurn(string Role, string Content);

/// <summary>Chat request. SessionId keys the multi-turn history in the distributed cache.</summary>
public record ChatRequest(string SessionId, string Message);

/// <summary>
/// Chat reply plus token usage. Usage is surfaced so the prompt-caching demo is visible
/// (CacheReadInputTokens &gt; 0 on a repeat call means the cached system prompt was reused).
/// </summary>
public record ChatResponse(
    string Reply,
    int InputTokens,
    int OutputTokens,
    int CacheReadInputTokens);
