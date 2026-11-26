# Day 1 Demo D5: Subagent markdown + Async

| | |
|---|---|
| **Slide deck** | `decks/Day 1/Section2-Steering-Claude-Code.pptx` |
| **Scheduled** | 11:52 AM (Section 2, Block 2C — after the subagent slides) |
| **Duration** | 12 min |
| **Risk** | Medium (async notify timing is out of your control — have the screenshot fallback ready) |
| **Internet** | Not required |
| **GitHub issue** | https://github.com/cwoodruff/dotnet-claude-workshop/issues/9 |

## What this demo teaches

- Subagents are **`.md` files** (*correction* from the previous workshop version — not `.yaml`):
  `.claude/agents/<name>.md`, YAML frontmatter (`name`, `description`, optional `tools`,
  `model`) + the body **is the system prompt**.
- **Isolated context:** the subagent runs in its own window; only the **final summary returns**
  to the main thread — heavy, noisy work never floods your context.
- **`model:` as a cost lever:** mechanical work goes to Sonnet 5.5 while the main session stays on
  Opus 5.5.
- **Async:** spawn the subagent, keep working in the main session, get **notified** when it
  finishes.
- The decision rule: **skill = in-thread and steerable; subagent = out-of-sight, hands back a
  summary.**

## Prerequisites / setup

- [ ] Copy this demo's artifacts into the BookTracker solution (run from the repo root):

      ```bash
      cp -R "demos/Day 1/07-subagent-async/artifacts/.claude/." "src/BookTracker/.claude/"
      ```

      This stages/refreshes `.claude/agents/test-writer.md` (runs on Sonnet 5.5).
      Note: `src/BookTracker/.claude/agents/` ships a shorter staged copy; the copy overwrites it
      with this demo's fuller version. That is intended.
- [ ] **Remove any stale `.claude/agents/*.yaml`** — subagents are markdown; a leftover YAML
      undermines the correction you're teaching:

      ```bash
      ls "src/BookTracker/.claude/agents/"
      ```

- [ ] `claude` running from `src/BookTracker/`; `dotnet build BookTracker.sln --nologo` and
      `dotnet test BookTracker.sln --nologo` both pass beforehand (the subagent will run tests).
- [ ] **Fallback screenshot ready:** a saved screenshot of a returned test-writer summary, in case
      the async notification is slow on stage (this is the Medium-risk item).
- [ ] Pick the target class: `BookTracker.Core/Services/BookService.cs` (real logic, thin
      existing coverage — `BookTracker.Tests` is intentionally sparse).

## Step-by-step script

1. **Show the file — it's markdown.** Open `.claude/agents/test-writer.md` in the editor.
   Narrate the anatomy top to bottom:

   > "A subagent is one markdown file. Frontmatter: `name`, `description` — which, like a skill,
   > tells the main agent *when to delegate* — `tools` to limit what it can touch, and `model:
   > claude-sonnet-5-5`, the cost lever. Everything below the frontmatter is its **system prompt**. Not YAML,
   > not JSON — markdown. That's the correction from the last version of this workshop."

2. **Spawn it async.** In the Claude session:

   ```text
   Use the test-writer agent to write xUnit tests for BookTracker.Core/Services/BookService.cs.
   Run it in the background — I want to keep working while it runs.
   ```

   Expected output: Claude launches the `test-writer` agent (you'll see the Task/agent spawn in
   the transcript) and control returns to your prompt.

3. **Keep working in the main session — this is the async payoff.** Immediately ask something
   unrelated:

   ```text
   While that runs: which endpoint groups in BookTracker.Api/Endpoints are missing pagination
   on collection routes?
   ```

   Narrate while both run: "Two contexts. The test writer is reading `BookService`, the test
   project, running `dotnet test` — hundreds of lines of noise — and none of it lands in this
   window. I keep my context for my work."

4. **The notification.** When the subagent finishes, the main session is notified and Claude
   relays the summary. Expected output: a short report — tests added, cases covered (happy path,
   missing-author failure, cancellation), pass count — **not** the full test file contents.

5. **Prove the isolation.** Run:

   ```text
   /context
   ```

   Point out the main session's context grew only by the summary. Then show the new test file on
   disk (`BookTracker.Tests/`) — real artifact, none of the noise.

6. **Land the decision rule** (verbatim):

   > "Skill = in-thread and steerable — you watch it work and can redirect. Subagent =
   > out-of-sight, hands back a summary. Reach for a subagent when you *don't* want the details
   > in your main context. And `model: claude-sonnet-5-5` means the mechanical work ran at a fraction of the
   > cost."

7. **(If time)** run the generated tests live: `dotnet test BookTracker.sln --nologo`.

## Recovery

- **Async notify is slow (the expected failure):** don't stall — this is scripted. Keep step 3's
  main-session work going and narrate the isolated-context benefit while it runs. If it's still
  not back after ~3 minutes, show the **prepared screenshot** of a returned summary, land the
  callouts, and let the real summary interrupt you whenever it arrives (which is itself a nice
  beat: "there's the notification").
- **Claude runs the task in-thread instead of spawning the agent:** be explicit — re-prompt with
  "Spawn the `test-writer` subagent as a background task for this." If it still won't, name the
  fix: the agent's `description` is the delegation trigger; sharpen it live (same lesson as D4).
- **Agent not found:** verify `src/BookTracker/.claude/agents/test-writer.md` exists and you
  launched `claude` from `src/BookTracker/`.
- **Generated tests fail to build:** fine — the subagent's workflow iterates with
  `dotnet test --filter` until green; if it returned red anyway, say "summaries can lie — verify"
  and segue: deterministic verification is exactly what the next demo's hooks are for.

## Teaching callouts

- **Markdown, not YAML** — the file format correction is a headline point for returning attendees.
- **The body is the system prompt.** Everything you know about prompting applies; a subagent is a
  reusable, named prompt with its own context and tool belt.
- **Summary-only return is the feature**, not a limitation: big reads, exploratory analysis, and
  test churn stay out of your window. Cheap to the parent.
- **`model:` is a real cost lever** — Sonnet 5.5 for mechanical generation, main session stays on
  Opus 5.5. Teams route work this way deliberately.
- **`tools:` is a guardrail too:** this agent gets Read/Grep/Glob/Write/Edit/Bash — and its prompt
  forbids touching production code. Frontmatter narrows *capability*; the next demo (D6) shows
  narrowing by *enforcement*.
