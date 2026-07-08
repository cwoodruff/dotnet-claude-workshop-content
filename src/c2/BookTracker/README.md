# BookTracker — Checkpoint c2 (Steering with `.claude/`)

The sample application for the **.NET AI with Claude Workshop**. An ASP.NET Core 10 Minimal API
for tracking books, backed by EF Core 10 and SQLite.

> **c2 — steering kit + Reviews feature.** *Day 1 · Section 2 — Steering Claude Code.* This is `c1`
> after **Day 1, Lab 2**: a full `.claude/` directory now steers Claude Code with path-scoped rules,
> a custom sub-agent, hooks, a skill, and a plugin — and the app gains its first *new* feature,
> **book Reviews**, built with that steering in place.

## What's new in this checkpoint

### `.claude/` steering directory

```text
.claude/
├── settings.json                       # hooks wiring (see below)
├── rules/
│   ├── api-endpoints.md                # path-scoped rules for BookTracker.Api endpoints
│   └── data-access.md                  # path-scoped rules for BookTracker.Data
├── agents/
│   └── test-writer.md                  # a custom sub-agent for writing xUnit tests
├── hooks/
│   └── block-destructive.sh            # PreToolUse guard against destructive Bash commands
├── skills/
│   └── add-api-endpoint/               # a reusable skill: route + handler + DTOs + test
│       ├── SKILL.md
│       └── references/endpoint-pattern.md
└── plugins/
    └── booktracker-kit/plugin.json     # bundles the rules, skill, and guardrail as one plugin
```

- **Rules** — conventions moved out of `CLAUDE.md` into `.claude/rules/`, loaded automatically only
  when Claude works in the matching files (`api-endpoints.md`, `data-access.md`). `CLAUDE.md` now just
  points at them.
- **`settings.json` hooks** — a **PreToolUse** hook runs `block-destructive.sh` before any `Bash`
  call, and a **PostToolUse** hook runs `dotnet build` after every `Edit`/`Write` so a broken change
  surfaces immediately.
- **`test-writer` agent** — a focused sub-agent you can dispatch to write tests in the house style.
- **`add-api-endpoint` skill** — a repeatable procedure for adding an endpoint (DTOs → handler →
  route wiring → test), with a reference pattern doc.
- **`booktracker-kit` plugin** — packages the rules, the skill, and the destructive-command guardrail
  so the whole steering kit can be shared as one unit.

### Reviews feature (new)

The first feature built *with* the steering kit — a one-to-many **Review** per book:

- `Review` entity, `ReviewDto`, `IReviewRepository` + `ReviewRepository`, `IReviewService` +
  `ReviewService`, and a `ReadingReviews` endpoint group.
- EF Core migration `AddReviews`.
- `ReviewServiceTests` in `BookTracker.Tests`.

## Getting started

```bash
cd src/c2/BookTracker

dotnet build BookTracker.sln
dotnet test BookTracker.sln
dotnet run --project BookTracker.Api   # http://localhost:5255  (OpenAPI at /openapi)
```

The SQLite database is created, migrated (now including `AddReviews`), and seeded automatically.

## Project layout

```text
BookTracker.sln
CLAUDE.md                     # now delegates conventions to .claude/rules/
.claude/                      # ← NEW: rules, agent, hooks, skill, plugin
BookTracker.Core/             # + Review entity, ReviewDto, IReviewRepository, IReviewService
BookTracker.Data/             # + ReviewRepository, AddReviews migration
BookTracker.Api/              # + Endpoints/ReviewsEndpoints.cs
BookTracker.Tests/            # + ReviewServiceTests.cs
```

## API endpoints

Base **Books** and **Authors** endpoints are unchanged from
[`c0`](../../c0/BookTracker/README.md#api-endpoints). New in `c2`:

**Reviews** — nested under a book at `/api/books/{id:int}/reviews`:

| Method | Route                              | Description                                  |
|--------|------------------------------------|----------------------------------------------|
| GET    | `/api/books/{id}/reviews`          | List a book's reviews (404 if book missing)  |

(Plus the create/read handlers wired in `ReviewsEndpoints.cs` — explore the file to see the full set.)

## Try it

Trigger the steering you just added:

- Ask Claude Code to *"add an endpoint"* and watch it invoke the **`add-api-endpoint`** skill.
- Edit a file and watch the **PostToolUse** `dotnet build` hook run automatically.
- Ask it to run a destructive `rm`-style command and watch the **PreToolUse** guard block it.

## Teaching gaps

Intentional flaws from the starter still remain in the pre-existing endpoints — they're fixed in later
Day 1 work, not here. Reading Progress is still absent (built in `c4`).

## Workshop resources

- **Guide:** [dotnetclaude.com](https://dotnetclaude.com) — Day 1, Section 2 (Steering Claude Code).
- **Deck:** `decks/Day 1/Section2-Steering-Claude-Code.pptx`
- **Reference card:** `docs/steering-reference-card.pdf`
- **Docs:** [Rules & settings](https://docs.claude.com/en/docs/claude-code/settings) ·
  [Subagents](https://docs.claude.com/en/docs/claude-code/sub-agents) ·
  [Hooks](https://docs.claude.com/en/docs/claude-code/hooks) ·
  [Skills](https://docs.claude.com/en/docs/claude-code/skills) ·
  [Plugins](https://docs.claude.com/en/docs/claude-code/plugins)
- **Previous:** [`c1`](../../c1/BookTracker/README.md) — project memory via `CLAUDE.md`.
- **Next:** [`c3`](../../c3/BookTracker/README.md) — connect the GitHub MCP server with `.mcp.json`.
