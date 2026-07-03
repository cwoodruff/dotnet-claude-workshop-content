using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace BookTracker.VectorStore.Chunking;

/// <summary>
/// Sentence-aware chunker: packs whole sentences into ~ChunkSize-character windows with ChunkOverlap
/// characters carried into the next window, so context isn't cut mid-sentence.
/// </summary>
public partial class TextChunker : ITextChunker
{
    private readonly VectorStoreOptions _options;

    public TextChunker(IOptions<VectorStoreOptions> options)
    {
        _options = options.Value;
    }

    public IReadOnlyList<string> Chunk(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var size = Math.Max(1, _options.ChunkSize);
        var overlap = Math.Clamp(_options.ChunkOverlap, 0, size - 1);

        var sentences = SentenceSplitter().Split(text.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .ToList();

        var chunks = new List<string>();
        var current = new StringBuilder();

        foreach (var sentence in sentences)
        {
            if (current.Length > 0 && current.Length + sentence.Length + 1 > size)
            {
                chunks.Add(current.ToString().Trim());

                // Carry the tail of the finished chunk into the next one for overlap.
                var tail = current.ToString();
                tail = tail.Length > overlap ? tail[^overlap..] : tail;
                current.Clear();
                if (overlap > 0)
                {
                    current.Append(tail).Append(' ');
                }
            }

            current.Append(sentence).Append(' ');
        }

        if (current.Length > 0)
        {
            chunks.Add(current.ToString().Trim());
        }

        return chunks;
    }

    // Split on sentence-ending punctuation followed by whitespace.
    [GeneratedRegex(@"(?<=[.!?])\s+")]
    private static partial Regex SentenceSplitter();
}
