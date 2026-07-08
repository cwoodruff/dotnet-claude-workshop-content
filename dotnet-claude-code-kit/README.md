# .NET Claude Code Kit

A complete `.claude/` productivity kit for C# / ASP.NET Core solutions: a CLAUDE.md template, path-scoped rules, skills, hooks, subagents, and slash commands that work together out of the box.

Verified against Claude Code documentation as of July 2026.

## Install

1. Copy `CLAUDE.md` and the `.claude/` directory into your solution root (the folder containing your `.sln`).
2. Replace the `[bracketed placeholders]` in `CLAUDE.md` with your solution's names.
3. Make hooks executable (macOS/Linux/WSL/Git Bash): `chmod +x .claude/hooks/*.sh`
4. Start Claude Code and accept the workspace trust dialog — project hooks, permission rules, and skill `allowed-tools` only activate after trust is granted.
5. Commit everything except `CLAUDE.local.md` and `.claude/settings.local.json` so your team shares the setup.

**Prerequisites:** .NET SDK on PATH, `jq` (macOS: `brew install jq`, Ubuntu: `apt install jq`), and on Windows run Claude Code from Git Bash (already a Claude Code requirement) so the shell hooks work.

## What's inside

```
CLAUDE.md                      Solution-level instructions (loaded every session)
.claude/
  settings.json                Hooks + permission allow/deny rules (team-shared)
  rules/                       Modular instructions; path-scoped ones load on demand
    csharp-style.md              **/*.cs
    aspnet-api.md                Api project, Endpoints, Program.cs
    ef-core.md                   Data project + Migrations
    testing.md                   Test projects
    security.md                  always loaded
    solution-structure.md        always loaded
  skills/                      Procedures Claude applies automatically or via /name
    ef-migration/                safe EF Core migration workflow
    add-endpoint/                convention-following Minimal API endpoint, end to end
    gen-tests/                   xUnit test generation procedure
    nuget-audit/                 vulnerable/deprecated/outdated package audit
    async-hygiene/               async/await anti-pattern sweep with fixes
    dotnet-upgrade/              phased TFM upgrade (assess → execute → modernize)
    summarize-changes/           diff summary + risk flags (uses dynamic context injection)
  agents/                      Subagents with isolated context
    code-reviewer.md             senior review of recent changes (inherits model)
    test-writer.md               mechanical test generation (Haiku — ~cheap)
    security-auditor.md          read-only audit with persistent project memory
    perf-analyzer.md             EF/async/allocation performance analysis
    docs-writer.md               XML docs, endpoint docs, ADRs (Haiku)
    migration-planner.md         phased modernization plans with effort estimates
  commands/                    Explicit-invocation prompts (/name)
    security-review.md  commit.md  pr-description.md  api-docs.md
    coverage-gaps.md    changelog.md  perf-review.md   fix-issue.md
  hooks/                       Scripts wired up in settings.json
    dotnet-post-write.sh         auto-build after every .cs/.csproj write; errors AND
                                 warnings feed back to Claude (exit 2) for self-correction
    protect-paths.sh             blocks hand-edits to Migrations/, lock files, prod config
    format-on-stop.sh            end-of-turn dotnet format check (non-blocking notice)
```

## How the pieces divide the work

| Mechanism | Loads | Best for | In this kit |
| --- | --- | --- | --- |
| CLAUDE.md | Every session, in full | Facts Claude needs always: build commands, architecture, hard rules | Solution guide |
| Rules (`.claude/rules/`) | At launch, or when a matching file is touched (`paths:` frontmatter) | Conventions scoped to a file type or layer | 4 path-scoped + 2 global |
| Skills | Description always visible; body only when invoked (by you or Claude) | Multi-step procedures; long reference that shouldn't cost context all the time | 7 procedures |
| Commands | Only when you type `/name` | Fixed-structure prompts you trigger deliberately | 8 workflows |
| Subagents | Own context window, own prompt/tools/model | Verbose or specialized work whose output shouldn't flood your session | 6 specialists |
| Hooks | Deterministically at lifecycle events | Things that must *always* happen, regardless of what the model decides | build gate, path protection, format check |

Note: commands and skills are now unified in Claude Code — a file in `.claude/commands/` and a `SKILL.md` both create a `/name`. This kit keeps side-effectful, deliberately-triggered workflows in `commands/` (several use `disable-model-invocation: true`) and reusable procedures with auto-application in `skills/`.

## Try it

```
# In your solution root:
claude

> What did I change?                         # summarize-changes skill triggers
> /add-endpoint POST /books/{id}/ratings rate a book
> Use the code-reviewer subagent on my recent changes
> While I keep working, have the test-writer subagent fill the top coverage gaps in the background
> /security-review
> /commit
```

Then make a deliberate syntax error in a `.cs` file and ask Claude to save it — watch the PostToolUse hook surface the build failure and Claude fix it unprompted.

## Customizing

- Tighten or loosen `permissions.allow`/`deny` in `settings.json` first — it's the highest-leverage safety dial.
- Subagent models: `test-writer` and `docs-writer` run on Haiku deliberately; switch `model:` to `sonnet`/`inherit` if you want more judgment at more cost.

### Choosing a model per agent

Set the `model:` field in each agent's frontmatter. One agent = one model; to run the same role at two capability levels, create two agent files (e.g. `quick-reviewer` on `haiku`, `deep-reviewer` on `inherit`).

| Task profile | `model:` value | Why | Kit examples |
| --- | --- | --- | --- |
| Mechanical, high-volume (test generation, doc writing, formatting-level edits) | `haiku` | Cheapest and fastest; the procedure in the prompt does the heavy lifting | `test-writer`, `docs-writer` |
| Judgment-heavy (review, audits, performance analysis, planning) | `inherit` | Analysis quality tracks whatever model your session runs | `code-reviewer`, `security-auditor`, `perf-analyzer`, `migration-planner` |
| Reproducibility across a team or CI (identical behavior everywhere, always) | full model string, e.g. `claude-haiku-4-5-20251001` | Aliases float to newer versions; a pinned string never changes under you | — (opt in per team) |
| No strong opinion | omit the field | Falls back to the configured default subagent model | — |

Rule of thumb: if the agent's prompt is a checklist, Haiku executes it well; if the agent must notice what the checklist missed, inherit your session's model.
- Personal preferences go in `~/.claude/CLAUDE.md` or `~/.claude/rules/`, not this project's files.
- The `security-auditor` uses `memory: project` — its accumulated findings live in `.claude/agent-memory/security-auditor/` and can be committed to share institutional knowledge.
