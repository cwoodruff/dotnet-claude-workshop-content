using Demo5.Rag.Options;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Demo5.Rag.Stores;

/// <summary>
/// Qdrant-backed store over the official gRPC client. Connects on the gRPC port (6334), not REST 6333.
/// </summary>
public class QdrantVectorStore : IVectorStore
{
    private readonly QdrantClient _client;
    private readonly string _collection;

    public QdrantVectorStore(IOptions<VectorStoreOptions> options)
    {
        var opts = options.Value;
        _client = new QdrantClient(opts.QdrantHost, opts.QdrantPort);
        _collection = opts.CollectionName;
    }

    public async Task EnsureCollectionAsync(int vectorSize, CancellationToken ct = default)
    {
        if (await _client.CollectionExistsAsync(_collection, ct))
        {
            return;
        }

        await _client.CreateCollectionAsync(
            _collection,
            new VectorParams { Size = (ulong)vectorSize, Distance = Distance.Cosine },
            cancellationToken: ct);
    }

    public async Task UpsertAsync(IEnumerable<VectorRecord> records, CancellationToken ct = default)
    {
        var points = records.Select(r =>
        {
            var point = new PointStruct
            {
                Id = new PointId { Uuid = r.Id.ToString() },
                Vectors = r.Vector,
            };
            point.Payload.Add("bookId", r.BookId);
            point.Payload.Add("title", r.Title);
            point.Payload.Add("text", r.Text);
            return point;
        }).ToList();

        if (points.Count == 0)
        {
            return;
        }

        await _client.UpsertAsync(_collection, points, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<VectorHit>> SearchAsync(
        float[] query,
        int topK,
        CancellationToken ct = default)
    {
        var results = await _client.SearchAsync(
            _collection,
            query,
            limit: (ulong)topK,
            payloadSelector: true,
            cancellationToken: ct);

        return results.Select(p => new VectorHit(
            (int)p.Payload["bookId"].IntegerValue,
            p.Payload["title"].StringValue,
            p.Payload["text"].StringValue,
            p.Score)).ToList();
    }

    public async Task<long> CountAsync(CancellationToken ct = default)
    {
        if (!await _client.CollectionExistsAsync(_collection, ct))
        {
            return 0;
        }

        return (long)await _client.CountAsync(_collection, cancellationToken: ct);
    }
}
