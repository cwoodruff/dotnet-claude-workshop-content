# .NET AI with Claude — Workshop Content

Welcome! This repository contains all the hands-on content for the two-day **.NET AI with Claude** workshop. It is the companion to the workshop guide at **[dotnetclaude.com](https://dotnetclaude.com)** — the site walks you through each module and lab, and this repo gives you the slide decks and the working code checkpoints you'll build against.

> 🔑 Workshop attendees receive access credentials for dotnetclaude.com as part of the workshop. If you're attending and don't have them yet, ask your instructor.

## About the workshop

**.NET AI with Claude** is a two-day, hands-on workshop for .NET developers who want to build real AI capabilities into their applications — and use AI to build them faster.

- **Day 1 — Claude Code for .NET developers.** Learn to work effectively with Claude Code as your pair programmer: project memory with `CLAUDE.md`, rules, custom agents, hooks, skills, plugins, MCP servers, and spec-driven feature development. You'll use Claude Code to find and fix intentionally planted issues in a real codebase, then build complete features from written specs.
- **Day 2 — Building AI features with the Anthropic API.** Integrate Claude directly into a .NET application using the Anthropic C# SDK: chat endpoints, streaming, tool use and agent loops, retrieval-augmented generation (RAG) with a vector store, building your own MCP server in C#, testing AI systems, and responsible AI practices (prompt-injection defenses, auditing, token budgets).

Everything is taught against **BookTracker**, a realistic ASP.NET Core Minimal API application for tracking books, authors, reviews, and reading progress. You don't just watch — every module ends in a lab where you extend the app yourself.

## What's in this repository

```
decks/   Slide decks for Day 1 and Day 2 (PowerPoint)
src/     Eleven frozen checkpoints of the BookTracker app: c0 … c10
```

### Checkpoints: c0 through c10

Each directory under `src/` (`c0`–`c10`) is a **complete, independently buildable snapshot** of the BookTracker solution, representing the expected state *after* that lab. If you fall behind or want to skip ahead, just open the next checkpoint — no git branch juggling required.

| Checkpoint | Day | What it adds |
|---|---|---|
| `c0` | 1 | Starter app: Books + Authors API — with intentional teaching gaps you'll discover in the labs |
| `c1` | 1 | Project memory: a `CLAUDE.md` for the app |
| `c2` | 1 | `.claude/` customization (rules, agents, hooks, skills, plugin) + Reviews feature |
| `c3` | 1 | MCP integration (`.mcp.json` with GitHub MCP) + proposed issues |
| `c4` | 1 | Reading Progress feature built from a written spec (`specs/reading-progress.md`) |
| `c5` | 2 | Anthropic C# SDK, `/api/chat` endpoint, `ClaudeService` |
| `c6` | 2 | Streaming responses, tool use, and an agent loop (`AgentService`) |
| `c7` | 2 | RAG: vector store project, `/api/recommend`, Qdrant via Docker Compose |
| `c8` | 2 | `BookTracker.Mcp` — an MCP server written in C# |
| `c9` | 2 | Testing AI systems: LLM abstraction, agent tests, integration tests with Testcontainers |
| `c10` | 2 | Responsible AI: prompt-injection defenses, AI audit logging, token-budget middleware, security tests |

⚠️ **Heads up:** the early checkpoints (`c0`–`c3`) contain *deliberate* problems — validation holes, a SQL-injection vector, endpoints returning EF entities. Finding and fixing them is part of the labs, so resist the urge to "clean them up" before the workshop. The fully hardened app is `c10`.

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Claude Code](https://claude.com/claude-code) (Day 1) and an Anthropic API key (Day 2)
- [Docker](https://www.docker.com/) — for Qdrant (c7+) and Testcontainers-based tests (c9+)
- [Ollama](https://ollama.com/) with `ollama pull nomic-embed-text` — local embeddings for RAG (c7+); optional, since the default vector store provider is `InMemory`

### Running a checkpoint

All commands run from inside a checkpoint, e.g.:

```bash
cd src/c5/BookTracker

dotnet build BookTracker.sln
dotnet test BookTracker.sln
dotnet run --project BookTracker.Api      # http://localhost:5255
```

The SQLite database is created, migrated, and seeded automatically at startup.

For checkpoints `c5` and later, set your Anthropic API key with user secrets (never commit it):

```bash
dotnet user-secrets set "Anthropic:ApiKey" "your-key-here" --project BookTracker.Api
```

For `c7` and later, start Qdrant when you want real vector search:

```bash
docker compose up -d
```

## About your instructor

**Chris "Woody" Woodruff** is a veteran software developer and architect with decades of experience building on the Microsoft stack, and a long-time international conference speaker on .NET, APIs, and software craftsmanship. He is a co-host of [The Breakpoint Show](https://breakpoint.show) podcast and writes about .NET and developer topics at [woodruff.dev](https://woodruff.dev). Woody created this workshop to give .NET teams a practical, code-first path into building with Claude — both as a development partner and as a capability inside their own applications.

- 🌐 Blog: [woodruff.dev](https://woodruff.dev)
- 💼 LinkedIn: [linkedin.com/in/chriswoodruff](https://www.linkedin.com/in/chriswoodruff/)
- 🎙️ Podcast: [The Breakpoint Show](https://breakpoint.show)

## Bring this workshop to your team

Want **.NET AI with Claude** delivered at your company, user group, or conference? Woody offers this workshop as a private engagement — on-site or remote — and can tailor the depth and pacing to your team's experience level.

📧 Contact him at **christopherlwoodruff@gmail.com** or reach out on [LinkedIn](https://www.linkedin.com/in/chriswoodruff/) to discuss scheduling, group size, and pricing.

## License

See [LICENSE](LICENSE) for details. Workshop content and the BookTracker application are provided for workshop attendees' learning and reference.
