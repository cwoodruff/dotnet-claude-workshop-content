using System.Text;
using Anthropic;
using Anthropic.Models.Messages;
using Demo5.Rag.Embeddings;
using Demo5.Rag.Options;
using Demo5.Rag.Stores;
using Microsoft.Extensions.Options;

namespace Demo5.Rag.Services;

/// <summary>
/// The grounded reply, the chunks/titles it drew on (empty when ungrounded), and token usage.
/// </summary>
public record RecommendResult(
    bool Grounded,
    string Reply,
    IReadOnlyList<string> Sources,
    IReadOnlyList<RetrievedChunk> RetrievedContext,
    int InputTokens,
    int OutputTokens);

/// <summary>A retrieved chunk surfaced in the response so the audience can see what grounding used.</summary>
public record RetrievedChunk(string Title, float Score, string Text);

/// <summary>
/// The point of the demo, side by side:
/// - Ungrounded: the query goes straight to Claude, which happily invents plausible titles.
/// - Grounded (RAG): embed the query, retrieve top-K similar catalog chunks by cosine similarity,
///   and put them in the system block so Claude answers from real data only.
/// </summary>
public class RecommendService
{
    private readonly IEmbeddingService _embeddings;
    private readonly IVectorStore _store;
    private readonly AnthropicClient _client;
    private readonly AnthropicOptions _anthropic;
    private readonly VectorStoreOptions _vectorOptions;

    public RecommendService(
        IEmbeddingService embeddings,
        IVectorStore store,
        AnthropicClient client,
        IOptions<AnthropicOptions> anthropic,
        IOptions<VectorStoreOptions> vectorOptions)
    {
        _embeddings = embeddings;
        _store = store;
        _client = client;
        _anthropic = anthropic.Value;
        _vectorOptions = vectorOptions.Value;
    }

    public Task<RecommendResult> RecommendAsync(string query, bool grounded, CancellationToken ct = default)
        => grounded ? RecommendGroundedAsync(query, ct) : RecommendUngroundedAsync(query, ct);

    /// <summary>No retrieval: Claude is told a book catalog exists but never sees it, so it guesses.</summary>
    private async Task<RecommendResult> RecommendUngroundedAsync(string query, CancellationToken ct)
    {
        const string systemPrompt =
            "You are the recommendation assistant for BookTracker, a personal book-catalog app. "
            + "Recommend two or three books from the user's BookTracker catalog that fit their "
            + "request, citing each catalog title you recommend.";

        var response = await _client.Messages.Create(new MessageCreateParams
        {
            Model = _anthropic.Model,
            MaxTokens = _anthropic.MaxTokens,
            System = new List<TextBlockParam> { new() { Text = systemPrompt } },
            Messages = [new() { Role = Role.User, Content = query }],
        }, cancellationToken: ct);

        return new RecommendResult(
            Grounded: false,
            Reply: ExtractText(response),
            Sources: [],
            RetrievedContext: [],
            InputTokens: (int)response.Usage.InputTokens,
            OutputTokens: (int)response.Usage.OutputTokens);
    }

    /// <summary>RAG: embed → top-K cosine retrieval → augmented prompt → grounded answer.</summary>
    private async Task<RecommendResult> RecommendGroundedAsync(string query, CancellationToken ct)
    {
        var queryVector = await _embeddings.EmbedAsync(query, ct);
        var hits = await _store.SearchAsync(queryVector, _vectorOptions.TopK, ct);

        var context = new StringBuilder();
        context.AppendLine(
            "You are the recommendation assistant for BookTracker, a personal book-catalog app. "
            + "Recommend books using ONLY the retrieved context below. If the context does not "
            + "contain a good match, say so rather than inventing titles. Cite the book titles "
            + "you draw on.");
        context.AppendLine();
        context.AppendLine("Retrieved context:");
        foreach (var hit in hits)
        {
            context.AppendLine($"- [{hit.Title}] {hit.Text}");
        }

        // Instructions + retrieved context are the static prefix — mark them cacheable so repeat
        // queries against the same context bill cached-input rates (ties back to Section 1).
        var response = await _client.Messages.Create(new MessageCreateParams
        {
            Model = _anthropic.Model,
            MaxTokens = _anthropic.MaxTokens,
            System = new List<TextBlockParam>
            {
                new() { Text = context.ToString(), CacheControl = new CacheControlEphemeral() },
            },
            Messages = [new() { Role = Role.User, Content = query }],
        }, cancellationToken: ct);

        return new RecommendResult(
            Grounded: true,
            Reply: ExtractText(response),
            Sources: hits.Select(h => h.Title).Distinct().ToList(),
            RetrievedContext: hits.Select(h => new RetrievedChunk(h.Title, h.Score, h.Text)).ToList(),
            InputTokens: (int)response.Usage.InputTokens,
            OutputTokens: (int)response.Usage.OutputTokens);
    }

    private static string ExtractText(Message response)
        => string.Concat(response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));
}
