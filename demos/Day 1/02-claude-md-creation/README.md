# Day 1 Demo 2: CLAUDE.md Creation

| | |
|---|---|
| **Slide deck** | `decks/Day 1/Section1-Foundations-Claude-Code-CLI.pptx` (slides 8–10) |
| **Scheduled** | 9:45 AM |
| **Duration** | 10 min |
| **Risk** | Low |
| **Internet** | No |
| **GitHub issue** | <https://github.com/cwoodruff/dotnet-claude-workshop/issues/4> |

## What this demo teaches

- `CLAUDE.md` is the **project briefing**: loaded at session start, it becomes part of the system prompt — every line you keep is paid for on every turn.
- `/init` scaffolds a first draft from the codebase; you **trim it to a map**, never ship the raw scaffold.
- `#` at the start of a line appends a fact to CLAUDE.md mid-session.
- Facts and soft conventions belong here; hard "never do X" prohibitions don't — those become hooks in Section 2.

## Prerequisites / setup

- [ ] BookTracker cloned; terminal `cd`'d into the solution folder: `src/BookTracker` (quote the path if your shell needs it — the repo root path may contain spaces).
- [ ] `dotnet build BookTracker.sln` succeeds from that folder (night-before checklist).
- [ ] **Important:** the shipped `src/BookTracker/CLAUDE.md` already exists. For the demo to show `/init` generating fresh, move it aside beforehand and restore it after:

  ```bash
  cd "src/BookTracker"
  mv CLAUDE.md CLAUDE.md.shipped
  ```

  (Restore after the demo with `mv CLAUDE.md.shipped CLAUDE.md`, or keep your trimmed demo version if you prefer — but restore before Section 2, whose demos assume the staged config.) Alternative low-risk play: skip the `mv`, run `/init` anyway, and teach from how it proposes updating the existing file — but the fresh-scaffold version lands the "map, not junk drawer" point harder.
- [ ] `claude` authenticated (Demo 1 just proved it).
- [ ] An editor you can flip to for the trimming step (or plan to have Claude do the trimming in-session — script below covers both).

## Step-by-step script

1. **Start the session.** From `src/BookTracker`:

   ```bash
   claude
   ```

   Say: "Right now Claude knows nothing about this project except what it can read. Let's give it a standing briefing."

2. **Scaffold.**

   ```text
   /init
   ```

   **Expected:** Claude explores the solution (watch the file reads scroll by — narrate them: "it's reading the `.sln`, the csprojes, `Program.cs`…") and proposes a `CLAUDE.md`. It will typically be long — commands, architecture, conventions, sometimes exhaustive file inventories. Accept the write.

3. **Critique it live (this is the core teaching moment).** Scroll the generated file on screen. Say: "Don't ship this. CLAUDE.md is loaded into the system prompt **every session** — every line here costs context on every turn, all day. It's a map, not a junk drawer. Keep only what Claude needs *all the time*."

4. **Trim it to a map.** Either edit in your editor, or stay in-session and prompt:

   ```text
   Rewrite CLAUDE.md down to a lean map, under 60 lines: a one-paragraph project description
   (ASP.NET Core 10 Minimal API + EF Core + SQLite), the four essential commands (build, test,
   run the API, add a migration), and the architecture in three lines — BookTracker.Core has no
   dependencies, BookTracker.Data depends on Core, BookTracker.Api depends on both; Tests
   references all. Drop everything else.
   ```

   **Expected result** — a file shaped like this (yours will vary; the shape is the point):

   ```markdown
   # BookTracker

   ASP.NET Core 10 Minimal API for tracking books, EF Core 10 + SQLite. Solution: BookTracker.sln.

   ## Commands
   - Build: dotnet build BookTracker.sln
   - Test:  dotnet test BookTracker.sln
   - Run:   dotnet run --project BookTracker.Api
   - Migration: dotnet ef migrations add <Name> --project BookTracker.Data --startup-project BookTracker.Api

   ## Architecture
   - BookTracker.Core — entities, DTOs, service interfaces. Depends on nothing.
   - BookTracker.Data — DbContext, repositories, migrations. Depends on Core.
   - BookTracker.Api  — Minimal API endpoints (Endpoints/*.cs, wired in Program.cs). Returns DTOs, never entities.
   ```

   Point at each section: "Description, four commands, architecture. That's the whole map."

5. **Append a fact mid-session with `#`.** Type in the REPL, leading character literally `#`:

   ```text
   # DTOs live in BookTracker.Core/Dtos; endpoints must return DTOs, never EF entities.
   ```

   **Expected:** Claude Code offers to add the line to memory — choose the **project CLAUDE.md**. Show the file now contains the line. Say: "That's the mid-session habit: learn a fact once, pin it with `#`, never re-explain it."

6. **Land the boundary for Section 2.** Say: "Notice what I did *not* put in here: 'never run rm -rf', 'never commit secrets'. A CLAUDE.md 'never' is a suggestion the model may follow. Real prohibitions are deterministic **hooks and permissions** — Section 2, right after the break."

7. **Hand-off.** "In Lab 1 Part B you write your own — description, four commands, five conventions, architecture notes — and commit it: `git add CLAUDE.md && git commit -m "feat: configure CLAUDE.md for workshop"`."

## Recovery

- **`/init` over-generates wildly:** perfect — that *is* the teaching point. Edit it down live and narrate every deletion ("does Claude need this on every turn? No — cut").
- **`/init` under-generates or misses the architecture:** also teachable — add the missing facts by hand or with the trim prompt above; the lesson becomes "the scaffold is a draft, you are the editor."
- **You forgot to move the shipped CLAUDE.md aside:** don't restart — pivot. `/init` against an existing file demonstrates that it proposes improvements to what's there; then teach trimming on the merged result.
- **The `#` append targets the wrong file (user-level memory):** re-do it and pick the project-level CLAUDE.md at the prompt; briefly name the difference (user memory follows *you*, project CLAUDE.md follows the *repo* and gets committed).
- **Session state feels confused after heavy editing:** `/clear` and restart the session — a nice bonus demo that CLAUDE.md is re-loaded fresh at session start (ask "what are the build commands for this project?" and watch it answer from the briefing without reading files).

## Teaching callouts

- "CLAUDE.md is loaded every session and becomes part of the system prompt — every line is paid for on every turn."
- "It's a map, not a junk drawer — keep it under ~200 lines; ours is under 60."
- "`/init` gives you a draft; you're the editor. Facts and soft conventions stay, everything else goes."
- "Prohibitions don't belong here — a CLAUDE.md 'never' is a suggestion. Real guardrails are hooks, coming in Section 2."
