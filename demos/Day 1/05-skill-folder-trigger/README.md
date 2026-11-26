# Day 1 Demo D4: Skill Folder + `/name` trigger

| | |
|---|---|
| **Slide deck** | `decks/Day 1/Section2-Steering-Claude-Code.pptx` |
| **Scheduled** | 11:18 AM (Section 2, Block 2B — after the "skills are folders" slides) |
| **Duration** | 8 min |
| **Risk** | Low |
| **Internet** | Yes, for Claude Code's model connection (nothing else) |
| **GitHub issue** | https://github.com/cwoodruff/dotnet-claude-workshop/issues/7 |

## What this demo teaches

- Skills are **folders**, not single files (*correction* from the previous workshop version):
  `SKILL.md` + `references/` (+ optionally `scripts/`).
- The skill's **description is its trigger, written for the model** — Claude *auto-matches* a
  skill to a request from the description alone; you don't have to type `/add-api-endpoint`.
- Skills are **lazy**: only the one-line description sits in context until the skill fires, so
  many skills don't bloat every turn.
- The **Gotchas section is the high-value part** — spend lines on what Claude would otherwise get
  wrong, not on the obvious.

## Prerequisites / setup

- [ ] Copy this demo's artifacts into the BookTracker solution (run from the repo root). This
      stages/refreshes the `add-api-endpoint` skill folder:

      ```bash
      cp -R "demos/Day 1/05-skill-folder-trigger/artifacts/.claude/." "src/c1/BookTracker/.claude/"
      ```

      Section 2 demos run in `src/c1/BookTracker` (the state after Lab 1, which has `CLAUDE.md` and no
      `.claude/` yet). Lab 2 ends at `src/c2`, whose `.claude/` holds the attendee version of
      this skill (with `references/endpoint-pattern.md`); this demo's version routes its
      references through `references/gotchas.md`.
- [ ] Verify the folder layout is what you'll show on screen:

      ```bash
      find "src/c1/BookTracker/.claude/skills/add-api-endpoint" -type f
      ```

      Expected: `SKILL.md` and `references/gotchas.md`.
- [ ] **Remove any stale single-file skills** under `src/c1/BookTracker/.claude/skills/` (a bare
      `*.md` directly in `skills/`) — the folder-not-file point is the demo.
- [ ] `claude` running from `src/c1/BookTracker/`. Skills are discovered per-message, so a fresh
      session is not strictly required, but starting one keeps `/context` clean for narration.
- [ ] `dotnet build BookTracker.sln --nologo` passes before you start (the skill ends with a build).

## Step-by-step script

1. **Show the folder, not a file.** In the editor (or terminal), display the layout:

   ```bash
   find .claude/skills/add-api-endpoint -type f
   ```

   Narrate: "A skill is a folder. `SKILL.md` holds the metadata and procedure; `references/`
   holds supporting docs the skill points Claude at; `scripts/` (when present) holds helpers it
   can run. The folder is the unit you share."

2. **Open `SKILL.md` and read only the frontmatter.** Point at `description:` and say:

   > "This description is not documentation for humans — it's the trigger, written for the model.
   > It's the only part that sits in context until the skill fires. Phrase it the way a user
   > would ask."

3. **Open `references/gotchas.md` for ten seconds.** Point out one line (the SQL-injection trap in
   `SearchByTitleAsync`) and say: "The Gotchas file is the high-value part — non-obvious traps in
   *this* codebase, not generic advice."

4. **Trigger by auto-match — do NOT type `/add-api-endpoint`.** In the Claude session, paste:

   ```text
   Add a GET /api/authors/{id}/books endpoint returning the author's books sorted by publication year.
   ```

   (This route does not exist in c1: `AuthorEndpoints.cs` has CRUD only. Don't use the old
   `GET /books/{id}/reviews` prompt here; c1 has no Reviews feature yet, it arrives in c2.)

5. **Narrate the match.** Expected output: Claude announces it is using the `add-api-endpoint`
   skill (a skill-invocation line appears), then follows the procedure — reads
   `references/gotchas.md` and an existing endpoint group, adds a DTO-returning handler to
   `BookTracker.Api/Endpoints/AuthorEndpoints.cs` (or a new group), threads the
   `CancellationToken`, and builds.

   Say: "I never named the skill. Claude matched my request against the description. That's why
   the description is written like a user request."

6. **Land the lazy-loading point** while it works: run `/context` and show that skills cost only
   their descriptions until triggered.

7. **(Optional, if ahead of schedule)** show the explicit trigger: `/add-api-endpoint` also works,
   and tab-completion lists it.

## Recovery

- **Skill doesn't auto-match** (Claude just starts coding without the skill): that *is* teachable
  — say "the description was too vague; watch me fix it." Open `SKILL.md`, sharpen the
  `description` to include the words you used ("add an endpoint", "create a GET"), send the prompt
  again in a new message. Descriptions are re-evaluated per message.
- **Skill not found at all:** check the copy landed at
  `src/c1/BookTracker/.claude/skills/add-api-endpoint/SKILL.md` and that you launched `claude` from
  `src/c1/BookTracker/` (project-level `.claude/` is relative to the working directory).
- **The implementation runs long:** you don't need it to finish. Once the skill has visibly
  triggered and Claude is reading `gotchas.md`, the point is made — press Escape, and move to the
  callouts.

## Teaching callouts

- **Folders are the unit** — this is the correction from the previous workshop version. A bare
  `.claude/skills/foo.md` is a stale pattern; `foo/SKILL.md` is current.
- **Description = trigger.** A vague description means the skill never fires. Write it with the
  user's words in it.
- **Gotchas are where the value is.** Anyone can write the seven obvious steps; the skill earns
  its keep by encoding the traps (`SearchByTitleAsync` injection, forgetting to wire the group in
  `Program.cs`, dropped `CancellationToken`).
- **Command vs. skill:** a saved one-shot prompt is a command (`.claude/commands/`); the moment it
  needs *steps* or judgment, it's a skill.
- **Bridge to D4b:** "Skills are procedures you invoke. Next: rules — conventions that load
  themselves when Claude touches matching files."
