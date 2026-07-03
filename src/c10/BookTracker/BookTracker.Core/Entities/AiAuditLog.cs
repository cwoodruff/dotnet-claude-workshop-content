namespace BookTracker.Core.Entities;

/// <summary>
/// One row per AI call (chat / agent / recommend) — who, which feature/model, token usage, estimated
/// cost, and when. Answers "what did the AI do, for whom, at what cost?".
/// </summary>
public class AiAuditLog
{
    public int Id { get; set; }

    public string User { get; set; } = string.Empty;

    public string Feature { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int InputTokens { get; set; }

    public int OutputTokens { get; set; }

    public decimal CostUsd { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}
