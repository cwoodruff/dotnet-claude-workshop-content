using Anthropic;
using BookTracker.Api.Endpoints;
using BookTracker.Api.Options;
using BookTracker.Api.Services;
using BookTracker.Core.Services;
using BookTracker.Data;
using BookTracker.VectorStore;
using BookTracker.VectorStore.Chunking;
using BookTracker.VectorStore.Embeddings;
using BookTracker.VectorStore.Ingestion;
using BookTracker.VectorStore.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("BookTracker")
    ?? "Data Source=booktracker.db";

builder.Services.AddBookTrackerData(connectionString);
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IReadingProgressService, ReadingProgressService>();

// --- Day 2: Claude SDK chat ---
builder.Services.Configure<AnthropicOptions>(builder.Configuration.GetSection("Anthropic"));

// AnthropicClient is a Singleton — it holds an HttpClient; Scoped/Transient risks socket exhaustion.
// (The SDK already retries 429/5xx with backoff, so no extra resilience pipeline is wired here.)
builder.Services.AddSingleton(sp =>
    new AnthropicClient { ApiKey = sp.GetRequiredService<IOptions<AnthropicOptions>>().Value.ApiKey });
builder.Services.AddScoped<IClaudeService, ClaudeService>();

// In-memory IDistributedCache for chat history — zero infra. Swap AddStackExchangeRedisCache for prod.
builder.Services.AddDistributedMemoryCache();

// Streaming + tool-calling agent. BookTrackerTools dispatches to the real Core services
// (update_reading_progress calls the C4 IReadingProgressService — Day 1's feature is the agent's write surface).
builder.Services.AddScoped<BookTracker.Api.Tools.BookTrackerTools>();
builder.Services.AddScoped<IStreamingService, StreamingService>();
builder.Services.AddScoped<IAgentService, AgentService>();

// --- Day 2: RAG (BookTracker.VectorStore) ---
builder.Services.Configure<VectorStoreOptions>(builder.Configuration.GetSection("VectorStore"));

// Embeddings via the Microsoft.Extensions.AI seam. Default: local Ollama (free). Swap the generator
// registration for OpenAI without touching anything downstream.
builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
{
    var o = sp.GetRequiredService<IOptions<VectorStoreOptions>>().Value;
    return new OllamaApiClient(new Uri(o.OllamaUrl), o.EmbeddingModel);
});
builder.Services.AddSingleton<IEmbeddingService, EmbeddingService>();
builder.Services.AddSingleton<ITextChunker, TextChunker>();

// Vector store: in-memory by default (no Docker), Qdrant when configured.
var vectorProvider = builder.Configuration.GetValue<string>("VectorStore:Provider") ?? "InMemory";
if (vectorProvider.Equals("Qdrant", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IVectorStore, QdrantVectorStore>();
}
else
{
    builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
}

builder.Services.AddScoped<CorpusIngestionService>();
builder.Services.AddScoped<IRagService, RagService>();

var app = builder.Build();

// Apply migrations and seed at startup so the app is runnable straight from a clone.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookTrackerDbContext>();
    db.Database.Migrate();

    // Best-effort RAG corpus ingestion. Skips silently if the embedding provider (Ollama) is
    // unreachable so the app still runs from a clone without Docker/Ollama; /api/recommend works
    // once ingestion succeeds.
    var ingestion = scope.ServiceProvider.GetRequiredService<CorpusIngestionService>();
    try
    {
        var count = await ingestion.IngestAsync();
        app.Logger.LogInformation("RAG corpus ingested: {Count} chunks.", count);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex,
            "RAG corpus ingestion skipped (is Ollama running at the configured URL?). " +
            "/api/recommend will be unavailable until ingestion succeeds.");
    }
}

app.MapGet("/", () => "BookTracker API");
app.MapBookEndpoints();
app.MapAuthorEndpoints();
app.MapReviewsEndpoints();
app.MapReadingProgressEndpoints();
app.MapChatEndpoints();
app.MapAgentEndpoints();
app.MapRecommendEndpoints();

app.Run();

// Exposed so integration tests can use WebApplicationFactory<Program>.
public partial class Program;
