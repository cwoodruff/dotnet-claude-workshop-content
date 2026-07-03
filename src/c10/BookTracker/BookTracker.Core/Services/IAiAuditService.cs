namespace BookTracker.Core.Services;

/// <summary>
/// Records an AI call to the audit log and adds its token spend to the caller's daily budget.
/// Pure interface (primitives only) — the impl lives in Api.
/// </summary>
public interface IAiAuditService
{
    Task RecordAsync(
        string user,
        string feature,
        int inputTokens,
        int outputTokens,
        CancellationToken ct = default);
}
