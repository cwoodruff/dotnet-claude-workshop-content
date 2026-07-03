using BookTracker.Core.Dtos;

namespace BookTracker.Core.Services;

/// <summary>
/// Retrieval-augmented recommendations: embed the query, retrieve the most similar book/review
/// chunks, and ask Claude to answer grounded in that context. Pure interface — SDK impl in Api.
/// </summary>
public interface IRagService
{
    Task<RecommendResult> RecommendAsync(string query, CancellationToken ct = default);
}
