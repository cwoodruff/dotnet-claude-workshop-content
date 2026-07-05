# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repository is

Content for the two-day **.NET AI with Claude** workshop:

- `decks/Day 1` and `decks/Day 2` — PowerPoint slide decks (binary, not editable here).
- `src/c0` … `src/c10` — eleven **frozen checkpoints** of the same BookTracker app, one per lab.
  Each `src/cN/BookTracker/` is a complete, independently buildable solution representing the
  expected state *after* that lab.

## Checkpoints are copies, not branches

The single most important thing to know: checkpoints do not share code. A fix to something present
in `c3` almost certainly needs to be replicated into `c4`–`c10` by hand (each later checkpoint is a
superset of the earlier ones). When changing shared code, diff neighboring checkpoints first
(`diff -rq src/cN/BookTracker src/cN+1/BookTracker`) to see where the file diverges.

What each checkpoint adds (Day 1 = Claude Code track, Day 2 = Anthropic API track):

| Checkpoint | Adds |
|---|---|
| c0 | Starter: Books + Authors API with **intentional teaching gaps** (validation holes, SQL-injection vector, entity-returning endpoint) |
| c1 | `CLAUDE.md` inside the app |
| c2 | `.claude/` (rules, agents, hooks, skills, plugin) + Reviews feature |
| c3 | `.mcp.json` (GitHub MCP) + `proposed-issues/` |
| c4 | Reading Progress feature, built from `specs/reading-progress.md` |
| c5 | Anthropic C# SDK (`Anthropic` NuGet), `/api/chat`, `ClaudeService` |
| c6 | Streaming, tool use, agent loop (`AgentService`, `Api/Tools/`) |
| c7 | RAG: `BookTracker.VectorStore` project, `/api/recommend`, Qdrant `docker-compose.yml` |
| c8 | `BookTracker.Mcp` — MCP server in C# (`ModelContextProtocol` SDK, Streamable HTTP) |
| c9 | AI testing: `IAgentLlm` abstraction, `Tests/Agent`, `Tests/Integration` (Testcontainers) |
| c10 | Responsible AI: prompt-injection defenses, `AiAuditService`, token-budget middleware, `Tests/Security` |

**Do not "fix" the teaching gaps in c0–c3** — attendees discover and fix them in the labs. The gaps
are removed progressively in later checkpoints; c10 is the fully hardened state.

## Commands

All commands run from inside a checkpoint, e.g. `cd src/c10/BookTracker`:

```bash
dotnet build BookTracker.sln
dotnet test BookTracker.sln
dotnet test --filter "FullyQualifiedName~ReviewServiceTests"   # single test class
dotnet run --project BookTracker.Api                            # http://localhost:5255
dotnet run --project BookTracker.Mcp                            # MCP server (c8+)

# EF Core migrations (dotnet tool install --global dotnet-ef)
dotnet ef migrations add <Name> --project BookTracker.Data --startup-project BookTracker.Api
```

External services (c7+): `docker compose up -d` starts Qdrant (6333 REST / 6334 gRPC — the .NET
client uses gRPC); embeddings come from a local Ollama (`ollama pull nomic-embed-text`). The
default `VectorStore:Provider` is `InMemory`, so the app runs without either. Integration tests in
c9/c10 use Testcontainers (require Docker) and the SQLite database is created, migrated, and seeded
automatically at startup.

The Anthropic API key (c5+) is bound from configuration `Anthropic:ApiKey` via user-secrets —
never committed.

## BookTracker architecture (all checkpoints)

Strict dependency direction, all projects referenced by `Tests`:

- **BookTracker.Core** — entities, DTOs (records), service interfaces + implementations, repository
  *ports* (`IBookRepository` etc.). Depends on nothing; stays free of EF Core.
- **BookTracker.Data** — EF Core `DbContext`, repository implementations, migrations, seed. → Core.
- **BookTracker.VectorStore** (c7+) — chunking, Ollama embeddings, InMemory/Qdrant stores. → Core.
- **BookTracker.Api** — Minimal API (no controllers), endpoint groups in `Endpoints/`, AI services
  in `Services/`. → Core, Data, VectorStore.
- **BookTracker.Mcp** (c8+) — MCP server reusing Data + Core services against the same SQLite db.

Conventions the codebase enforces (formalized in each checkpoint's own `CLAUDE.md` and
`.claude/rules/`): endpoints never return EF entities, only DTOs; endpoints stay thin
(parse → validate → service → map → typed `Results<...>`); each endpoint group's `Map…Endpoints`
is wired in `Program.cs`; all data access is async with `CancellationToken` threaded through;
schema changes go through EF migrations.

Note: from c2 on, each checkpoint contains its own `.claude/settings.json` with a `PostToolUse`
hook that runs `dotnet build` after edits, and a `PreToolUse` hook blocking destructive Bash —
these fire when you work *inside* a checkpoint directory.
