# Day 1 Demo 3: Live Codebase Exploration

| | |
|---|---|
| **Slide deck** | `decks/Day 1/Section1-Foundations-Claude-Code-CLI.pptx` (slide 12, "Live Demo") |
| **Scheduled** | 10:00 AM |
| **Duration** | 20 min |
| **Risk** | Low |
| **Internet** | No |
| **GitHub issue** | <https://github.com/cwoodruff/dotnet-claude-workshop/issues/5> |

## What this demo teaches

- **Grounding beats recall** — Claude reads the real files before it answers; if it can't see it, it may invent it.
- The habit Lab 1 drills: **verify Claude, don't trust it** — you will spot at least one inaccuracy live, on purpose.
- How to make citation a requirement ("cite the files you used") so answers are checkable.
- This is a direct preview of Lab 1 Part C's five exploration prompts.

## Prerequisites / setup

- [ ] Same terminal, same working directory as Demo 2: `src/BookTracker` (quote paths — the repo path may contain spaces).
- [ ] `claude` session open (continue the Demo 2 session, or start fresh — a fresh start bonus-demos CLAUDE.md loading at startup).
- [ ] The trimmed/restored `CLAUDE.md` from Demo 2 is in place.
- [ ] You have personally skimmed these files the night before, so you can verify claims instantly on screen:
  - `BookTracker.Api/Program.cs` (DI wiring, endpoint mapping: `MapBookEndpoints`, `MapAuthorEndpoints`, `MapReviewsEndpoints`, `MapReadingProgressEndpoints`, plus the Day 2 chat/agent/recommend groups)
  - `BookTracker.Api/Endpoints/BookEndpoints.cs`
  - `BookTracker.Core/Entities/` (`Book`, `Author`, `Review`, `ReadingProgress`, `ReadingStatus`)
  - `BookTracker.Core/Services/BookService.cs` and `BookTracker.Core/Interfaces/IBookRepository.cs`
  - `BookTracker.Data/BookTrackerDbContext.cs`, `BookTracker.Data/Repositories/BookRepository.cs`
  - `BookTracker.Tests/` (`BookServiceTests.cs`, `ReviewServiceTests.cs`, `ReadingProgressTests.cs`)
- [ ] Know your ace: `BookTracker.Data/Repositories/BookRepository.cs`, `SearchByTitleAsync` — it builds SQL by string interpolation into `FromSqlRaw` (`$"SELECT * FROM Books WHERE Title LIKE '%{term}%'"`). It's the staged SQL-injection teaching gap and your guaranteed "verify this claim" anchor if the quality/gap prompts surface it — or your counter-example if Claude's assessment is too rosy.
- [ ] A second terminal or editor pane ready to open cited files side-by-side for verification.

## Step-by-step script

1. **Frame it (1 min).** Say: "Watch *how* it answers, not just *what* it answers — does it read files before it talks? That's grounding. And I'm going to catch at least one inaccuracy live, because the skill you practice in Lab 1 isn't trusting Claude — it's **verifying** it."

2. **Architecture overview prompt.** Paste exactly (this is Lab 1 Part C prompt #1 — tell them so):

   ```text
   Read the BookTracker solution. Give me an architecture overview: the projects, each one's
   responsibility, and the dependency direction between them. Cite the files you used.
   ```

   **While it runs:** narrate the tool calls out loud as they scroll — "it's reading `BookTracker.sln`… now the csproj files… now `Program.cs`… That's grounding happening in front of you." If Claude is fast, deliberately slow down and read the file-read list aloud.

   **Expected output:** the projects and dependency direction match the code — `BookTracker.Core` (entities, DTOs, service interfaces; depends on nothing), `BookTracker.Data` (`BookTrackerDbContext`, repositories, migrations; depends on Core), `BookTracker.Api` (Minimal API endpoints under `Endpoints/`, wired in `Program.cs`; depends on Core + Data), `BookTracker.Tests` (references all). It should also notice the extra projects in this solution (`BookTracker.VectorStore`, `BookTracker.Mcp`) — if it does, say "those are Day 2's — park them."

3. **Verify one citation on screen (2 min).** Pick one cited file — e.g. it claims endpoints are wired via `MapBookEndpoints` in `Program.cs` — open `BookTracker.Api/Program.cs` in your second pane and show the actual `app.MapBookEndpoints();` line. Say: "Sixty seconds of checking. That's the Lab 1 rhythm: prompt, then verify."

4. **Gap-analysis prompt.** Paste exactly (Lab 1 Part C prompt #3):

   ```text
   What's missing for production readiness? Check specifically for authentication, logging,
   health checks, caching, and observability. For each, tell me whether it exists, where, and
   what's absent.
   ```

   **Expected output:** a mostly accurate list — typically: no authentication/authorization on the endpoints, no health-check endpoint mapped in `Program.cs`, logging exists only as default host logging, caching limited to the in-memory `IDistributedCache` used for chat history, no metrics/tracing. Quality varies run to run — that variance is your material.

5. **Spot one inaccuracy live (the money moment, 4–5 min).** Scan the answer for a claim that is soft, generalized, or wrong, and verify it on screen. Reliable places to catch one:

   - **A "missing" that actually exists** (or vice versa): e.g. Claude says "no caching anywhere" — open `Program.cs` and point at `AddDistributedMemoryCache()`; or it says "structured logging is configured" — show that nothing beyond defaults is.
   - **A generalized security claim:** if it says "data access uses parameterized EF Core queries throughout," open `BookTracker.Data/Repositories/BookRepository.cs` at `SearchByTitleAsync` and show the interpolated `FromSqlRaw` — a real injection vector it just glossed over.
   - **A hallucinated file or line number:** open the cited path and show it doesn't exist or says something different.

   If the answer happens to be flawless, flip the move: **you** assert something plausible-but-wrong ("it told me earlier there's a health check — let's verify") and disprove it, or ask a follow-up it can't have grounded yet:

   ```text
   You said there's no input validation. Which specific service methods accept unvalidated
   input? Cite exact files and line numbers.
   ```

   …then check one cited line on screen. Either way, land it out loud: "There it is. Plausible, confident, and not quite right. This is why Lab 1's checkpoint literally says *you spotted at least one inaccuracy*."

6. **Close the loop on grounding (1 min).** Say: "An LLM predicts plausible text. Show it the file and the answer is grounded; don't, and it may invent. Rule of the day: **if Claude can't see it, it may invent it — make it read before it writes.**"

7. **Hand off to Lab 1 (1 min).** "Part C of Lab 1 is five prompts — you just watched #1 and #3. The other three: a quality assessment, an EF Core data-layer walkthrough, and a test-coverage analysis. Spend ~60 seconds verifying each answer against the real code. Then commit your CLAUDE.md." Point them at `docs/Day 1/04-lab-1-explore.md`.

## Recovery

- **Nothing should fail — this demo is fully offline** against local files with an already-authenticated session. The risks are pacing and a too-good answer.
- **Claude is too fast to narrate:** slow down and read its file reads aloud after the fact — scroll back through the tool calls and reconstruct the grounding trail ("look at what it chose to read, and in what order").
- **The answer is perfect and you can't find an inaccuracy:** use the flip from step 5 — demand exact file/line citations on a specific claim and verify one, or assert a plausible falsehood yourself and disprove it. The lesson is the *verification habit*, not Claude being wrong.
- **The answer is badly wrong or goes sideways:** even better — verify, correct it in-session ("Actually, read BookTracker.Data/Repositories/BookRepository.cs and reconsider"), and show the grounded second answer improving. Failure-and-correction is the strongest version of this demo.
- **Context feels polluted from Demo 2:** `/clear` and re-run the architecture prompt — costs ~1 minute and demos context hygiene as a bonus.
- **Running long:** cut step 4's full gap analysis and fold the inaccuracy hunt into the architecture answer instead; the non-negotiable beats are one verified citation and one caught inaccuracy.

## Teaching callouts

- "If Claude can't see it, it may invent it — grounding beats recall."
- "Watch *how* it answers: files read before words written. That's the tell of a grounded answer."
- "'Cite the files you used' turns an answer into something you can check in sixty seconds."
- "The skill isn't trusting Claude — it's verifying it. You'll spot your own inaccuracy in Lab 1."
