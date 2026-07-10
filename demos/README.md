# Workshop Demos

Instructor demo kits for the *.NET AI with Claude Workshop*. Each folder contains a `README.md`
with the full run-of-show (setup, exact prompts/commands, expected output, recovery, callouts).
Demos that need staged files ship an `artifacts/` folder; Day 2 demos that need code ship a
self-contained .NET solution in `src/`.

## Day 2 — Anthropic C# SDK Track

| Folder | Demo | Deck (decks/Day 2/) | Issue | Code |
|---|---|---|---|---|
| `01-first-api-call` | Demo 1: First working API call | Day2-Section1-CSharp-SDK-Fundamentals.pptx |  | `src/` console app |
| `02-multi-turn-prompt-caching` | Demo 2: Multi-turn + prompt-cache token counts | Day2-Section1-CSharp-SDK-Fundamentals.pptx |  | `src/` console app |
| `03-sse-streaming` | Demo 3: Live SSE streaming endpoint | Day2-Section2-Streaming-Tools-Thinking.pptx |  | `src/` Minimal API (port 5055) |
| `04-tool-calling-agent-loop` | Demo 4: Tool-calling agent loop (2+ tools) | Day2-Section2-Streaming-Tools-Thinking.pptx |  | `src/` console app |
| `05-rag-grounded-vs-ungrounded` | Demo 5: RAG — grounded vs ungrounded | Day2-Section3-RAG-Pipelines.pptx |  | `src/` Minimal API + Qdrant/in-memory |
| `06-csharp-mcp-server` | Demo 6: C# MCP server → Claude Code | Day2-Section4-MCP-Servers-CSharp.pptx |  | `src/` MCP server (port 5100) |
| `07-generate-xunit-suite` | Demo 7: Generate xUnit suite for AgentService | Day2-Section5-AI-Testing-CICD.pptx |  | runs against `src/BookTracker` |
| `08-github-actions-ai-review` | Demo 8: GitHub Actions AI review on a PR | Day2-Section5-AI-Testing-CICD.pptx |  | `artifacts/ai-code-review.yml` |
| `09-prompt-injection` | Demo 9: Prompt injection (make it visceral) | Day2-Section6-Responsible-AI-WrapUp.pptx |  | `src/` Minimal API (port 5099) |

All Day 2 projects target `net10.0`, use the same `Anthropic` 12.31.0 NuGet package as
BookTracker, read the API key from user-secrets (`Anthropic:ApiKey`) with an
`ANTHROPIC_API_KEY` env-var fallback, and build with zero warnings. None contain secrets.
