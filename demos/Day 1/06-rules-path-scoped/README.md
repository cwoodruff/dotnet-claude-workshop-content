# Day 1 Demo D4b: Rules — path-scoped in C#

| | |
|---|---|
| **Slide deck** | `decks/Day 1/Section2-Steering-Claude-Code.pptx` |
| **Scheduled** | 11:38 AM (Section 2, Block 2B — after the skills demo D4) |
| **Duration** | 7 min |
| **Risk** | Low |
| **Internet** | Yes, for Claude Code's model connection (nothing else) |
| **GitHub issue** | https://github.com/cwoodruff/dotnet-claude-workshop/issues/8 |

## What this demo teaches

- The **Rules** feature (new in this workshop version): `.claude/rules/*.md` files.
- **Unscoped vs. path-scoped:** a rule with no `paths:` frontmatter is always loaded (a focused
  slice of CLAUDE.md); a rule with `paths:` globs loads **only when Claude reads a matching file**.
- Verifying what's loaded with **`/memory`**.
- The two honest gotchas: rules load on **read, not create**, and user-level (`~/.claude/rules/`)
  `paths:` scoping is currently unreliable — keep path-scoped rules at the project level.

## Prerequisites / setup

- [ ] Copy this demo's artifacts into the BookTracker solution (run from the repo root):

      ```bash
      cp -R "demos/Day 1/06-rules-path-scoped/artifacts/.claude/." "src/c1/BookTracker/.claude/"
      ```

      This stages/refreshes two rules:
      - `.claude/rules/api-endpoints.md` — `paths: BookTracker.Api/Endpoints/**`
      - `.claude/rules/data-access.md` — `paths: BookTracker.Data/**`

      Lab 2's finished versions of these rules are in `src/c2/BookTracker/.claude/rules/`.
- [ ] Verify the globs survived the copy:

      ```bash
      head -4 "src/c1/BookTracker/.claude/rules/api-endpoints.md" "src/c1/BookTracker/.claude/rules/data-access.md"
      ```

- [ ] Start a **fresh `claude` session from `src/c1/BookTracker/`** so the rule set is picked up
      cleanly and `/memory` output isn't cluttered by the previous demo.
- [ ] Know your three exhibit files: `BookTracker.Api/Endpoints/BookEndpoints.cs` (matches
      api-endpoints), `BookTracker.Core/Services/BookService.cs` (matches neither), and
      `BookTracker.Data/Repositories/BookRepository.cs` (matches data-access).

## Step-by-step script

1. **Show the rule files first.** Open `.claude/rules/api-endpoints.md` in the editor and point
   at the frontmatter:

   ```yaml
   ---
   paths:
     - "BookTracker.Api/Endpoints/**"
   ---
   ```

   Narrate: "A rule is a markdown file of conventions. The `paths:` glob is the whole trick —
   this one only loads when Claude reads a file under `BookTracker.Api/Endpoints/`. No glob means
   always loaded."

2. **Baseline: nothing loaded yet.** In the fresh session, run:

   ```text
   /memory
   ```

   Expected output: CLAUDE.md entries; **neither** path-scoped rule listed as loaded yet (nothing
   matching has been read). Say: "Right now these rules cost us nothing."

3. **Make Claude read a matching file:**

   ```text
   Read BookTracker.Api/Endpoints/BookEndpoints.cs and summarize what routes it maps.
   ```

4. **Show the rule fired.** Run `/memory` again. Expected output: the `api-endpoints` rule now
   appears as loaded (the `data-access` rule does not). Narrate: "Claude read a file matching the
   glob, so the endpoint conventions are now in context — and only those."

5. **Counter-example — no match, no load.** Ask:

   ```text
   Read BookTracker.Core/Services/BookService.cs and summarize what it does.
   ```

   Run `/memory` — **still no `data-access` rule** (Core doesn't match either glob). Say: "Scoped
   means scoped. Working in Core doesn't pay for API or EF conventions."

6. **(Optional, 30s)** Trigger the second rule by asking Claude to read
   `BookTracker.Data/Repositories/BookRepository.cs`, then `/memory` shows `data-access` loaded too.

7. **Land the two gotchas** (verbatim from the section page):

   > "Rules load on *read*, not *create* — a path-scoped rule meant to guide creating brand-new
   > files may never fire. Make creation-time rules unscoped. And keep path-scoped rules at the
   > project level: user-level `~/.claude/rules/` globs are unreliable right now."

8. **Bridge to the lab:** "In Lab 2 you'll migrate the conventions out of your Lab 1 CLAUDE.md
   into rules like these, trimming CLAUDE.md back to a map."

## Recovery

- **Rule doesn't fire on the matching file:** check, in order —
  1. The glob: `head -4 .claude/rules/api-endpoints.md` — it must be
     `BookTracker.Api/Endpoints/**` relative to the working directory (`src/c1/BookTracker/`).
  2. You launched `claude` from `src/c1/BookTracker/`, not the repo root (globs are relative to
     the project root Claude Code detected).
  3. It's a **project-level** rule, not one under `~/.claude/rules/` — user-level `paths:` is
     the known-unreliable case; say so, it's literally the teaching point.
  Fix live and re-run the read; rules re-evaluate on each file read, no restart needed.
- **`/memory` output looks different from expectations:** the exact rendering varies by Claude
  Code version — what matters (and what to point at) is the rule name appearing after the
  matching read and not before.
- **Time crunch:** cut step 6; the load / no-load contrast in steps 3–5 carries the demo.

## Teaching callouts

- **Rules are the middle of the steering ladder:** more targeted than CLAUDE.md, cheaper than
  always-loaded, less procedural than a skill. Conventions → rules; facts → CLAUDE.md;
  procedures → skills.
- **Read-not-create is the trap** everyone hits: a rule scoped to `**/Endpoints/**` won't guide
  Claude when it *creates* the first file in a new folder. Creation-time conventions go unscoped
  (or in CLAUDE.md).
- **Project-level beats user-level** for path-scoped rules today. User-level rules are fine
  unscoped.
- **`/memory` is the debugging tool** — whenever an attendee says "Claude ignored my rule," the
  first question is: did `/memory` show it loaded?
