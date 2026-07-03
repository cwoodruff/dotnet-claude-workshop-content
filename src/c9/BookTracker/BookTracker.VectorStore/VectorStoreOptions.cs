namespace BookTracker.VectorStore;

/// <summary>
/// RAG configuration. Bound from the "VectorStore" config section. Defaults run Docker-free:
/// Provider = InMemory, embeddings from a local Ollama. VectorSize must equal the embedding model's
/// output dimensions (nomic-embed-text = 768, OpenAI text-embedding-3-small = 1536).
/// </summary>
public class VectorStoreOptions
{
    /// <summary>"InMemory" (default, no Docker) or "Qdrant".</summary>
    public string Provider { get; set; } = "InMemory";

    public string QdrantHost { get; set; } = "localhost";

    public int QdrantPort { get; set; } = 6334; // gRPC — NOT the REST port 6333

    public string CollectionName { get; set; } = "books";

    public int VectorSize { get; set; } = 768;

    public string OllamaUrl { get; set; } = "http://localhost:11434";

    public string EmbeddingModel { get; set; } = "nomic-embed-text";

    public int ChunkSize { get; set; } = 500;

    public int ChunkOverlap { get; set; } = 80;

    public int TopK { get; set; } = 5;
}
