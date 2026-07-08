# BookTracker — Checkpoint c1 (Project memory)

The sample application for the **.NET AI with Claude Workshop**. An ASP.NET Core 10 Minimal API
for tracking books, backed by EF Core 10 and SQLite.

> **c1 — project memory.** *Day 1 · Section 1 — Foundations.* This is `c0` after **Day 1, Lab 1**:
> the solution now carries a real [`CLAUDE.md`](./CLAUDE.md) so Claude Code understands the app's
> stack, architecture, commands, and house rules on every turn — without you re-explaining them.

## What's new in this checkpoint

- **`CLAUDE.md`** at the solution root — the project-memory file Claude Code auto-loads into context.
  It captures:
  - the **stack** (.NET 10 Minimal API, EF Core 10 + SQLite, xUnit),
  - the **build / test / run / migration commands**,
  - the **four-project architecture** and its deliberate dependency direction, and
  - the **conventions** (return DTOs never entities, validate at the endpoint, keep handlers thin,
    parameterized queries only, async all the way, schema changes via migrations).

Everything else is unchanged from `c0` — same Books + Authors API, same intentional
[teaching gaps](#teaching-gaps).

## Why it matters

`CLAUDE.md` is the single highest-leverage steering file in Claude Code. Anything you'd otherwise
repeat in every prompt — "we use Minimal APIs, not controllers," "never return EF entities" — belongs
here once. Read it, then notice how much less you have to spell out for the rest of Day 1. In `c2`
these conventions get *split out* into path-scoped rules under `.claude/rules/`.

## Stack

- **.NET 10** · **ASP.NET Core Minimal API** · **EF Core 10 / SQLite** · **xUnit**

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) · [Claude Code](https://claude.com/claude-code)

## Getting started

```bash
cd src/c1/BookTracker

dotnet build BookTracker.sln
dotnet test BookTracker.sln
dotnet run --project BookTracker.Api   # http://localhost:5255  (OpenAPI at /openapi)
```

The SQLite database is created, migrated, and seeded automatically on first run.

## Project layout

```text
BookTracker.sln
CLAUDE.md                     # ← NEW: project memory Claude Code loads every session
BookTracker.Core/             # entities, DTOs, interfaces, services  (depends on nothing)
BookTracker.Data/             # EF Core DbContext, repository, migrations, seed  (→ Core)
BookTracker.Api/              # Minimal API endpoints + DI wiring  (→ Core + Data)
BookTracker.Tests/            # xUnit tests
```

## API endpoints

Unchanged from `c0`: **Books** under `/api/books` (list, get, `search?q=`, create, update, delete)
and **Authors** under `/api/authors` (list, get, create, update, delete). See
[`c0`'s README](../../c0/BookTracker/README.md#api-endpoints) for the full table and examples.

## Try it

Ask Claude Code something whose answer should now come from `CLAUDE.md` — e.g.
*"How do I add a migration in this repo?"* or *"What's our rule about returning EF entities?"* — and
watch it answer from project memory instead of guessing.

## Teaching gaps

The intentional flaws from `c0` are still here (validation holes, an SQL-injection vector, endpoints
returning EF entities). Fixing them is the work of later Day 1 labs — don't pre-emptively clean them
up. The Reading Progress feature is still intentionally absent (it's built in `c4`).

## Workshop resources

- **Guide:** [dotnetclaude.com](https://dotnetclaude.com) — Day 1, Section 1 (Foundations).
- **Deck:** `decks/Day 1/Section1-Foundations-Claude-Code-CLI.pptx`
- **`CLAUDE.md` / memory docs:** <https://docs.claude.com/en/docs/claude-code/memory>
- **Previous:** [`c0`](../../c0/BookTracker/README.md) — the raw starter.
- **Next:** [`c2`](../../c2/BookTracker/README.md) — add `.claude/` steering (rules, agents, hooks, skills, a plugin) and the Reviews feature.
