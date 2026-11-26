# Day 1 Demo 9b: Debugging from a Stack Trace

| | |
|---|---|
| **Slide deck** | `decks/Day 1/Section4-AI-in-the-SDLC.pptx` (slide 7, "Debugging") |
| **Scheduled** | 3:51 PM |
| **Duration** | 6 min |
| **Risk** | Low (local files plus Claude Code's model connection) |
| **Internet** | Yes, for Claude Code's model connection (nothing else) |
| **GitHub issue** | <https://github.com/cwoodruff/dotnet-claude-workshop/issues/14> |

## What this demo teaches

The debugging loop, run honestly:

1. **Paste the failing output** — a real xUnit failure with a stack trace, not a description of it.
2. **Read before fixing** — Claude reads the relevant source and test files first (grounding), so it fixes the real bug, not an imagined one.
3. **Root cause, not symptom** — "the test fails" is a symptom; "the state machine now allows a transition out of `Completed`, violating the spec's terminal-status rule" is a cause.
4. **Reproduce before fixing** — a failing test that captures the bug means the fix is verified *and* regression-protected.
5. **Fix the source, not the test** — a green suite you got by weakening a test is worse than a red one.

Bonus callout: the `PostToolUse` build/test hook from Section 2 surfaces failures into Claude's context automatically — self-correction often starts before you ask.

## Prerequisites / setup

- [ ] Terminal running `claude` in `src/c4/BookTracker`; `.claude/` config (including the PostToolUse build hook) loaded.
- [ ] **Stage the deliberate bug** — the exact before/after edit is in [`artifacts/introduce-bug.md`](artifacts/introduce-bug.md). It adds a plausible-looking "allow re-reading" case to the status state machine in `src/c4/BookTracker/BookTracker.Core/Domain/ReadingProgressRules.cs`, which breaks the spec rule that `Completed` is terminal. Apply it **before the session** (it's on the night-before checklist), on a scratch branch:

  ```bash
  cd src/c4/BookTracker
  git switch -c demo/9b-staged-bug
  # apply the edit from artifacts/introduce-bug.md, then:
  dotnet build BookTracker.sln        # must still compile — the bug is logical, not syntactic
  dotnet test BookTracker.sln --filter "FullyQualifiedName~ReadingProgressTests"
  # expect exactly 1 failure: Apply_CompletedToReading_Rejected
  ```

- [ ] **Capture the failing output** from that test run into a scratch buffer you can paste from. A canonical capture is in [`artifacts/failing-output.txt`](artifacts/failing-output.txt) — use it verbatim if you'd rather not re-run live (paths in it are illustrative; your live capture is better).
- [ ] Verify the diff is small (`git diff` shows one file, ~5 added lines) so the "find it" step is fair at demo speed.
- [ ] Know the fix cold: revert the added case so `Completed → anything` falls through to the `default:` rejection.

> **Note on the demo timeline:** `src/c4/BookTracker` is the state after Lab 4, so it already has the Reading Progress feature that Demo 10 builds. For this 6-minute demo that's fine: you're debugging the *rules* class and its tests, which exist there. If you run Section 4 demos strictly in order, run Demo 9b *after* Demo 10's implementation instead, exactly as Lab 4 Part C does.

## Step-by-step script

1. **Frame it (30 s).** "Last discipline of the day: debugging. Someone — fine, me — shipped a bug. All I have is a red test run. Watch the order of operations, because the order *is* the technique."

2. **Show the failure (30 s).** Run it live (or show the staged capture):

   ```bash
   dotnet test BookTracker.sln --filter "FullyQualifiedName~ReadingProgressTests" --nologo
   ```

   **Expected output:** 7 pass, 1 fail — `Apply_CompletedToReading_Rejected`, `Assert.False() Failure: Expected: False, Actual: True`, stack trace pointing at `ReadingProgressTests.cs:line 94`. (Full text: `artifacts/failing-output.txt`.)

3. **Paste it to Claude with the loop spelled out (30 s).** In `claude`:

   ```text
   Here's the failing output: [paste the full test output + stack trace]. Read the relevant
   source and tests, find the root cause, and reproduce it with a failing test before fixing.
   ```

4. **Narrate the reads (1–2 min).** **Expected:** Claude reads `BookTracker.Tests/ReadingProgressTests.cs` (the failing assertion at line 94) and `BookTracker.Core/Domain/ReadingProgressRules.cs`, then states the root cause — something like: "a `(Completed, Reading)` case was added to the transition switch, so `Apply` now accepts a transition out of `Completed`; the spec (`specs/reading-progress.md` §4/§6) says `Completed` is terminal." Point at the screen: "It didn't guess from the assertion message — it read the state machine and the spec. That's grounding."

5. **Demand the reproduction (1 min).** If Claude jumps straight to the fix, stop it: "Reproduce first." **Expected:** Claude either points out that `Apply_CompletedToReading_Rejected` *is* the reproduction, or (better) adds a sharper regression test — e.g. `Completed → WantToRead` is also now accepted by the buggy case? It isn't (only `Reading` was opened up), so a good extra test is `Apply_CompletedToReading_DoesNotClearFinishedOn`. Either is fine; the visible lesson is that a failing test exists *before* the source changes.

6. **Fix the source, verify (1–2 min).** **Expected:** Claude removes the offending `case (ReadingStatus.Completed, ReadingStatus.Reading)` block so the transition falls back to the `default:` rejection, re-runs the tests (or the PostToolUse hook does), and the suite goes green — including the regression test. Say: "Notice what it did *not* do: it did not edit the test to expect `True`. Fix the source, not the test."

7. **Land it (30 s).** "Paste the failure → read → root cause → reproduce → fix source → verify. You'll run this exact loop yourself in Lab 4 Part C — you'll even introduce the bug yourselves. And the hook means half the time Claude sees the red build before you paste anything."

## Recovery

- **Claude Code loses its connection:** nothing here runs without it. Switch to your hotspot; if that fails too, walk the steps from the slides and the expected-output notes below.
- **The fix comes too fast** (Claude one-shots root cause + fix in a single reply): don't fight it — slow it down and surface the discipline explicitly. "It skipped a step I care about — Claude, before applying that, show me the failing test that reproduces the bug, run it, and *then* fix." The reproduce-before-fix step is the actual lesson.
- **Claude fixes the test instead of the source** (rare, but the perfect teachable failure): stop, highlight it — "that's the anti-pattern from the slide, live" — and reply: "The test encodes the spec (`specs/reading-progress.md` says Completed is terminal). Fix the source." Let it self-correct on camera.
- **Claude can't find the bug** (didn't read the rules file): steer with grounding, not the answer: "Read BookTracker.Core/Domain/ReadingProgressRules.cs and compare the transition switch against section 4 of specs/reading-progress.md."
- **You forgot to stage the bug** (tests are green): apply the edit from `artifacts/introduce-bug.md` live in 20 seconds — narrate it as "here's the bug a well-meaning teammate shipped" — rerun the filtered tests, and proceed.
- **Clock overruns:** cut step 5 (the extra regression test) and keep steps 4 and 6 — root cause + source fix + green run is the minimum viable lesson.
- **After the demo:** `git switch -` (or restore the file) so Demo 10 starts from a clean tree.

## Teaching callouts

- "Fix the **source**, not the test — unless the test itself encodes the bug. A green suite you got by weakening a test is worse than a red one."
- "**Reproduce before fixing** — the failing test is both the proof of the bug and the regression guard for the fix."
- "Root cause, not symptom: 'the test fails' vs. 'the state machine allows a transition out of a terminal state.'"
- "Make Claude read the source and tests *first* — grounding is what separates diagnosis from guessing."
- "The `PostToolUse` build/test hook pipes failures straight into Claude's context — debugging that starts before you ask."
