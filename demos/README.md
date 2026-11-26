# Workshop Demos

Instructor demo kits for the *.NET AI with Claude Workshop*. Each folder contains a `README.md`
with the full run-of-show (setup, exact prompts/commands, expected output, recovery, callouts).
Demos that need staged files ship an `artifacts/` folder; Day 2 demos that need code ship a
self-contained .NET solution in `src/`.

Each demo is tracked as a GitHub issue under its day's milestone:
[Day 1 Demos](https://github.com/cwoodruff/dotnet-claude-workshop/milestone/1) ·
[Day 2 Demos](https://github.com/cwoodruff/dotnet-claude-workshop/milestone/2)

## Day 1 — Claude Code Track

| Folder | Demo | Deck (decks/Day 1/) | Issue |
|---|---|---|---|
| `01-installation-walkthrough` | Demo 1: Installation Walk-Through | Section1-Foundations-Claude-Code-CLI.pptx | [#3](https://github.com/cwoodruff/dotnet-claude-workshop/issues/3) |
| `02-claude-md-creation` | Demo 2: CLAUDE.md Creation | Section1-Foundations-Claude-Code-CLI.pptx | [#4](https://github.com/cwoodruff/dotnet-claude-workshop/issues/4) |
| `03-live-codebase-exploration` | Demo 3: Live Codebase Exploration | Section1-Foundations-Claude-Code-CLI.pptx | [#5](https://github.com/cwoodruff/dotnet-claude-workshop/issues/5) |
| `04-steering-decision` | Demo D-STEER: The Steering Decision | Section2-Steering-Claude-Code.pptx | [#6](https://github.com/cwoodruff/dotnet-claude-workshop/issues/6) |
| `05-skill-folder-trigger` | Demo D4: Skill Folder + `/name` trigger | Section2-Steering-Claude-Code.pptx | [#7](https://github.com/cwoodruff/dotnet-claude-workshop/issues/7) |
| `06-rules-path-scoped` | Demo D4b: Rules — path-scoped in C# | Section2-Steering-Claude-Code.pptx | [#8](https://github.com/cwoodruff/dotnet-claude-workshop/issues/8) |
| `07-subagent-async` | Demo D5: Subagent (markdown) + Async | Section2-Steering-Claude-Code.pptx | [#9](https://github.com/cwoodruff/dotnet-claude-workshop/issues/9) |
| `08-guardrail-hook` | Demo D6: Guardrail Hook (PreToolUse exit 2) | Section2-Steering-Claude-Code.pptx | [#10](https://github.com/cwoodruff/dotnet-claude-workshop/issues/10) |
| `09-package-as-plugin` | Demo D6b: Package as Plugin | Section2-Steering-Claude-Code.pptx | [#11](https://github.com/cwoodruff/dotnet-claude-workshop/issues/11) |
| `10-github-mcp-server` | Demo 8: GitHub MCP Server | Section3-IDE-Integration-MCP.pptx | [#12](https://github.com/cwoodruff/dotnet-claude-workshop/issues/12) |
| `11-domain-modeling` | Demo 9: Domain Modeling | Section4-AI-in-the-SDLC.pptx | [#13](https://github.com/cwoodruff/dotnet-claude-workshop/issues/13) |
| `12-debugging-stack-trace` | Demo 9b: Debugging from a Stack Trace | Section4-AI-in-the-SDLC.pptx | [#14](https://github.com/cwoodruff/dotnet-claude-workshop/issues/14) |
| `13-full-feature-from-spec` | Demo 10: Full Feature from Spec | Section4-AI-in-the-SDLC.pptx | [#15](https://github.com/cwoodruff/dotnet-claude-workshop/issues/15) |

Day 1 demos run Claude Code against the BookTracker lab checkpoints in the public
[workshop content repo](https://github.com/cwoodruff/dotnet-claude-workshop-content) (`src/c0` to
`src/c4`), no separate demo code. Each demo's setup names its checkpoint: Section 1 runs in `c0`,
Section 2 in `c1`, the MCP demo in `c2`, domain modeling and the full feature build in `c3` (with the
spec staged from `c4`), and the debugging demo in `c4`. Reset a checkpoint afterward with
`git restore . && git clean -fd` from its folder. The `artifacts/` folders hold the `.claude/` files (skill, rules, subagent, hook,
settings, plugin manifest) each demo stages, plus the offline fallback, ADR template, and the
staged bug + failing output for the debugging demo.

## Day 2 — Anthropic C# SDK Track

| Folder | Demo | Deck (decks/Day 2/) | Issue | Code |
|---|---|---|---|---|
| `01-first-api-call` | Demo 1: First working API call | Day2-Section1-CSharp-SDK-Fundamentals.pptx | [#16](https://github.com/cwoodruff/dotnet-claude-workshop/issues/16) | `src/` console app |
| `02-multi-turn-prompt-caching` | Demo 2: Multi-turn + prompt-cache token counts | Day2-Section1-CSharp-SDK-Fundamentals.pptx | [#17](https://github.com/cwoodruff/dotnet-claude-workshop/issues/17) | `src/` console app |
| `03-sse-streaming` | Demo 3: Live SSE streaming endpoint | Day2-Section2-Streaming-Tools-Thinking.pptx | [#18](https://github.com/cwoodruff/dotnet-claude-workshop/issues/18) | `src/` Minimal API (port 5055) |
| `04-tool-calling-agent-loop` | Demo 4: Tool-calling agent loop (2+ tools) | Day2-Section2-Streaming-Tools-Thinking.pptx | [#19](https://github.com/cwoodruff/dotnet-claude-workshop/issues/19) | `src/` console app |
| `05-rag-grounded-vs-ungrounded` | Demo 5: RAG — grounded vs ungrounded | Day2-Section3-RAG-Pipelines.pptx | [#20](https://github.com/cwoodruff/dotnet-claude-workshop/issues/20) | `src/` Minimal API + Qdrant/in-memory |
| `06-csharp-mcp-server` | Demo 6: C# MCP server → Claude Code | Day2-Section4-MCP-Servers-CSharp.pptx | [#21](https://github.com/cwoodruff/dotnet-claude-workshop/issues/21) | `src/` MCP server (port 5100) |
| `07-generate-xunit-suite` | Demo 7: Generate xUnit suite for AgentService | Day2-Section5-AI-Testing-CICD.pptx | [#22](https://github.com/cwoodruff/dotnet-claude-workshop/issues/22) | runs against `src/BookTracker` |
| `08-github-actions-ai-review` | Demo 8: GitHub Actions AI review on a PR | Day2-Section5-AI-Testing-CICD.pptx | [#23](https://github.com/cwoodruff/dotnet-claude-workshop/issues/23) | `artifacts/ai-code-review.yml` |
| `09-prompt-injection` | Demo 9: Prompt injection (make it visceral) | Day2-Section6-Responsible-AI-WrapUp.pptx | [#24](https://github.com/cwoodruff/dotnet-claude-workshop/issues/24) | `src/` Minimal API (port 5099) |

All Day 2 projects target `net10.0`, use the same `Anthropic` 13.14.0 NuGet package as
BookTracker, read the API key from user-secrets (`Anthropic:ApiKey`) with an
`ANTHROPIC_API_KEY` env-var fallback, and build with zero warnings. None contain secrets.
