using BookTracker.Core.Entities;

namespace BookTracker.Core.Domain;

/// <summary>Outcome of applying a reading-progress update — success, or a validation message.</summary>
public readonly record struct RuleResult(bool Ok, string? Error)
{
    public static RuleResult Success() => new(true, null);

    public static RuleResult Fail(string error) => new(false, error);
}

/// <summary>
/// The reading-progress state machine and validation. Pure logic, no EF — lives in Core so it can be
/// unit-tested directly and reused by the Day 2 agent/MCP tools. The service calls <see cref="Apply"/>.
/// </summary>
public static class ReadingProgressRules
{
    /// <summary>
    /// Validate and apply an update to <paramref name="current"/> in place. On success the entity is
    /// mutated (page, status, dates); on failure it is left untouched and a message is returned.
    /// </summary>
    public static RuleResult Apply(
        ReadingProgress current,
        int requestedPage,
        ReadingStatus requestedStatus,
        int totalPages,
        DateOnly today)
    {
        // Page rule: 0 <= page <= TotalPages (inclusive).
        if (requestedPage < 0 || requestedPage > totalPages)
        {
            return RuleResult.Fail($"CurrentPage must be between 0 and {totalPages}.");
        }

        var from = current.Status;
        var to = requestedStatus;

        // Status state machine. Same → same is a no-op (page may still change). Forward-only;
        // Completed is terminal.
        if (from != to)
        {
            switch ((from, to))
            {
                case (ReadingStatus.WantToRead, ReadingStatus.Reading):
                    current.StartedOn ??= today;
                    break;

                case (ReadingStatus.Reading, ReadingStatus.Completed):
                    current.StartedOn ??= today;
                    current.FinishedOn = today;
                    if (current.FinishedOn < current.StartedOn)
                    {
                        return RuleResult.Fail("FinishedOn cannot be before StartedOn.");
                    }

                    break;

                case (ReadingStatus.WantToRead, ReadingStatus.Completed):
                    return RuleResult.Fail("Cannot skip from WantToRead to Completed — start reading first.");

                // Reading → WantToRead, Completed → Reading, Completed → WantToRead: all rejected.
                // Completed is terminal: this guard is what the Lab 4 Part C debugging exercise breaks.
                default:
                    return RuleResult.Fail($"Invalid status transition from {from} to {to}.");
            }
        }

        // Completing a book pins the page to the end; otherwise honor the requested page.
        current.CurrentPage = to == ReadingStatus.Completed ? totalPages : requestedPage;
        current.Status = to;
        return RuleResult.Success();
    }
}
