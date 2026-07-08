# BookTracker — Checkpoint c4 (Spec-driven feature)

The sample application for the **.NET AI with Claude Workshop**. An ASP.NET Core 10 Minimal API
for tracking books, backed by EF Core 10 and SQLite.

> **c4 — Reading Progress, built from a spec.** *Day 1 · Section 4 — AI in the SDLC.* This is `c3`
> after **Day 1, Lab 4**: the previously-absent **Reading Progress** feature is now implemented —
> driven end-to-end by a written specification in [`specs/reading-progress.md`](./specs/reading-progress.md).
> This is the capstone of Day 1: spec → code → tests, with Claude Code doing the build.

## What's new in this checkpoint

### `specs/reading-progress.md`

A complete, testable feature spec — the input to the lab. It defines the context/scope, the exact API
contract, the data model, the **business rules** (a forward-only status state machine), the required
tests, and explicit prohibitions. Read it alongside the code to see how a good spec constrains the
implementation.

### Reading Progress feature (new)

One progress record per book — current page, status, and start/finish dates:

- `ReadingProgress` + `ReadingStatus` entities, `ReadingProgressDto`.
- `ReadingProgressRules` domain logic in `BookTracker.Core/Domain/` — the state machine and page
  bounds live here, not in the handler.
- `IReadingProgressRepository` / `ReadingProgressRepository`,
  `IReadingProgressService` / `ReadingProgressService`.
- `ReadingProgressEndpoints` (nested under a book).
- EF Core migration `AddReadingProgress` (FK to Books, unique index on `BookId`).
- `ReadingProgressTests` covering the rules from the spec.

**Business rules (from the spec):**

- Page bounds: `0 <= CurrentPage <= Book.TotalPages` (inclusive).
- Status state machine (forward-only): `WantToRead → Reading → Completed`; same→same is a no-op;
  skipping (`WantToRead → Completed`) is rejected; `Completed` is **terminal** (no going back).
- Entering `Reading` sets `StartedOn` (today, if unset); entering `Completed` sets `FinishedOn` and
  forces `CurrentPage = TotalPages`; `FinishedOn >= StartedOn`.

## Getting started

```bash
cd src/c4/BookTracker

dotnet build BookTracker.sln
dotnet test BookTracker.sln
dotnet run --project BookTracker.Api   # http://localhost:5255  (OpenAPI at /openapi)
```

## Project layout

```text
BookTracker.sln
CLAUDE.md   .claude/   .mcp.json   proposed-issues/
specs/
└── reading-progress.md         # ← NEW: the feature spec that drove this checkpoint
BookTracker.Core/               # + ReadingProgress/ReadingStatus, ReadingProgressRules, DTO, interfaces, service
BookTracker.Data/               # + ReadingProgressRepository, AddReadingProgress migration
BookTracker.Api/                # + Endpoints/ReadingProgressEndpoints.cs
BookTracker.Tests/              # + ReadingProgressTests.cs
```

## API endpoints

Base **Books**, **Authors**, and **Reviews** are unchanged. New in `c4`:

**Reading Progress** — nested under a book at `/api/books/{id:int}/reading-progress`:

| Method | Route                                    | Description                                                    |
|--------|------------------------------------------|----------------------------------------------------------------|
| GET    | `/api/books/{id}/reading-progress`       | Get the book's progress (`200` / `404` if no record or book)   |
| PUT    | `/api/books/{id}/reading-progress`       | Set progress `{ currentPage, status }` (`200` / `400` rule violation / `404`) |

## Try it

```bash
# Start reading a book, then mark it completed
curl -X PUT http://localhost:5255/api/books/1/reading-progress \
  -H "Content-Type: application/json" -d '{"currentPage":40,"status":"Reading"}'

# Rule violation — skipping straight to Completed is rejected with 400
curl -X PUT http://localhost:5255/api/books/2/reading-progress \
  -H "Content-Type: application/json" -d '{"currentPage":0,"status":"Completed"}'
```

## Day 1 complete

`c4` is the end of the Day 1 track: you've gone from a raw starter (`c0`) to a steered, MCP-connected
project that ships features from specs. This same Reading Progress data is what Day 2's AI features
(agent tools, MCP server) build on.

## Workshop resources

- **Guide:** [dotnetclaude.com](https://dotnetclaude.com) — Day 1, Section 4 (AI in the SDLC).
- **Deck:** `decks/Day 1/Section4-AI-in-the-SDLC.pptx`
- **Spec-driven development:** see [`specs/reading-progress.md`](./specs/reading-progress.md) and the
  original proposal in [`proposed-issues/reading-progress.md`](./proposed-issues/reading-progress.md).
- **Previous:** [`c3`](../../c3/BookTracker/README.md) — MCP integration.
- **Next (Day 2):** [`c5`](../../c5/BookTracker/README.md) — call Claude from C# with the Anthropic SDK.
