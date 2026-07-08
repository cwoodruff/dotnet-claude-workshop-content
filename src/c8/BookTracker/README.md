# BookTracker — Checkpoint c8 (An MCP server in C#)

The sample application for the **.NET AI with Claude Workshop**. An ASP.NET Core 10 Minimal API
for tracking books, backed by EF Core 10 and SQLite.

> **c8 — your own MCP server.** *Day 2 · Section 4 — MCP Servers in C#.* This is `c7` plus a brand-new
> **`BookTracker.Mcp`** project: a Model Context Protocol server, written in C#, that exposes
> BookTracker's data as MCP tools any MCP client (Claude Code, Claude Desktop, …) can call. In `c3`
> you *consumed* an MCP server; here you *build* one.

## What's new in this checkpoint

### `BookTracker.Mcp` project (new)

An ASP.NET Core (`Microsoft.NET.Sdk.Web`) MCP server built on the `ModelContextProtocol` and
`ModelContextProtocol.AspNetCore` packages, referencing `BookTracker.Core` and `BookTracker.Data` so
it works against the real database.

```text
BookTracker.Mcp/
├── Program.cs                          # host + MCP server wiring
├── Tools/BookMcpTools.cs               # [McpServerTool] methods exposed to MCP clients
├── Security/
│   ├── McpAuth.cs                      # auth for the server
│   └── AuditLogger.cs                  # logs tool invocations
├── appsettings.json / .Development.json
└── Properties/launchSettings.json
```

**Tools exposed** (`[McpServerTool]` methods in `BookMcpTools.cs`):

- **Search books** — find books whose title matches a query (returns JSON).
- **Get reading progress** — a book's page / status / percent complete by id.
- **Add book** — create a new book (the author must already exist).

## Prerequisites

- **Anthropic API key** for the API's AI features (as in earlier Day 2 checkpoints).
- **Docker / Ollama** only if you exercise the `c7` RAG path.

## Getting started

```bash
cd src/c8/BookTracker

dotnet build BookTracker.sln
dotnet test BookTracker.sln

# run the API as before
dotnet run --project BookTracker.Api      # http://localhost:5255

# run the MCP server
dotnet run --project BookTracker.Mcp
```

Point an MCP client (e.g. Claude Code via `.mcp.json`, or Claude Desktop) at the running
`BookTracker.Mcp` server to call its tools.

## Project layout

```text
BookTracker.sln
BookTracker.Mcp/                        # ← NEW: the C# MCP server
BookTracker.Core/  BookTracker.Data/  BookTracker.Api/  BookTracker.VectorStore/  BookTracker.Tests/
docker-compose.yml
```

## API endpoints

The HTTP API is unchanged from `c7`. The new surface here is **MCP tools**, served by
`BookTracker.Mcp` — not REST endpoints. Explore `Tools/BookMcpTools.cs` to see the tool contracts.

## Try it

Run `BookTracker.Mcp`, connect Claude Code or Claude Desktop to it, and ask something like
*"Search BookTracker for books about refactoring"* or *"What's my reading progress on book 1?"* —
Claude will call your C# tools.

## Workshop resources

- **Guide:** [dotnetclaude.com](https://dotnetclaude.com) — Day 2, Section 4 (MCP Servers in C#).
- **Deck:** `decks/Day 2/Day2-Section4-MCP-Servers-CSharp.pptx`
- **Docs:** [Model Context Protocol](https://modelcontextprotocol.io) ·
  [C# MCP SDK](https://github.com/modelcontextprotocol/csharp-sdk) ·
  [MCP in Claude Code](https://docs.claude.com/en/docs/claude-code/mcp)
- **Previous:** [`c7`](../../c7/BookTracker/README.md) — the RAG pipeline.
- **Next:** [`c9`](../../c9/BookTracker/README.md) — testing AI systems.
