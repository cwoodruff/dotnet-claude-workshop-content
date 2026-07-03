using Microsoft.Extensions.AI;

namespace BookTracker.VectorStore.Embeddings;

/// <summary>
/// Generates embeddings via the injected IEmbeddingGenerator (Ollama or OpenAI — same seam).
/// </summary>
public class EmbeddingService : IEmbeddingService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;

    public EmbeddingService(IEmbeddingGenerator<string, Embedding<float>> generator)
    {
        _generator = generator;
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        var result = await _generator.GenerateAsync([text], cancellationToken: ct);
        return result[0].Vector.ToArray();
    }

    public async Task<IReadOnlyList<float[]>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken ct = default)
    {
        if (texts.Count == 0)
        {
            return [];
        }

        var result = await _generator.GenerateAsync(texts, cancellationToken: ct);
        return result.Select(e => e.Vector.ToArray()).ToList();
    }
}
