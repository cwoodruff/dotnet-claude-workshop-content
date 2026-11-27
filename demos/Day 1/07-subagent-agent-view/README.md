# Day 1 Demo D5: Subagent markdown + Agent View

| | |
|---|---|
| **Slide deck** | `decks/Day 1/Section2-Steering-Claude-Code.pptx` (slides 9 and 10) |
| **Scheduled** | Section 2, Block 2C, after the subagent slide (clock time: see the timing guide) |
| **Duration** | 12 min (about 4 for the subagent file, 8 for Agent View and `/goal`) |
| **Risk** | Medium (background sessions finish on their own schedule; have the screenshots ready) |
| **Internet** | Yes, for Claude Code's model connection (nothing else) |
| **Mode** | Demo only, not hands-on: several background sessions at once burn quota fast, and a room on Pro plans could rate-limit itself before Lab 2 |
| **GitHub issues** | https://github.com/cwoodruff/dotnet-claude-workshop/issues/9 (original), https://github.com/cwoodruff/dotnet-claude-workshop/issues/46 (Agent View) |

## What this demo teaches

- Subagents are **`.md` files** (*correction* from the previous workshop version, not `.yaml`):
  `.claude/agents/<name>.md`, YAML frontmatter (`name`, `description`, optional `tools`,
  `model`) + the body **is the system prompt**.
- **`model:` as a cost lever:** mechanical work goes to Sonnet 5.5 while the main session stays on
  Opus 5.5.
- **Background sessions replace "async subagents".** Instead of a subagent you wait on, a whole
  session runs detached: `claude --bg "<prompt>"` starts one, `/bg` (short for `/background`)
  sends the current one there, and `claude agents` opens **Agent View**, one screen for every
  session you have running.
- **`/goal <condition>`** keeps a session working until the condition is met. An evaluator model
  checks the condition each time Claude would stop. It reads only the conversation (it can't run
  commands or open files), so the condition has to name visible evidence.
- **Auto mode is what makes it unattended.** In Manual mode a background session stops at every
  permission prompt and shows as needing input. The ladder slide right after this demo covers why
  Auto is convenient and also why it isn't a guardrail.
- The decision rule: **skill = in-thread and steerable; subagent = out-of-sight, hands back a
  summary; background session = a whole session you check on later.**

## Prerequisites / setup

- [ ] Copy this demo's artifacts into the BookTracker solution (run from the repo root):

      ```bash
      cp -R "demos/Day 1/07-subagent-agent-view/artifacts/.claude/." "src/c1/BookTracker/.claude/"
      ```

      This stages/refreshes `.claude/agents/test-writer.md` (runs on Sonnet 5.5).
      `src/c2/BookTracker/.claude/agents/` has the shorter attendee version from Lab 2; this
      demo's copy is the fuller one.
- [ ] **Remove any stale `.claude/agents/*.yaml`.** Subagents are markdown; a leftover YAML
      undermines the correction you're teaching:

      ```bash
      ls "src/c1/BookTracker/.claude/agents/"
      ```

- [ ] `dotnet build BookTracker.sln --nologo` and `dotnet test BookTracker.sln --nologo` both pass
      from `src/c1/BookTracker/` beforehand (the background session will run tests).
- [ ] **Check Agent View on the demo machine and account** the night before: run `claude agents`
      and confirm it opens. If your organization sets `disableAgentView`, `claude agents`, `--bg`
      and `/background` are all off; switch to the screenshot path below.
- [ ] **Check the permission mode indicator** in a fresh `claude` session. If it shows **auto**,
      background sessions run without stopping. If it shows **manual**, start them with
      `--permission-mode auto` where your account allows it, or plan to narrate the "needs input"
      state instead.
- [ ] **Clear old background sessions:** `claude agents --json` should print `[]` or only sessions
      you mean to show.
- [ ] **Fallback screenshots ready:** Agent View with two sessions running, and a finished
      session whose transcript ends with the goal met.
- [ ] Target class: `BookTracker.Core/Services/BookService.cs` (real logic, thin existing
      coverage; `BookTracker.Tests` is intentionally sparse).

## Step-by-step script

1. **Show the file: it's markdown** (about 2 min). Open `.claude/agents/test-writer.md`. Narrate
   the anatomy top to bottom:

   > "A subagent is one markdown file. Frontmatter: `name`, `description` (which, like a skill,
   > tells the main agent *when to delegate*), `tools` to limit what it can touch, and `model:
   > claude-sonnet-5-5`, the cost lever. Everything below the frontmatter is its **system prompt**.
   > Not YAML, not JSON: markdown. That's the correction from the last version of this workshop."

2. **Use it in the foreground once** (about 2 min). In a session in `src/c1/BookTracker/`:

   ```text
   Use the test-writer agent to list the untested public methods in
   BookTracker.Core/Services/BookService.cs. Don't write tests yet.
   ```

   Expected: Claude delegates, and only the subagent's short list comes back, not the files it
   read. "Isolated context: the reading happened in the subagent's window, not mine."

3. **Start a background session from the terminal** (about 1 min). In a second terminal, from
   `src/c1/BookTracker/`:

   ```bash
   claude --bg -n book-tests "Use the test-writer agent to write xUnit tests for BookTracker.Core/Services/BookService.cs, then run dotnet test BookTracker.sln --nologo and show the summary line."
   ```

   Expected: it prints a short id and returns immediately. "That's a whole Claude session, running
   detached. Not a subagent I'm waiting on: a session I can check on, attach to, or stop."

4. **Send the current session to the background** (about 30 s). Back in the first session, give it
   a second job and move it out of the way:

   ```text
   /bg Find the endpoint groups in BookTracker.Api/Endpoints whose collection routes have no pagination, and list them.
   ```

   Expected: the terminal is free again.

5. **Open Agent View** (about 2 min):

   ```bash
   claude agents
   ```

   Expected: both sessions listed with their state. Narrate: "One screen for every session. If a
   session needs me, for a permission prompt or a question, this is where it shows up." Open
   `book-tests` from the list (or `claude attach book-tests` from a terminal) to look inside.

6. **Set a goal** (about 2 min). Attached to `book-tests`:

   ```text
   /goal dotnet test BookTracker.sln output in this conversation shows 0 failed with the new BookService tests included, or three test runs in a row have failed
   ```

   Narrate: "`/goal` keeps the session going until an evaluator model agrees the condition holds.
   The evaluator only reads this conversation, so the condition names something it can see: test
   output. The 'three runs in a row' clause is the turn limit; without it, a stubborn failure could
   keep it going until my quota runs out." Detach and let it run.

7. **Land it** (about 30 s):

   > "Skill = in-thread and steerable. Subagent = out-of-sight, hands back a summary. Background
   > session = a whole session you check on later, and `/goal` keeps it on task. Skills and
   > subagents are rows in the decision table because they hold instructions. Background sessions
   > and `/goal` aren't; they change where and how long the work runs. And notice what lets a background session run without stopping: Auto mode.
   > That's the next slide."

8. **Clean up** after the section (not on stage): `claude stop <id>`, then `claude rm <id>` once
   you've checked the tests it wrote. The id is the one `--bg` printed; `claude agents --json` lists
   them again. Idle background sessions also stop by
   themselves after about an hour.

## Recovery

- **Agent View won't open or `--bg` is refused:** the account or organization has it off. Show the
  screenshots and narrate steps 3 to 6; the concepts carry without the live run.
- **A session shows it needs input:** it hit a permission prompt because it's in Manual mode. Make
  this the teaching beat: attach, approve, and say "this is exactly what Auto mode is for, and
  exactly why it needs a hook behind it."
- **The background session is slow:** don't wait on stage. Move on to the ladder slide and check
  `claude agents` again before Lab 2.
- **Claude runs the task itself instead of delegating to the subagent (step 2):** re-prompt with
  "Use the `test-writer` subagent for this." If it still won't, the agent's `description` is the
  delegation trigger; sharpen it live (same lesson as D4).
- **Agent not found:** verify `src/c1/BookTracker/.claude/agents/test-writer.md` exists and you
  launched `claude` from `src/c1/BookTracker/`.
- **Generated tests fail to build:** say "summaries can lie, verify" and segue: deterministic
  verification is what the hooks coming up are for.

## Teaching callouts

- **Markdown, not YAML:** the file format correction is a headline point for returning attendees.
- **The body is the system prompt.** A subagent is a reusable, named prompt with its own context
  and tool belt.
- **Summary-only return is the feature:** big reads and test churn stay out of your window.
- **`model:` is a real cost lever:** Sonnet 5.5 for mechanical generation, main session on
  Opus 5.5.
- **Background sessions cost like sessions.** Three at once is three sessions' worth of quota.
  That's why this is a demo and not a lab step.
- **Write goals the evaluator can check.** "Tests pass" is vague; "the dotnet test output in this
  conversation shows 0 failed" names the evidence. Add a stop clause so a goal can't run forever.
