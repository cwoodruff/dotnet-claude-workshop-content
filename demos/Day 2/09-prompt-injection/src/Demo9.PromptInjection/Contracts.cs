namespace Demo9.PromptInjection;

/// <summary>Request body for both summarize endpoints: the raw text a user typed.</summary>
public record SummarizeRequest(string Text);

/// <summary>
/// Response for both endpoints. <c>SentToModel</c> echoes the exact prompt construction so the
/// instructor can show, on screen, how naive vs. hardened differ — the teaching is the structure.
/// </summary>
public record SummarizeResponse(
    string Endpoint,
    string Reply,
    long InputTokens,
    long OutputTokens,
    string SentToModel);
