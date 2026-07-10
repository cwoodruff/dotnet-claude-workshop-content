namespace Demo5.Rag.Stores;

/// <summary>A chunk to store: its vector plus the payload needed to ground an answer.</summary>
public record VectorRecord(Guid Id, float[] Vector, int BookId, string Title, string Text);

/// <summary>A retrieved chunk with its similarity score.</summary>
public record VectorHit(int BookId, string Title, string Text, float Score);

/// <summary>
/// Vector store abstraction. Two implementations: Qdrant (gRPC) and an in-memory fallback that
/// keeps the demo alive without Docker. Both rank by cosine similarity.
/// </summary>
public interface IVectorStore
{
    /// <summary>Create the collection if it doesn't exist, sized to the embedding model's dims.</summary>
    Task EnsureCollectionAsync(int vectorSize, CancellationToken ct = default);

    Task UpsertAsync(IEnumerable<VectorRecord> records, CancellationToken ct = default);

    Task<IReadOnlyList<VectorHit>> SearchAsync(float[] query, int topK, CancellationToken ct = default);

    /// <summary>Number of stored vectors — used to skip ingestion when already populated.</summary>
    Task<long> CountAsync(CancellationToken ct = default);
}
