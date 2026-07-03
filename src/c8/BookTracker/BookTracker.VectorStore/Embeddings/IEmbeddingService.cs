namespace BookTracker.VectorStore.Embeddings;

/// <summary>
/// Thin wrapper over Microsoft.Extensions.AI's IEmbeddingGenerator. The provider (Ollama / OpenAI) is
/// chosen at registration; this interface keeps callers provider-agnostic.
/// </summary>
public interface IEmbeddingService
{
    Task<float[]> EmbedAsync(string text, CancellationToken ct = default);

    Task<IReadOnlyList<float[]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken ct = default);
}
