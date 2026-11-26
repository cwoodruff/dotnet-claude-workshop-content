# Day 1 Demo 10: Full Feature from Spec

| | |
|---|---|
| **Slide deck** | `decks/Day 1/Section4-AI-in-the-SDLC.pptx` (slide 11, "Live Demo," part 2) |
| **Scheduled** | 4:05 PM |
| **Duration** | 17 min |
| **Risk** | Medium (live multi-file implementation can drift — but recovery *is* the lesson) |
| **Internet** | No |
| **GitHub issue** | <https://github.com/cwoodruff/dotnet-claude-workshop/issues/15> |

## What this demo teaches

The full Day 1 acceleration loop in one sitting — every prior section pays off here:

- **Spec-first construction:** review the spec (cheap, one page of text) *before* the code (expensive) — Claude finds gaps in the spec pass, not in a week-old PR.
- **The approval gate:** "pause before changing more than 4 files" keeps the human in control of a multi-file change — no 40-file runaway.
- **Steering in action:** CLAUDE.md, the `.claude/rules/`, and the PostToolUse build hook (Section 2) shape the implementation without a single mid-flight reminder.
- **Staying in the loop:** review each diff, push back on at least one choice out loud, and make Claude justify or revise — the visible difference between supervising and rubber-stamping.
- **Drift recovery:** when the implementation violates the spec or a rule, pointing at the violated line and having Claude self-correct is the technique, not a failure.

## Prerequisites / setup

- [ ] **Read the spec first:** `src/BookTracker/specs/reading-progress.md` — the 6-section worked example (context/scope, API contract, data model, business rules incl. the forward-only status state machine, tests required, prohibitions). The whole demo drives from this file; know its sections cold, especially §4 (rules) and §6 (prohibitions), because the pushback and drift-recovery moments lean on them.
- [ ] Terminal running `claude` in `src/BookTracker`; `.claude/` config from Sections 1–2 loaded (CLAUDE.md, `api-endpoints`/`data-access` rules, PostToolUse build hook). A **fresh session** so the hook registration is live.
- [ ] `dotnet build BookTracker.sln` and `dotnet test BookTracker.sln` green before you start; Demo 9b's staged bug reverted (`git status` clean).
- [ ] **Starting-state decision (important):** this repo's `src/BookTracker` already contains the finished Reading Progress feature (`ReadingProgressRules`, `ReadingProgressService`, endpoints, tests). Two ways to run the demo — pick one in advance:
  - **A. Build-it-live (matches the script below):** demo from a checkpoint/branch *without* the feature (the pre-Lab-4 state), so Claude genuinely implements from the spec. This is the full-fat version.
  - **B. Spec-review + gap-fill (fallback, and fine on this repo as-is):** run the spec-review pass, then have Claude *verify* the existing implementation against the spec ("read the spec, read the implementation, report any rule it violates and any required test that's missing") and implement one missing slice. Less theater, same lessons.
- [ ] Rehearsed once end-to-end with a timer — 17 minutes is tight for a multi-file build; know which steps you'll cut (see Recovery).
- [ ] Have `specs/reading-progress.md` open in a second pane/editor so you can point at the exact line during pushback.

## Step-by-step script

1. **Frame + skim the spec (2 min).** Open `specs/reading-progress.md` on screen. Walk the six section headers in 60 seconds: "Context and scope — note the explicit *out of scope*. API contract with status codes. Data model. Business rules — a real state machine, forward-only, `Completed` is terminal. The tests it must pass. And prohibitions — three explicit 'do not's. One page. This page is the contract; now watch it earn its keep." Remind the room this is the very issue filed via MCP in Demo 8.

2. **Review the spec before any code (3 min).** In `claude`:

   ```text
   Read specs/reading-progress.md. Identify gaps, ambiguities, and edge cases. Do NOT implement yet.
   ```

   **Expected:** Claude reads the spec and surfaces real questions — typical catches: what happens on `PUT` when no progress record exists yet (create-on-first-write or 404?); can `CurrentPage` change while status stays `Completed`; is `WantToRead` with a non-zero page valid; time zone for "today" on the date rules; does deleting a book cascade to its progress row. Pick **one** catch, resolve it out loud ("good catch — first PUT creates the record; the spec implies it, let's make it explicit"), and optionally edit the spec line live. Say: "That review cost thirty seconds of reading a page of text. Every one of those would have been a rework cycle if we'd found it in code."

3. **Implement with the approval gate (7–8 min).**

   ```text
   Implement specs/reading-progress.md. First read the spec and the relevant existing files
   (the Endpoints, the Core services, and the Data layer). Follow CLAUDE.md and the rules under
   .claude/rules. Build after each change. Pause for my approval before changing more than 4 files
   at once.
   ```

   **Expected:** Claude reads first (`BookTracker.Api/Endpoints/`, `BookTracker.Core/Services/`, `BookTracker.Data/`), proposes a plan touching Core (entity + `ReadingStatus` enum + rules + service + DTOs), Data (repository + migration), Api (`ReadingProgressEndpoints.cs` + `Program.cs` wiring), and Tests — then **pauses at the 4-file gate** for approval. The PostToolUse hook fires builds after edits; narrate the first one: "I didn't ask for that build — the hook did. Deterministic, every time."

   While diffs stream, review them **out loud** — "rules as pure logic in `Core/Domain`, no EF — that's our data-access rule doing its job."

4. **Push back once, visibly (2 min).** Find one choice to challenge — even a defensible one. Reliable candidates: validation logic appearing in the endpoint instead of the Core rules class; the endpoint returning the entity instead of a DTO; a missing `CancellationToken`. Say it to Claude, not the room:

   ```text
   Why is that validation in the endpoint instead of the Core service? The spec's acceptance
   criteria say rules live in Core/Domain. Justify it or move it.
   ```

   **Expected:** Claude either justifies with a reason you accept, or revises. Both outcomes are wins. Tell the room: "That's the loop. I'm not rubber-stamping diffs; I'm reviewing them like a PR — and the spec gives me something objective to point at."

5. **Verify against the spec (2 min).** Close the loop with the spec's own §5:

   ```text
   Run the tests. Then check the implementation against sections 5 and 6 of
   specs/reading-progress.md — list each required test and each prohibition, and whether
   it's satisfied.
   ```

   **Expected:** `dotnet test` green (or the hook already showed it); a checklist mapping each required test (page bounds, `WantToRead→Reading` sets `StartedOn`, `Reading→Completed` fills pages, skip rejected, `Completed→Reading` rejected, GET 404) and each prohibition to its evidence.

6. **Land it (1 min).** "Spec → review the spec → implement against spec plus rules with an approval gate → verify against the spec's own acceptance criteria. That is Lab 4, which starts *now* — you'll write your own spec for this same feature, and in Part C you'll break it on purpose and run yesterday's debugging loop. Protect time for Part C."

   ```bash
   git add -A && git commit -m "feat: reading-progress feature per spec"
   ```

## Recovery

- **Implementation drifts from the spec or a rule** (EF entity across the API boundary, logic in the endpoint, a skipped status guard): do not fix it silently and do not panic — this is the scripted Medium-risk moment. Point at the exact spec line or rule file and have Claude self-correct live: *"You returned the EF entity from the endpoint. Read .claude/rules/api-endpoints.md and section 6 of the spec, then fix it."* Tell the room: "This failure-and-recovery is the actual lesson — the spec and rules give you something objective to enforce."
- **Claude ignores the 4-file gate:** interrupt (Esc), restate: "You changed 6 files without pausing. Roll that into batches of 4 with approval between." Narrate that gates are instructions you enforce, not magic.
- **Build hook failure loop** (migration or wiring error cascades): let Claude attempt one self-correction off the hook output; if it thrashes, scope down — "skip the migration for now, finish Core + Api against an in-memory repository" — and note that Lab 4 gives attendees the full hour you don't have.
- **Clock overruns** (you started at 4:05; Lab 4 must start ~4:22): pre-planned cuts in order — drop step 5 (say it, don't run it), then take the first justification in step 4 without a revision round. Never cut step 2; the spec review is the cheapest, highest-value teaching in the demo.
- **Spec review finds nothing** (rare): seed it — "What happens on the first PUT when no progress record exists? Is that in the spec?" — and let Claude take it from there.
- **Total stall:** fall back to plan **B** from the setup: verify the existing reference implementation against the spec instead of building live. The verification checklist lands the same points.

## Teaching callouts

- "Review the **spec** — cheap — before the **code** — expensive. A page of text now beats a rework cycle later."
- "The spec makes the hundred decisions a vague prompt leaves to chance **precise** — 'add reading progress' vs. a forward-only state machine with terminal `Completed`."
- "'Pause every N changes' is how you refactor and build at scale without a runaway you can't review."
- "Push back at least once and make Claude justify or revise — supervising, not rubber-stamping. The spec gives you something objective to point at."
- "Nobody reminded Claude about DTOs or async — CLAUDE.md and the rules did. Steering compounds: everything from Section 2 just worked."
- "The demo closed the day's arc: the issue from the MCP demo became a spec, the spec became code, the code verified against the spec's own acceptance criteria. Lab 4 is you doing it."
