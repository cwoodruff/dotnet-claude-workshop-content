namespace Demo5.Rag.Options;

/// <summary>
/// Bound from the "Anthropic" configuration section. ApiKey comes from user-secrets — never
/// from appsettings.json or code.
/// </summary>
public class AnthropicOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "claude-sonnet-4-6";

    public int MaxTokens { get; set; } = 1024;
}

/// <summary>
/// RAG configuration, bound from the "VectorStore" section. Defaults run Docker-free:
/// Provider = InMemory, embeddings from a local Ollama. VectorSize must equal the embedding
/// model's output dimensions (nomic-embed-text = 768) or Qdrant upserts fail.
/// </summary>
public class VectorStoreOptions
{
    /// <summary>"InMemory" (default, no Docker) or "Qdrant".</summary>
    public string Provider { get; set; } = "InMemory";

    public string QdrantHost { get; set; } = "localhost";

    public int QdrantPort { get; set; } = 6334; // gRPC — NOT the REST port 6333

    public string CollectionName { get; set; } = "demo5-books";

    public int VectorSize { get; set; } = 768;

    public string OllamaUrl { get; set; } = "http://localhost:11434";

    public string EmbeddingModel { get; set; } = "nomic-embed-text";

    public int ChunkSize { get; set; } = 500;

    public int ChunkOverlap { get; set; } = 80;

    public int TopK { get; set; } = 5;
}
