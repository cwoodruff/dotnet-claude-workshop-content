# BookTracker — Checkpoint c3 (MCP integration)

The sample application for the **.NET AI with Claude Workshop**. An ASP.NET Core 10 Minimal API
for tracking books, backed by EF Core 10 and SQLite.

> **c3 — MCP integration.** *Day 1 · Section 3 — IDE Integration & MCP.* This is `c2` after
> **Day 1, Lab 3**: the project now declares an **MCP server** in [`.mcp.json`](./.mcp.json), giving
> Claude Code tools to read and write GitHub — and a `proposed-issues/` folder that captures the
> lab's output (and an offline fallback for it).

## What's new in this checkpoint

- **`.mcp.json`** — project-scoped MCP configuration wiring the **GitHub MCP server** (HTTP transport)
  into Claude Code:

  ```json
  {
    "mcpServers": {
      "github": {
        "type": "http",
        "url": "https://api.githubcopilot.com/mcp/",
        "headers": { "Authorization": "Bearer ${GITHUB_PAT}" }
      }
    }
  }
  ```

  With this connected, Claude Code can browse repos, read issues, and (given a token with the right
  scope) file issues — directly from your session. Set a `GITHUB_PAT` environment variable so the
  `${GITHUB_PAT}` placeholder resolves.

- **`proposed-issues/`** — the artifact of the lab:
  - `reading-progress.md` — a fully-written GitHub issue body proposing the **Reading Progress**
    feature. It doubles as an **offline fallback**: when the network is down and the GitHub MCP
    server can't reach the API, writing the issue here *is* the "action" half of the lab. The feature
    itself is implemented next, in `c4`.
  - `recent-history.txt` — supporting context for the proposed issue.

Application code is unchanged from `c2` — this checkpoint is about *connecting Claude Code to an
external system*, not shipping a feature.

## Getting started

```bash
cd src/c3/BookTracker

dotnet build BookTracker.sln
dotnet test BookTracker.sln
dotnet run --project BookTracker.Api   # http://localhost:5255  (OpenAPI at /openapi)
```

To use the GitHub MCP server from Claude Code, export a personal access token first:

```bash
export GITHUB_PAT="ghp_your_token_here"
```

## Project layout

```text
BookTracker.sln
CLAUDE.md
.claude/                      # steering kit from c2
.mcp.json                     # ← NEW: GitHub MCP server declaration
proposed-issues/              # ← NEW: the lab's output (+ offline fallback)
│   ├── reading-progress.md
│   └── recent-history.txt
BookTracker.Core/  BookTracker.Data/  BookTracker.Api/  BookTracker.Tests/
```

## API endpoints

Unchanged from `c2` — **Books**, **Authors**, and **Reviews**
(`/api/books/{id}/reviews`). See [`c0`](../../c0/BookTracker/README.md#api-endpoints) and
[`c2`](../../c2/BookTracker/README.md#api-endpoints) for the tables.

## Try it

With the GitHub MCP server connected, ask Claude Code to *"propose a Reading Progress feature as a
GitHub issue"* and watch it use the MCP tools. Offline? It falls back to writing
`proposed-issues/reading-progress.md`, which is exactly what you'll build in `c4`.

## Teaching gaps

The starter's intentional flaws still exist in the pre-existing endpoints. Reading Progress is still
absent — it is the next checkpoint.

## Workshop resources

- **Guide:** [dotnetclaude.com](https://dotnetclaude.com) — Day 1, Section 3 (IDE Integration & MCP).
- **Deck:** `decks/Day 1/Section3-IDE-Integration-MCP.pptx`
- **Docs:** [MCP in Claude Code](https://docs.claude.com/en/docs/claude-code/mcp) ·
  [Model Context Protocol](https://modelcontextprotocol.io) ·
  [GitHub MCP server](https://github.com/github/github-mcp-server)
- **Previous:** [`c2`](../../c2/BookTracker/README.md) — the `.claude/` steering kit.
- **Next:** [`c4`](../../c4/BookTracker/README.md) — build Reading Progress from a written spec.
