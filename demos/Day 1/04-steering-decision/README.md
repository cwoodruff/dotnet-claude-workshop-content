# Day 1 Demo D-STEER: The Steering Decision

| | |
|---|---|
| **Slide deck** | `decks/Day 1/Section2-Steering-Claude-Code.pptx` |
| **Scheduled** | 10:48 AM (Section 2, right after the steering-model slides) |
| **Duration** | 5 min |
| **Risk** | Low (purely conceptual — no live tooling) |
| **Internet** | No (slides and a quiz; no Claude Code session) |
| **GitHub issue** | https://github.com/cwoodruff/dotnet-claude-workshop/issues/6 |

## What this demo teaches

- The single framing question for all of Section 2: **"where does an instruction belong?"**
- The seven-methods steering table, read **by column**: *when it loads · survives compaction ·
  context cost · authority*.
- The mental shortcut: the more **deterministic and always-needed** a piece of guidance is, the
  "lower" it lives (hook → rule → CLAUDE.md); the more **procedural or occasional**, the more it
  belongs in a skill or subagent.
- The **three decisions** attendees will actually use:
  1. "Every time X, always do Y" → **hook** (the model *choosing* to run a formatter is not the
     same as it running).
  2. "Never do X" → **not a CLAUDE.md line**. Real guardrails are deterministic: hooks
     (`PreToolUse` exit 2), permissions, managed settings.
  3. A 30-line procedure → **skill**. CLAUDE.md is for facts Claude needs all the time.

## Prerequisites / setup

- [ ] The **Steering Reference Card** open on screen — either the published page
      `docs/Day 1/11-steering-reference-card.md` (dotnetclaude.com) or the PDF at
      `decks/steering-reference-card.pdf`. The table also lives in
      `docs/Day 1/05-section-2-steering.md` ("The steering model") if you prefer the source.
- [ ] The Section 2 deck (`decks/Day 1/Section2-Steering-Claude-Code.pptx`) on the steering-table
      slide, so you can flip between deck and card.
- [ ] No `.claude/` staging, no terminal, no build required. This demo has **no artifacts folder**.
- [ ] Optional: have `src/c2/BookTracker/.claude/` (the finished Lab 2 config) visible in your editor's file tree, so you can
      point at real homes (rules, skills, agents, hooks) as you name them — they are the exact
      files the next four demos use.

## Step-by-step script

1. **Frame the question.** Put the reference card on screen and say:

   > "Everything in this section answers one question: where does an instruction belong? You have
   > seven-plus places to put guidance for Claude, and they differ on four properties."

2. **Walk the table by column, not by row.** Trace each column top-to-bottom with the pointer:

   - **When it loads** — CLAUDE.md at session start; subdir CLAUDE.md on demand; rules always or
     on file-match; skills on `/name` or auto-match; subagents when spawned; hooks on events.
   - **Survives compaction** — CLAUDE.md is memoized and re-read after compaction; skills and
     hooks don't need to survive because they're invoked/executed, not remembered.
   - **Context cost** — CLAUDE.md is paid on *every* request (hence < 200 lines); skills cost only
     their one-line description until triggered; hooks cost **nothing** — they run outside the model.
   - **Authority** — this is the column people miss. A CLAUDE.md line is a suggestion. A
     `PreToolUse` exit-2 hook is enforcement.

3. **Land the shortcut.** Say it verbatim (it's on the card):

   > "The more deterministic and always-needed, the lower it lives: hook → rule → CLAUDE.md. The
   > more procedural or occasional, the more it belongs in a skill or subagent."

4. **Land the three decisions.** Read them off the card one at a time (every-time → hook;
   never → not-CLAUDE.md; 30-line procedure → skill) and pause after each.

5. **Quiz the room** (30–60 seconds — this is the checkpoint). Ask, and take shouted answers:

   ```text
   "Run dotnet build after every file edit" — where does it belong?        → hook
   "Never edit files under Migrations/" — where?                           → hook / permissions
   "DTOs live in BookTracker.Core/Dtos" — where?                           → CLAUDE.md (a fact)
   "The 7-step procedure for adding an endpoint" — where?                  → skill
   "EF conventions that only matter in BookTracker.Data" — where?          → path-scoped rule
   ```

   **Expected outcome:** attendees answer "where does this instruction belong?" unprompted.
   That's the success criterion for the demo.

6. **Bridge forward:** "The next four demos are those four homes, live, in BookTracker: a skill
   folder, path-scoped rules, a markdown subagent, and the guardrail hook."

## Recovery

None — this demo is purely conceptual with no failure mode. If timing slips, cut the quiz to two
questions (the hook and the skill) and land the three decisions; they're the payload. If the
reference card won't open, the same table is on the deck slide and in
`docs/Day 1/05-section-2-steering.md`.

## Teaching callouts

- **Authority is the forgotten column.** Everyone compares context cost; almost nobody asks "can
  the model ignore this?" A CLAUDE.md "never" can be ignored; an exit-2 hook cannot.
- **The table is a decision tool, not a spec.** Attendees keep the reference card open all through
  Lab 2 — tell them so.
- **Don't demo the features yet.** Resist the urge to open files here; the four demos that follow
  (D4, D4b, D5, D6) each own one row of this table. This demo owns the *columns*.
- **Compaction is why CLAUDE.md feels magical** — it's re-read after compaction, so it's the one
  thing Claude never forgets. That's also why every line in it costs you on every turn.
