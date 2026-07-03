namespace BookTracker.VectorStore.Chunking;

/// <summary>
/// Splits text into overlapping windows for embedding. Chunk size and overlap are the RAG quality
/// lever (Lab 3 Part C tunes them) — too large dilutes relevance, too small loses context.
/// </summary>
public interface ITextChunker
{
    IReadOnlyList<string> Chunk(string text);
}
