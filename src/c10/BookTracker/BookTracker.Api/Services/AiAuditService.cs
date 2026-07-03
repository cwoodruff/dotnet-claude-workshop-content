using BookTracker.Api.Options;
using BookTracker.Api.Security;
using BookTracker.Core.Entities;
using BookTracker.Core.Services;
using BookTracker.Data;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace BookTracker.Api.Services;

/// <summary>
/// Writes an AiAuditLog row for each AI call and adds its token spend to the user's daily budget.
/// </summary>
public class AiAuditService : IAiAuditService
{
    private readonly BookTrackerDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly AnthropicOptions _options;

    public AiAuditService(BookTrackerDbContext db, IDistributedCache cache, IOptions<AnthropicOptions> options)
    {
        _db = db;
        _cache = cache;
        _options = options.Value;
    }

    public async Task RecordAsync(
        string user,
        string feature,
        int inputTokens,
        int outputTokens,
        CancellationToken ct = default)
    {
        _db.AiAuditLogs.Add(new AiAuditLog
        {
            User = user,
            Feature = feature,
            Model = _options.Model,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            CostUsd = AiCost.Estimate(_options.Model, inputTokens, outputTokens),
            Timestamp = DateTimeOffset.UtcNow,
        });
        await _db.SaveChangesAsync(ct);

        await TokenBudget.AddAsync(_cache, user, inputTokens + outputTokens, ct);
    }
}
