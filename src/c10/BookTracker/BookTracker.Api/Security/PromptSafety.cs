using System.Text.RegularExpressions;

namespace BookTracker.Api.Security;

/// <summary>
/// Prompt-injection defense via structural separation: untrusted user text is sanitized and wrapped in
/// a labeled &lt;user_input&gt; block placed in the USER turn — never concatenated into the system prompt.
/// The system prompt instructs the model to treat everything inside the tags as data, not instructions.
/// </summary>
public static partial class PromptSafety
{
    public static string Wrap(string raw) => $"<user_input>\n{Sanitize(raw)}\n</user_input>";

    /// <summary>Strip null/control characters (keep tab/newline) so input can't smuggle control bytes.</summary>
    public static string Sanitize(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }

        // Defensively neutralize any attempt to forge the closing delimiter.
        s = s.Replace("</user_input>", "<\\/user_input>", StringComparison.OrdinalIgnoreCase);
        return ControlChars().Replace(s, string.Empty);
    }

    // Control characters except tab (09), newline (0A), carriage return (0D).
    [GeneratedRegex(@"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]")]
    private static partial Regex ControlChars();
}
