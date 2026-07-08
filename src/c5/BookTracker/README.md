# BookTracker — Checkpoint c5 (Anthropic C# SDK)

The sample application for the **.NET AI with Claude Workshop**. An ASP.NET Core 10 Minimal API
for tracking books, backed by EF Core 10 and SQLite.

> **c5 — Claude inside the app.** *Day 2 · Section 1 — C# SDK Fundamentals.* This is `c4` (the
> finished Day 1 app) with its first **AI feature**: a `/api/chat` endpoint that calls Claude through
> the official **Anthropic C# SDK**. Day 2 shifts from *using Claude to build* to *building with
> Claude inside your own application*.

## What's new in this checkpoint

- **Anthropic SDK dependency** — `BookTracker.Api` now references the `Anthropic` NuGet package.
- **`ClaudeService`** (`BookTracker.Api/Services/ClaudeService.cs`) implementing
  **`IClaudeService`** (`BookTracker.Core`) — the wrapper that sends messages to Claude and returns
  the reply. This is where model, system prompt, and max-tokens are configured.
- **`AnthropicOptions`** (`BookTracker.Api/Options/`) — strongly-typed config bound from
  `Anthropic:*` settings (API key, model).
- **`ChatDtos`** (`BookTracker.Core/Dtos/`) — the request/response shapes for the chat endpoint.
- **`ChatEndpoints`** — a `POST /api/chat` endpoint wired into the Minimal API.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- An **Anthropic API key** — set it with user secrets (never commit it):

  ```bash
  cd src/c5/BookTracker
  dotnet user-secrets set "Anthropic:ApiKey" "your-key-here" --project BookTracker.Api
  ```

## Getting started

```bash
cd src/c5/BookTracker

dotnet build BookTracker.sln
dotnet test BookTracker.sln
dotnet run --project BookTracker.Api   # http://localhost:5255  (OpenAPI at /openapi)
```

## Project layout

```text
BookTracker.Core/
└── Services/IClaudeService.cs          # ← NEW: the chat abstraction
└── Dtos/ChatDtos.cs                    # ← NEW: chat request/response
BookTracker.Api/
├── Options/AnthropicOptions.cs         # ← NEW: bound Anthropic:* config
├── Services/ClaudeService.cs           # ← NEW: SDK wrapper (model, system prompt, max tokens)
└── Endpoints/ChatEndpoints.cs          # ← NEW: POST /api/chat
```

## API endpoints

All Day 1 endpoints (**Books**, **Authors**, **Reviews**, **Reading Progress**) still work. New:

**Chat** — under `/api/chat`:

| Method | Route         | Description                                             |
|--------|---------------|---------------------------------------------------------|
| POST   | `/api/chat`   | Send a message to Claude, get a `ChatResponse` back     |

## Try it

```bash
curl -X POST http://localhost:5255/api/chat \
  -H "Content-Type: application/json" \
  -d '{"message":"Recommend a book about software craftsmanship."}'
```

If you get a 401/authentication error, your `Anthropic:ApiKey` user secret isn't set — see
[Prerequisites](#prerequisites).

## Workshop resources

- **Guide:** [dotnetclaude.com](https://dotnetclaude.com) — Day 2, Section 1 (C# SDK Fundamentals).
- **Deck:** `decks/Day 2/Day2-Section1-CSharp-SDK-Fundamentals.pptx`
- **Reference cards:** `docs/models-cost-reference-card.pdf` · `docs/day2-faq-reference-card.pdf`
- **Docs:** [Anthropic API](https://docs.claude.com/en/api) ·
  [Anthropic C# SDK](https://github.com/anthropics/anthropic-sdk-dotnet) ·
  [Models overview](https://docs.claude.com/en/docs/about-claude/models)
- **Previous:** [`c4`](../../c4/BookTracker/README.md) — the finished Day 1 app.
- **Next:** [`c6`](../../c6/BookTracker/README.md) — streaming, tool use, and an agent loop.
