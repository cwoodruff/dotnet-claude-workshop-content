# BookTracker — Checkpoint c10 (Responsible AI)

The sample application for the **.NET AI with Claude Workshop**. An ASP.NET Core 10 Minimal API
for tracking books, backed by EF Core 10 and SQLite.

> **c10 — responsible AI & the finished app.** *Day 2 · Section 6 — Responsible AI & Wrap-Up.* This
> is `c9` hardened for production: **prompt-injection defenses**, an **AI audit log**, a
> **token-budget middleware**, and security tests. This is the **final checkpoint** — the fully
> hardened BookTracker, with all the earlier intentional teaching gaps closed.

## What's new in this checkpoint

### Prompt safety & cost controls (`BookTracker.Api/Security/`)

- **`PromptSafety.cs`** — prompt-injection defenses: screens/wraps untrusted input before it reaches
  the model.
- **`TokenBudget.cs`** + **`AiCost.cs`** — model token accounting and cost estimation.
- **`Middleware/TokenBudgetMiddleware.cs`** — enforces a per-request (or per-window) token budget so a
  runaway prompt can't rack up unbounded cost.

### AI audit logging

- **`AiAuditLog`** entity (`BookTracker.Core/Entities/`) + **`IAiAuditService`** /
  **`AiAuditService`** — persist a record of every AI interaction (who asked what, tokens, outcome)
  for review and accountability.
- EF Core migration **`AddAiAuditLog`**.

### Security tests (`BookTracker.Tests/Security/`)

- **`PromptSafetyTests.cs`** — asserts injection attempts are neutralized.
- **`AiCostTests.cs`** — asserts token/cost accounting behaves.

## Prerequisites

- **.NET 10 SDK** · **Anthropic API key**
  (`dotnet user-secrets set "Anthropic:ApiKey" "your-key-here" --project BookTracker.Api`)
- **Docker / Ollama** only for the RAG path and the `c9` container tests.

## Getting started

```bash
cd src/c10/BookTracker

dotnet build BookTracker.sln
dotnet test BookTracker.sln
dotnet run --project BookTracker.Api   # http://localhost:5255  (OpenAPI at /openapi)
```

## Project layout

```text
BookTracker.Core/
├── Entities/AiAuditLog.cs              # ← NEW
└── Services/IAiAuditService.cs         # ← NEW
BookTracker.Data/                       # + AddAiAuditLog migration
BookTracker.Api/
├── Security/PromptSafety.cs            # ← NEW: injection defenses
├── Security/TokenBudget.cs             # ← NEW
├── Security/AiCost.cs                  # ← NEW
├── Middleware/TokenBudgetMiddleware.cs # ← NEW: enforce token budgets
└── Services/AiAuditService.cs          # ← NEW
BookTracker.Tests/Security/
├── PromptSafetyTests.cs                # ← NEW
└── AiCostTests.cs                      # ← NEW
```

## API endpoints

Same surface as `c9` (Books, Authors, Reviews, Reading Progress, Chat, Agent, Recommend) — but the AI
endpoints are now wrapped in the safety, budget, and audit layers. Requests that exceed the token
budget are rejected by the middleware; every AI call is written to the audit log.

## The finished app

`c10` is the destination. Compared to the `c0` starter, the intentional flaws are fixed, the app ships
real AI features (chat, streaming, tools, an agent, RAG, an MCP server), those features are tested,
and they run under responsible-AI guardrails. If you want to see how far the codebase travels across
the workshop, diff `c0` against `c10`.

## Workshop resources

- **Guide:** [dotnetclaude.com](https://dotnetclaude.com) — Day 2, Section 6 (Responsible AI & Wrap-Up).
- **Deck:** `decks/Day 2/Day2-Section6-Responsible-AI-WrapUp.pptx`
- **Reference cards:** `docs/models-cost-reference-card.pdf` · `docs/day2-faq-reference-card.pdf`
- **Docs:** [Prompt injection & safety](https://docs.claude.com/en/docs/test-and-evaluate/strengthen-guardrails) ·
  [Usage & cost](https://docs.claude.com/en/api/rate-limits) ·
  [Responsible use](https://docs.claude.com/en/docs/about-claude/use-case-guides)
- **Previous:** [`c9`](../../c9/BookTracker/README.md) — testing AI systems.
- **Start over:** [`c0`](../../c0/BookTracker/README.md) — the raw starter this all began from.
