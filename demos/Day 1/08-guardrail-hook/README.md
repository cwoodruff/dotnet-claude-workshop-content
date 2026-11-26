# Day 1 Demo D6: Guardrail Hook — `PreToolUse` exit 2

| | |
|---|---|
| **Slide deck** | `decks/Day 1/Section2-Steering-Claude-Code.pptx` |
| **Scheduled** | 12:08 PM (Section 2, Block 2C — after the hooks slides) |
| **Duration** | 8 min |
| **Risk** | Low |
| **Internet** | Yes, for Claude Code's model connection (nothing else) |
| **GitHub issue** | https://github.com/cwoodruff/dotnet-claude-workshop/issues/10 |

## What this demo teaches

- Hooks are **deterministic automation that runs as code, on events, outside the model's choice** —
  the model never gets a vote.
- **A `PreToolUse` hook that exits 2 blocks the tool call**, and its stderr text is fed back to
  Claude as the reason. *This* is why "never do X" belongs in a hook, not CLAUDE.md: a CLAUDE.md
  "never" is a suggestion the model may follow; an exit-2 hook is enforcement that cannot be
  skipped.
- **`PostToolUse`** for "every time X, do Y": a `dotnet build` after every Write/Edit, so compile
  errors flow straight back into Claude's context.
- Hooks are **registered in `settings.json`** (also possible: managed settings org-wide, or a
  skill's frontmatter) — and settings load at **session start**.

## Prerequisites / setup

- [ ] Copy this demo's artifacts into the BookTracker solution (run from the repo root):

      ```bash
      cp -R "demos/Day 1/08-guardrail-hook/artifacts/.claude/." "src/c1/BookTracker/.claude/"
      ```

      This stages/refreshes `.claude/hooks/block-destructive.sh` and `.claude/settings.json`.
      Lab 2's attendee `settings.json` (in `src/c2/BookTracker/.claude/`) registers the same two
      hooks; this demo's version adds a build timeout and a stricter pattern list.
- [ ] **Make the hook executable** (`cp` normally preserves the bit, but verify — a missing +x is
      the #1 way this demo dies):

      ```bash
      chmod +x "src/c1/BookTracker/.claude/hooks/block-destructive.sh"
      ```

- [ ] **Smoke-test the hook standalone** — it reads the tool-call JSON from stdin:

      ```bash
      echo '{"tool_input":{"command":"rm -rf bin"}}' | "src/c1/BookTracker/.claude/hooks/block-destructive.sh"; echo "exit=$?"
      ```

      Expected: `Blocked by guardrail: recursive force delete ('rm -rf') is not allowed...` and
      `exit=2`. Also check the allow path:

      ```bash
      echo '{"tool_input":{"command":"dotnet build"}}' | "src/c1/BookTracker/.claude/hooks/block-destructive.sh"; echo "exit=$?"
      ```

      Expected: no output, `exit=0`.
- [ ] `jq` installed (`which jq`) — the hook uses it to parse the tool JSON.
- [ ] **Start a FRESH `claude` session from `src/c1/BookTracker/` AFTER copying** — hooks in
      `settings.json` load at session start. A session that was already running will not see them.
      This is non-negotiable and is also the demo's own recovery lesson.
- [ ] `dotnet build BookTracker.sln --nologo` passes (so the PostToolUse hook demo shows a clean
      build, not pre-existing errors).

## Step-by-step script

1. **Show the registration first.** Open `.claude/settings.json` and narrate:

   > "Two hooks. `PreToolUse` with matcher `Bash` — before Claude runs *any* shell command, this
   > script gets the tool call as JSON on stdin. `PostToolUse` with matcher `Write|Edit` — after
   > every file change, `dotnet build` runs. Neither is a prompt. The model doesn't get a vote."

2. **Show the script for 20 seconds.** Open `.claude/hooks/block-destructive.sh`; point at one
   pattern (`rm -rf`) and the two-line payoff:

   ```bash
   echo "Blocked by guardrail: ..." >&2
   exit 2
   ```

   > "Exit 2 means *blocked*. The stderr line is fed back to Claude as the reason."

3. **Ask Claude to do something destructive.** In the fresh session:

   ```text
   Clean the build output: run rm -rf bin obj in BookTracker.Api.
   ```

   **Expected output:** the Bash call is **blocked** before it executes; the transcript shows the
   hook's stderr reason (`Blocked by guardrail: recursive force delete ('rm -rf') is not
   allowed...`), and Claude adapts — typically proposing `dotnet clean` or deleting specific
   files. Narrate: "The command never ran. Not 'Claude decided not to' — it *couldn't*."

4. **Push the point (optional, 30s):** try a second category:

   ```text
   Run: git reset --hard HEAD~1
   ```

   Blocked again, different reason. "One script, a whole class of disasters fenced off."

5. **Now the PostToolUse hook.** Make a real, harmless edit:

   ```text
   Add an XML doc comment to the MapBookEndpoints method in BookTracker.Api/Endpoints/BookEndpoints.cs.
   ```

   **Expected output:** after the Edit lands, the `PostToolUse` hook fires and `dotnet build
   BookTracker.sln --nologo -v q` runs automatically in the transcript. Narrate: "I never asked
   for a build. Every edit gets one. If it fails, the errors land in Claude's context and it
   fixes them — the loop closes without me."

6. **Land the line** (verbatim from the facilitator script):

   > "A CLAUDE.md 'never' is a suggestion the model may follow. A `PreToolUse` exit-2 hook is
   > enforcement that cannot be skipped. This is the real guardrail."

## Recovery

- **The destructive command was NOT blocked** — two causes, in order of likelihood:
  1. **Stale session** — you edited/copied `settings.json` after starting `claude`. Fix live and
     name it: "settings load at session start" — restart the session, retry. This recovery *is*
     part of the lesson.
  2. **Missing execute bit** — `chmod +x .claude/hooks/block-destructive.sh`, restart, retry.
  Also worth a glance: `which jq` (the script needs it) and that you launched from
  `src/c1/BookTracker/`.
- **Hook blocks but no reason shows:** the reason goes to stderr — scroll the tool result in the
  transcript; the standalone smoke test from setup proves the script side in five seconds.
- **PostToolUse build is slow or fails on pre-existing errors:** the hook has a 120s timeout; if
  the build was red before the demo, that's your environment — fall back to narrating the D6
  smoke-test output and move on.
- **Claude refuses the `rm -rf` on its own** (before the hook can block): amusing but off-message.
  Re-prompt more insistently ("I understand the risk, run it anyway") — the hook, not the model's
  judgment, should visibly be the thing that stops it.

## Teaching callouts

- **Authority column, made flesh.** This is the payoff of the D-STEER table: hooks are the only
  row with real *enforcement*. Prohibitions migrate out of CLAUDE.md and into hooks/permissions.
- **Exit codes are the contract:** 0 = allow, 2 = block + stderr fed back to the model. Anything
  the model reads back, it can act on — a blocked call with a good reason produces a *better*
  next attempt.
- **`PostToolUse` closes the verification loop deterministically** — "run the build after every
  change" stops being an instruction Claude might skip and becomes physics.
- **Events beyond these two:** `SessionStart`, `Stop`, `PreCompact`; hook types beyond `command`:
  HTTP, mcp_tool, prompt, agent. Registration also works in managed settings (org-wide,
  non-overridable) — that's the enterprise "never" story.
- **Fresh-session requirement** is the #1 support question in Lab 2 — you just demoed the fix.
