# BookTracker — Checkpoint c6 (Streaming, tools & agent loop)

The sample application for the **.NET AI with Claude Workshop**. An ASP.NET Core 10 Minimal API
for tracking books, backed by EF Core 10 and SQLite.

> **c6 — streaming, tool use, and an agent loop.** *Day 2 · Section 2 — Streaming, Tools & Thinking.*
> This is `c5` plus three of the most important patterns for building with Claude: **streaming**
> responses token-by-token, giving Claude **tools** it can call, and running an **agent loop** that
> lets Claude use those tools to get real work done against BookTracker's own data.

## What's new in this checkpoint

- **`StreamingService`** (`IStreamingService`) — streams Claude's response as it's generated, exposed
  over Server-Sent Events at `GET /api/chat/stream`.
- **`BookTrackerTools`** (`BookTracker.Api/Tools/`) — three tool definitions Claude can call, backed
  by the app's own services:
  - **`find_book`** — search the catalog by title.
  - **`get_reading_progress`** — read a book's current page / status / percent complete.
  - **`update_reading_progress`** — advance a book's reading progress.
- **`AgentService`** (`IAgentService`) — the agent loop: send the message + tool definitions to
  Claude, execute any tool calls it returns, feed the results back, and repeat until Claude produces a
  final answer. Exposed at `POST /api/agent`.
- **`AgentDtos`** — request/result shapes for the agent endpoint.

This is the checkpoint where Claude stops just *talking about* books and starts *acting on* them via
the Reading Progress feature you built in `c4`.

## Prerequisites

- **Anthropic API key** (as in `c5`):

  ```bash
  cd src/c6/BookTracker
  dotnet user-secrets set "Anthropic:ApiKey" "your-key-here" --project BookTracker.Api
  ```

## Getting started

```bash
cd src/c6/BookTracker

dotnet build BookTracker.sln
dotnet test BookTracker.sln
dotnet run --project BookTracker.Api   # http://localhost:5255  (OpenAPI at /openapi)
```

## Project layout

```text
BookTracker.Core/Services/
├── IStreamingService.cs                # ← NEW
└── IAgentService.cs                    # ← NEW
BookTracker.Core/Dtos/AgentDtos.cs      # ← NEW
BookTracker.Api/
├── Tools/BookTrackerTools.cs           # ← NEW: find_book, get/update_reading_progress
├── Services/StreamingService.cs        # ← NEW: SSE streaming
├── Services/AgentService.cs            # ← NEW: the tool-use agent loop
└── Endpoints/AgentEndpoints.cs         # ← NEW: GET /api/chat/stream + POST /api/agent
```

## API endpoints

Everything from `c5` still works. New in `c6`:

| Method | Route                 | Description                                                        |
|--------|-----------------------|-------------------------------------------------------------------|
| GET    | `/api/chat/stream`    | Stream Claude's reply as Server-Sent Events                       |
| POST   | `/api/agent`          | Run the agent loop: Claude calls BookTracker tools, returns a result |

## Try it

```bash
# Streaming — watch tokens arrive incrementally
curl -N "http://localhost:5255/api/chat/stream?message=Tell%20me%20about%20Clean%20Code"

# Agent loop — Claude uses tools to read/update real data
curl -X POST http://localhost:5255/api/agent \
  -H "Content-Type: application/json" \
  -d '{"message":"Mark Clean Code as finished."}'
```

## Workshop resources

- **Guide:** [dotnetclaude.com](https://dotnetclaude.com) — Day 2, Section 2 (Streaming, Tools & Thinking).
- **Deck:** `decks/Day 2/Day2-Section2-Streaming-Tools-Thinking.pptx`
- **Docs:** [Streaming](https://docs.claude.com/en/api/streaming) ·
  [Tool use](https://docs.claude.com/en/docs/build-with-claude/tool-use) ·
  [Building agents](https://docs.claude.com/en/docs/agents-and-tools)
- **Previous:** [`c5`](../../c5/BookTracker/README.md) — the Anthropic C# SDK and `/api/chat`.
- **Next:** [`c7`](../../c7/BookTracker/README.md) — RAG with a vector store and `/api/recommend`.
