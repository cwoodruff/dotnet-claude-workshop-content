# Day 1 Demo 9: Domain Modeling

| | |
|---|---|
| **Slide deck** | `decks/Day 1/Section4-AI-in-the-SDLC.pptx` (slide 11, "Live Demo," part 1) |
| **Scheduled** | 3:45 PM |
| **Duration** | 6 min |
| **Risk** | Low (local files plus Claude Code's model connection) |
| **Internet** | Yes, for Claude Code's model connection (nothing else) |
| **GitHub issue** | <https://github.com/cwoodruff/dotnet-claude-workshop/issues/13> |

## What this demo teaches

- Architecture analysis **under real constraints** — load, hosting budget, team size — not in the abstract. "What's the best architecture?" gets platitudes; "10k concurrent readers on one small VM" gets engineering.
- The **three-options pattern**: force Claude to produce three concrete options *with effort estimates* and a recommendation, so you see trade-offs instead of a single confident answer.
- Capturing the decision as a **MADR-format ADR** (Markdown Architecture Decision Record) committed next to the code, so the *reasoning* survives, not just the outcome.
- This is the "architecture design" fifth of the Section 4 outcome (architecture, construction, refactoring, testing, debugging).

## Prerequisites / setup

- [ ] Terminal running `claude` in `src/c3/BookTracker`, with the `.claude/` config (CLAUDE.md, rules) in place from earlier sections — the recommendation should respect the house architecture (`Core ← Data ← Api`).
- [ ] `dotnet build BookTracker.sln` is green (so Claude's file reads reflect a working tree).
- [ ] The constraint you'll state chosen in advance. Default: **"BookTracker must serve 10,000 concurrent readers on one small VM (2 vCPU / 4 GB), SQLite stays."** Have a backup constraint in your notes (e.g. "the team is two developers and the hosting budget is $20/month") in case an attendee asks for a rerun with different pressure.
- [ ] The MADR template staged: [`artifacts/adr-template.md`](artifacts/adr-template.md) in this folder. Either copy it into the repo beforehand (`docs/adr/adr-template.md`) or keep it open to paste — the demo ends by filling it in.
- [ ] `docs/adr/` directory decision made: the ADR will be written to `src/c3/BookTracker/docs/adr/0001-caching-strategy-for-hot-reads.md` (create the folder live; it's part of the show).
- [ ] Clock check: this is 6 minutes. Rehearse once; the trap is letting Claude's analysis run long-form while you read silently.

## Step-by-step script

1. **Frame it (30 s).** "Everything so far was code-level. Claude is also useful one level up — but only if you give it *your* constraints. Watch the difference a constraint makes."

2. **Ask under constraint (1 min to type, ~2 min for the answer).** In `claude`:

   ```text
   BookTracker must serve 10,000 concurrent readers on one small VM (2 vCPU / 4 GB RAM), and we
   keep SQLite. Read the current architecture first (the Api endpoints, Core services, and Data
   repositories). Then give me three options to get there, each with: what changes, rough effort
   in days, main risk, and what it does NOT solve. Finish with a recommendation and why.
   Do not implement anything.
   ```

   **Expected:** Claude reads real files first (`BookTracker.Api/Endpoints/`, `BookTracker.Core/Services/BookService.cs`, `BookTracker.Data/`), then produces three distinct options — typically along the lines of (a) in-process caching (`IMemoryCache`) on hot read endpoints, (b) response/output caching + read-optimized queries (`AsNoTracking`, projection to DTOs), (c) a bigger swing like a read-replica/CQRS-lite split or moving off SQLite — each with an effort estimate and a risk, plus one recommendation.

3. **Narrate the shape, not the content (1 min).** While it streams: "Three real options, effort in days, and what each *doesn't* solve — that last column is where the honesty lives. It read our actual endpoints before answering; this is grounded in *this* codebase, not a blog post." If an option ignores the constraint (e.g. proposes a Redis cluster on a single-VM budget), call it out live and make Claude revise — pushback is part of the show.

4. **Capture the decision as an ADR (2 min).** Take the recommendation and pin it:

   ```text
   Capture that recommendation as a MADR-format ADR at docs/adr/0001-caching-strategy-for-hot-reads.md.
   Use this structure: Title, Status (accepted), Date, Context and Problem Statement, Decision
   Drivers, Considered Options (all three, one line each), Decision Outcome with justification,
   and Consequences (good and bad). Keep it under a page.
   ```

   **Expected:** Claude writes the file (permission prompt for the write — approve it), matching the template in `artifacts/adr-template.md`. Open the file and skim the "Considered Options" and "Consequences" sections aloud.

5. **Land it (30 s).** "Commit this next to the code. Six months from now nobody remembers *why* SQLite plus a memory cache — the ADR does. In the Lab 4 bonus you'll run this exact loop yourself: constraint → three options → ADR → spike the winner."

   ```bash
   git add docs/adr/0001-caching-strategy-for-hot-reads.md
   git commit -m "docs: ADR 0001 - caching strategy for hot reads"
   ```

   (Commit live if the clock allows; otherwise show the command and move on — Demo 9b starts at 3:51.)

## Recovery

- **Claude Code loses its connection:** nothing here runs without it. Switch to your hotspot; if that fails too, walk the steps from the slides and the expected-output notes below.
- **Claude gives one option instead of three, or options without effort estimates:** don't accept it — reply "I asked for three options with effort in days; give me the other two" on camera. Making the model meet the brief *is* the demo.
- **The analysis runs long:** cut it off — "that's enough context, give me the three-line summary of each option now" — and skip to the ADR step. The ADR is the artifact; protect it.
- **An option is generic/ungrounded** (doesn't mention any BookTracker file): point at it — "which of our endpoints does this apply to?" — and let Claude self-correct. Narrate: "if it can't name the file, it didn't read the code."
- **Out of time entirely:** show the pre-written ADR template from `artifacts/adr-template.md`, describe what would go in it, and move to Demo 9b — the Lab 4 bonus covers the full exercise hands-on.

## Teaching callouts

- "Analyze under constraints, not in the abstract — the constraint is what turns opinions into engineering."
- "Three options with effort estimates and a recommendation — never accept a single confident answer for an architecture question."
- "'What it does NOT solve' is the highest-value line in each option."
- "Commit the ADR — the decision *and the reasoning* live next to the code, in version control, forever."
- "This same loop is the Lab 4 bonus: constraint → options → ADR → spike the recommendation with a cache-invalidation test."
