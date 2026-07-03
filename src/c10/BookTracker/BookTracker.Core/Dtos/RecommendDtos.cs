namespace BookTracker.Core.Dtos;

/// <summary>RAG recommendation request — a natural-language query about the library.</summary>
public record RecommendRequest(string Query);

/// <summary>A grounded recommendation, the book titles it drew on, and token usage.</summary>
public record RecommendResult(
    string Reply,
    IReadOnlyList<string> Sources,
    int InputTokens = 0,
    int OutputTokens = 0);
