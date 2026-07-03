namespace BookTracker.Core.Dtos;

/// <summary>RAG recommendation request — a natural-language query about the library.</summary>
public record RecommendRequest(string Query);

/// <summary>A grounded recommendation plus the book titles the answer drew on.</summary>
public record RecommendResult(string Reply, IReadOnlyList<string> Sources);
