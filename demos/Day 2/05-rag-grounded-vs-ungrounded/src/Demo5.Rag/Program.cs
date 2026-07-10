using Anthropic;
using Demo5.Rag.Chunking;
using Demo5.Rag.Embeddings;
using Demo5.Rag.Ingestion;
using Demo5.Rag.Options;
using Demo5.Rag.Services;
using Demo5.Rag.Stores;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);

// User-secrets are auto-loaded only in the Development environment. Register the provider
// explicitly so "Anthropic:ApiKey" resolves no matter which environment the demo runs under.
builder.Configuration.AddUserSecrets<Program>();

builder.Services.Configure<AnthropicOptions>(builder.Configuration.GetSection("Anthropic"));
builder.Services.Configure<VectorStoreOptions>(builder.Configuration.GetSection("VectorStore"));

// AnthropicClient is a Singleton — it holds an HttpClient; Scoped/Transient risks socket exhaustion.
// The key comes from user-secrets: dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-..."
builder.Services.AddSingleton(sp =>
    new AnthropicClient { ApiKey = sp.GetRequiredService<IOptions<AnthropicOptions>>().Value.ApiKey });

// Embeddings via the Microsoft.Extensions.AI seam: local Ollama running nomic-embed-text (768-dim).
builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
{
    var o = sp.GetRequiredService<IOptions<VectorStoreOptions>>().Value;
    return new OllamaApiClient(new Uri(o.OllamaUrl), o.EmbeddingModel);
});
builder.Services.AddSingleton<IEmbeddingService, EmbeddingService>();
builder.Services.AddSingleton<ITextChunker, TextChunker>();

// Vector store: Qdrant (Docker, gRPC on 6334) when configured; in-memory fallback otherwise.
// The fallback is also the live-demo rescue if Docker is down — flip appsettings and restart.
var vectorProvider = builder.Configuration.GetValue<string>("VectorStore:Provider") ?? "InMemory";
if (vectorProvider.Equals("Qdrant", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IVectorStore, QdrantVectorStore>();
}
else
{
    builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
}

builder.Services.AddSingleton<IngestionService>();
builder.Services.AddSingleton<RecommendService>();

var app = builder.Build();

// Best-effort corpus ingestion at startup so the demo runs straight from a clone. Skips silently
// if the embedding provider (Ollama) or Qdrant is unreachable; POST /api/ingest retries later.
try
{
    var count = await app.Services.GetRequiredService<IngestionService>().IngestAsync();
    app.Logger.LogInformation(
        "RAG corpus ready ({Provider}): {Count} chunks ingested (0 = already populated).",
        vectorProvider, count);
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex,
        "Corpus ingestion skipped — is Ollama running (and Qdrant, if Provider=Qdrant)? "
        + "Grounded answers will be empty until POST /api/ingest succeeds.");
}

app.MapGet("/", (IOptions<VectorStoreOptions> o) => new
{
    demo = "Day 2 Demo 5 — RAG: grounded vs ungrounded",
    provider = vectorProvider,
    embeddingModel = o.Value.EmbeddingModel,
    vectorSize = o.Value.VectorSize,
    usage = "GET /api/recommend?q=<query>&grounded=true|false · POST /api/ingest?force=true",
});

// The demo endpoint: same query, flip `grounded` to contrast invented vs cited answers.
app.MapGet("/api/recommend", async Task<Results<Ok<RecommendResult>, ValidationProblem>> (
    string? q, bool grounded, RecommendService recommend, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(q))
    {
        return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            ["q"] = ["Query is required, e.g. /api/recommend?q=a%20classic%20desert%20epic&grounded=true"],
        });
    }

    var result = await recommend.RecommendAsync(q, grounded, ct);
    return TypedResults.Ok(result);
});

// Re-run ingestion on demand (e.g. after starting Ollama/Qdrant, or with force=true to re-embed).
app.MapPost("/api/ingest", async (bool force, IngestionService ingestion, CancellationToken ct) =>
{
    var count = await ingestion.IngestAsync(force, ct);
    return TypedResults.Ok(new { chunksIngested = count, note = count == 0 ? "already populated (use ?force=true to re-embed)" : "ok" });
});

app.Run();
