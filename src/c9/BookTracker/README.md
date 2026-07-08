# BookTracker — Checkpoint c9 (Testing AI systems)

The sample application for the **.NET AI with Claude Workshop**. An ASP.NET Core 10 Minimal API
for tracking books, backed by EF Core 10 and SQLite.

> **c9 — testing AI systems.** *Day 2 · Section 5 — AI Testing & CI/CD.* This is `c8` with the AI
> features made **testable**: an LLM abstraction that can be substituted in tests, a test harness and
> unit tests for the agent loop, and **Testcontainers**-based integration tests for the RAG and
> SQL paths. Non-determinism is the challenge; this checkpoint shows how to test around it.

## What's new in this checkpoint

### An LLM seam for testing

- **`IAgentLlm`** (`BookTracker.Core/Services/`) — an abstraction over the model call used by the
  agent loop.
- **`AnthropicAgentLlm`** (`BookTracker.Api/Services/`) — the real, SDK-backed implementation.

Because the agent depends on `IAgentLlm`, tests can substitute a fake (via `NSubstitute`) and drive
the loop deterministically — asserting *how tools are called* rather than gambling on model output.

### Agent tests (`BookTracker.Tests/Agent/`)

- **`AgentTestHarness.cs`** — scaffolding to script LLM responses and observe tool calls.
- **`AgentServiceTests.cs`** — the happy paths of the tool-use loop.
- **`AgentEdgeCaseTests.cs`** — malformed tool calls, loops, empty results, and other edge cases.

### Integration tests (`BookTracker.Tests/Integration/`)

- **`DockerFactAttribute.cs`** — a custom xUnit trait that skips Docker-dependent tests when Docker
  isn't available, so the suite stays green locally and in CI.
- **`QdrantRagTests.cs`** — spins up **Qdrant** with `Testcontainers.Qdrant` and exercises the real
  RAG retrieval path from `c7`.
- **`SqlServerTests.cs`** — spins up **SQL Server** with `Testcontainers.MsSql` to test against a real
  relational engine, not just SQLite.

New test dependencies: `NSubstitute`, `Testcontainers.Qdrant`, `Testcontainers.MsSql`,
`Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.SqlServer`.

## Prerequisites

- **.NET 10 SDK** · **Anthropic API key** (for running the app; agent unit tests don't need it).
- **Docker** — required for the `[DockerFact]` integration tests (Qdrant / SQL Server containers).
  Without Docker those tests are **skipped**, not failed.

## Getting started

```bash
cd src/c9/BookTracker

dotnet build BookTracker.sln
dotnet test BookTracker.sln            # unit tests always run; container tests run if Docker is up
```

## Project layout

```text
BookTracker.Core/Services/IAgentLlm.cs          # ← NEW: the LLM seam
BookTracker.Api/Services/AnthropicAgentLlm.cs   # ← NEW: real implementation
BookTracker.Tests/
├── Agent/
│   ├── AgentTestHarness.cs                     # ← NEW
│   ├── AgentServiceTests.cs                     # ← NEW
│   └── AgentEdgeCaseTests.cs                    # ← NEW
└── Integration/
    ├── DockerFactAttribute.cs                   # ← NEW: skip-if-no-Docker trait
    ├── QdrantRagTests.cs                         # ← NEW: Testcontainers Qdrant
    └── SqlServerTests.cs                          # ← NEW: Testcontainers MsSql
```

## API endpoints

Unchanged from `c8` — this checkpoint adds *tests and a testable seam*, not new endpoints.

## Try it

```bash
# Run just the agent unit tests (no Docker needed)
dotnet test --filter "FullyQualifiedName~Agent"

# Run the container-backed integration tests (Docker must be running)
dotnet test --filter "FullyQualifiedName~Integration"
```

## Workshop resources

- **Guide:** [dotnetclaude.com](https://dotnetclaude.com) — Day 2, Section 5 (AI Testing & CI/CD).
- **Deck:** `decks/Day 2/Day2-Section5-AI-Testing-CICD.pptx`
- **Docs:** [Testcontainers for .NET](https://dotnet.testcontainers.org/) ·
  [xUnit](https://xunit.net/) · [NSubstitute](https://nsubstitute.github.io/)
- **Previous:** [`c8`](../../c8/BookTracker/README.md) — the C# MCP server.
- **Next:** [`c10`](../../c10/BookTracker/README.md) — responsible AI: safety, auditing, token budgets.
