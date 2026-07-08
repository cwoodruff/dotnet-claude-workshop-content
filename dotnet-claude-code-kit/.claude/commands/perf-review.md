---
description: Quick performance review of recent changes (EF queries, async, allocations)
---

Review $ARGUMENTS (default: uncommitted changes; if clean, the last commit) for performance issues.

Focus on the high-payoff categories for an ASP.NET Core + EF Core app: N+1 query patterns, missing AsNoTracking on reads, full-entity loads where projections suffice, queries inside loops, sync-over-async, sequential awaits that are safely parallelizable, allocations in hot paths, and missing pagination on growing collections.

Output: findings ranked by expected impact, each with location, mechanism (why it is slow), and a concrete fix. If nothing significant, say so in one line — do not manufacture findings. For deep analysis of the whole codebase, suggest delegating to the perf-analyzer subagent instead.
