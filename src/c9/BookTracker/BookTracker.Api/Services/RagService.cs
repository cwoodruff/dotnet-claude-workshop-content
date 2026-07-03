using System.Text;
using Anthropic;
using Anthropic.Models.Messages;
using BookTracker.Api.Options;
using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using BookTracker.VectorStore;
using BookTracker.VectorStore.Embeddings;
using BookTracker.VectorStore.Stores;
using Microsoft.Extensions.Options;

namespace BookTracker.Api.Services;

/// <summary>
/// RAG pipeline: embed the query, retrieve top-K similar chunks, build an augmented prompt (retrieved
/// context goes in the cached system block), and ask Claude for a grounded recommendation.
/// </summary>
public class RagService : IRagService
{
    private readonly IEmbeddingService _embeddings;
    private readonly IVectorStore _store;
    private readonly AnthropicClient _client;
    private readonly AnthropicOptions _anthropic;
    private readonly VectorStoreOptions _vectorOptions;

    public RagService(
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

    public async Task<RecommendResult> RecommendAsync(string query, CancellationToken ct = default)
    {
        var queryVector = await _embeddings.EmbedAsync(query, ct);
        var hits = await _store.SearchAsync(queryVector, _vectorOptions.TopK, ct);

        var context = new StringBuilder();
        context.AppendLine("You recommend books using ONLY the retrieved context below. If the context "
            + "does not contain a good match, say so rather than inventing titles. Cite the book titles "
            + "you draw on.");
        context.AppendLine();
        context.AppendLine("Retrieved context:");
        foreach (var hit in hits)
        {
            context.AppendLine($"- [{hit.Title}] {hit.Text}");
        }

        // The retrieved context + instructions are the static prefix — cache it (C5 prompt caching).
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

        var reply = string.Concat(
            response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));

        var sources = hits.Select(h => h.Title).Distinct().ToList();
        return new RecommendResult(reply, sources);
    }
}
