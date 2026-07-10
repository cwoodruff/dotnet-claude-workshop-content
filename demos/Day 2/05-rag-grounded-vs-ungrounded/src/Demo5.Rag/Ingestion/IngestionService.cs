using Demo5.Rag.Catalog;
using Demo5.Rag.Chunking;
using Demo5.Rag.Embeddings;
using Demo5.Rag.Options;
using Demo5.Rag.Stores;
using Microsoft.Extensions.Options;

namespace Demo5.Rag.Ingestion;

/// <summary>
/// Builds the RAG corpus from the embedded book catalog: each book's title, author, genre, and
/// description are chunked, embedded, and upserted. Idempotent — skips if the store is already
/// populated (pass force: true to re-ingest).
/// </summary>
public class IngestionService
{
    private readonly IEmbeddingService _embeddings;
    private readonly ITextChunker _chunker;
    private readonly IVectorStore _store;
    private readonly VectorStoreOptions _options;

    public IngestionService(
        IEmbeddingService embeddings,
        ITextChunker chunker,
        IVectorStore store,
        IOptions<VectorStoreOptions> options)
    {
        _embeddings = embeddings;
        _chunker = chunker;
        _store = store;
        _options = options.Value;
    }

    /// <summary>Ingest the catalog. Returns the number of chunks upserted (0 if already populated).</summary>
    public async Task<int> IngestAsync(bool force = false, CancellationToken ct = default)
    {
        await _store.EnsureCollectionAsync(_options.VectorSize, ct);

        if (!force && await _store.CountAsync(ct) > 0)
        {
            return 0; // already populated
        }

        var books = await BookCatalog.LoadAsync(ct);
        var records = new List<VectorRecord>();

        foreach (var book in books)
        {
            var text = $"{book.Title} by {book.Author}\nGenre: {book.Genre}\n{book.Description}";
            var chunks = _chunker.Chunk(text);
            if (chunks.Count == 0)
            {
                continue;
            }

            var vectors = await _embeddings.EmbedBatchAsync(chunks, ct);
            for (var i = 0; i < chunks.Count; i++)
            {
                records.Add(new VectorRecord(Guid.NewGuid(), vectors[i], book.Id, book.Title, chunks[i]));
            }
        }

        await _store.UpsertAsync(records, ct);
        return records.Count;
    }
}
