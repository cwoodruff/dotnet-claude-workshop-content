using System.Collections.Concurrent;

namespace BookTracker.VectorStore.Stores;

/// <summary>
/// Docker-free fallback store. Holds vectors in memory and ranks by cosine similarity. Keeps the RAG
/// lab working when Qdrant isn't available; not for production (no persistence, linear scan).
/// </summary>
public class InMemoryVectorStore : IVectorStore
{
    private readonly ConcurrentDictionary<Guid, VectorRecord> _records = new();

    public Task EnsureCollectionAsync(int vectorSize, CancellationToken ct = default) => Task.CompletedTask;

    public Task UpsertAsync(IEnumerable<VectorRecord> records, CancellationToken ct = default)
    {
        foreach (var record in records)
        {
            _records[record.Id] = record;
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<VectorHit>> SearchAsync(float[] query, int topK, CancellationToken ct = default)
    {
        var hits = _records.Values
            .Select(r => new VectorHit(r.BookId, r.Title, r.Text, Cosine(query, r.Vector)))
            .OrderByDescending(h => h.Score)
            .Take(topK)
            .ToList();

        return Task.FromResult<IReadOnlyList<VectorHit>>(hits);
    }

    public Task<long> CountAsync(CancellationToken ct = default) => Task.FromResult((long)_records.Count);

    private static float Cosine(float[] a, float[] b)
    {
        if (a.Length != b.Length || a.Length == 0)
        {
            return 0f;
        }

        double dot = 0, magA = 0, magB = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        if (magA == 0 || magB == 0)
        {
            return 0f;
        }

        return (float)(dot / (Math.Sqrt(magA) * Math.Sqrt(magB)));
    }
}
