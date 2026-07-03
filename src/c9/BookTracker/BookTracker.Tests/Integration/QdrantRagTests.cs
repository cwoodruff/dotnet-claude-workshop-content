using BookTracker.VectorStore;
using BookTracker.VectorStore.Stores;
using Microsoft.Extensions.Options;
using Testcontainers.Qdrant;

namespace BookTracker.Tests.Integration;

/// <summary>
/// Exercises the C7 QdrantVectorStore against a real Qdrant in Docker (Testcontainers). Uses
/// hand-built vectors (no Ollama) so it tests the store's upsert + cosine search directly.
/// </summary>
public class QdrantRagTests
{
    [DockerFact]
    public async Task UpsertThenSearch_ReturnsNearestNeighbour()
    {
        await using var container = new QdrantBuilder().Build();
        await container.StartAsync();

        var options = Options.Create(new VectorStoreOptions
        {
            Provider = "Qdrant",
            QdrantHost = container.Hostname,
            QdrantPort = container.GetMappedPublicPort(6334),
            CollectionName = "books-test",
            VectorSize = 3,
        });

        var store = new QdrantVectorStore(options);
        await store.EnsureCollectionAsync(3);

        await store.UpsertAsync(
        [
            new VectorRecord(Guid.NewGuid(), [1f, 0f, 0f], 1, "Alpha", "about alpha"),
            new VectorRecord(Guid.NewGuid(), [0f, 1f, 0f], 2, "Beta", "about beta"),
            new VectorRecord(Guid.NewGuid(), [0f, 0f, 1f], 3, "Gamma", "about gamma"),
        ]);

        Assert.Equal(3, await store.CountAsync());

        // Query closest to the "Alpha" vector.
        var hits = await store.SearchAsync([0.9f, 0.1f, 0f], topK: 1);

        var hit = Assert.Single(hits);
        Assert.Equal(1, hit.BookId);
        Assert.Equal("Alpha", hit.Title);
    }
}
