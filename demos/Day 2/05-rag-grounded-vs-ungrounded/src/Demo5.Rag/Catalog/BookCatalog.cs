using System.Text.Json;

namespace Demo5.Rag.Catalog;

/// <summary>One entry in the embedded book catalog (Data/books.json).</summary>
public record CatalogBook(int Id, string Title, string Author, string Genre, string Description);

/// <summary>
/// Loads the embedded book catalog that stands in for BookTracker's database in this demo.
/// The JSON file is copied to the output directory at build time.
/// </summary>
public static class BookCatalog
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static async Task<IReadOnlyList<CatalogBook>> LoadAsync(CancellationToken ct = default)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "books.json");
        await using var stream = File.OpenRead(path);
        var books = await JsonSerializer.DeserializeAsync<List<CatalogBook>>(stream, Options, ct);
        return books ?? [];
    }
}
