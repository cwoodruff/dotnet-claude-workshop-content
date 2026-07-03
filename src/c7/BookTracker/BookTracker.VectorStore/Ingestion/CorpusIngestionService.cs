using BookTracker.Core.Interfaces;
using BookTracker.VectorStore.Chunking;
using BookTracker.VectorStore.Embeddings;
using BookTracker.VectorStore.Stores;
using Microsoft.Extensions.Options;

namespace BookTracker.VectorStore.Ingestion;

/// <summary>
/// Builds the RAG corpus from BookTracker data: each book's title, genre, and description plus its
/// review bodies are chunked, embedded, and upserted. Idempotent — skips if the store is already
/// populated. Depends only on Core repository ports (the Data implementations are injected at runtime).
/// </summary>
public class CorpusIngestionService
{
    private readonly IBookRepository _books;
    private readonly IReviewRepository _reviews;
    private readonly IEmbeddingService _embeddings;
    private readonly ITextChunker _chunker;
    private readonly IVectorStore _store;
    private readonly VectorStoreOptions _options;

    public CorpusIngestionService(
        IBookRepository books,
        IReviewRepository reviews,
        IEmbeddingService embeddings,
        ITextChunker chunker,
        IVectorStore store,
        IOptions<VectorStoreOptions> options)
    {
        _books = books;
        _reviews = reviews;
        _embeddings = embeddings;
        _chunker = chunker;
        _store = store;
        _options = options.Value;
    }

    /// <summary>Ingest the corpus. Returns the number of chunks upserted (0 if already populated).</summary>
    public async Task<int> IngestAsync(CancellationToken ct = default)
    {
        await _store.EnsureCollectionAsync(_options.VectorSize, ct);

        if (await _store.CountAsync(ct) > 0)
        {
            return 0; // already populated
        }

        var books = await _books.GetAllAsync(ct);
        var records = new List<VectorRecord>();

        foreach (var book in books)
        {
            var parts = new List<string> { book.Title };
            if (!string.IsNullOrWhiteSpace(book.Genre))
            {
                parts.Add($"Genre: {book.Genre}");
            }

            if (!string.IsNullOrWhiteSpace(book.Description))
            {
                parts.Add(book.Description);
            }

            var reviews = await _reviews.GetForBookAsync(book.Id, ct);
            parts.AddRange(reviews.Select(r => $"Review by {r.Reviewer} ({r.Rating}/5): {r.Body}"));

            var text = string.Join("\n", parts);
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
