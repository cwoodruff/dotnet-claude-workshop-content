# Day 2 Demo 4: Tool-Calling Agent Loop (2+ Tools)

| | |
|---|---|
| **Deck** | `decks/Day 2/Day2-Section2-Streaming-Tools-Thinking.pptx` (slide 8, "Make It Real") |
| **Section** | Day 2 · Section 2 — Streaming, Tool Calling & Extended Thinking (Block 2B) |
| **Duration** | ~10 min |
| **Risk** | Medium (live API; the model can answer without calling tools) |
| **Internet** | Yes — Anthropic API |
| **Issue** | <https://github.com/cwoodruff/dotnet-claude-workshop/issues/19> |

## What this demo teaches

- **The agent acts on real data, not just text.** One prompt — *"Find Dune and mark it as
  completed"* — makes Claude call two C# functions in sequence and mutate application state.
- **Tool definition = name + description + JSON input schema**, and the *description is the
  contract*: written like a skill description, it says **when** to call the tool, not just what
  it does.
- **The agent loop** — the reusable pattern for multi-step workflows: Claude requests a tool →
  you execute the C# method → you return a `tool_result` → Claude continues, possibly calling
  more tools, until `StopReason` is `EndTurn`. Implemented here as a visible **manual loop** so
  every hop shows in the logs. (Mention the SDK's `BetaToolRunner`, which drives this same loop
  for you — Lab 2 lets attendees choose either; the manual loop is the one you can log.)
- **Guardrails:** a max-iteration cap, and per-tool `try/catch` that returns an `is_error`
  `tool_result` instead of crashing — the model gets to read the error and recover.

The demo app is a self-contained console app with an **in-memory** 5-book catalog (no database,
no EF Core) so the loop mechanics are the only thing on screen. Lab 2 Part B wires the same two
tools to BookTracker's real services.

## Prerequisites / setup

- [ ] .NET 10 SDK installed (`dotnet --version` → 10.x).
- [ ] Anthropic API key exported in the terminal you'll run from:

  ```bash
  export ANTHROPIC_API_KEY=sk-ant-...
  ```

  (Console app → env var; never hardcoded, never on screen. Set it *before* the demo starts.)
- [ ] Solution builds: `dotnet build` from `./src/` — 0 warnings, 0 errors.
- [ ] Terminal font large enough that log lines are readable from the back row.
- [ ] Backup: a screenshot of a successful run showing the two tool-call log lines.

## Step-by-step script

1. **Set the problem (30 s).** Say: *"So far Claude only talks. Now we let it act — on our data,
   through our C# methods, on our terms. The unit of that is a tool; the engine is a loop."*

2. **Show the tools** — open `./src/Demo4.AgentLoop/AgentTools.cs`:
   - `find_book`: point at the description — *"Use this whenever the user names a specific book,
     to look up the book (and its id) before acting on it."* Say: *"That sentence is doing the
     engineering. It tells the model when to call, and that this is where ids come from."*
   - `update_reading_progress`: note *"Requires the numeric book id — call find_book first if you
     only have a title"* — this line is what forces the two-step plan.
   - The `InputSchema` is plain JSON Schema built with `JsonSerializer.SerializeToElement`; the
     `ExecuteAsync` switch at the bottom is the *only* code that can touch the data — Claude never
     gets a database handle, it gets a menu.

3. **Show the data** — flash `./src/Demo4.AgentLoop/BookStore.cs`: five hardcoded books, Dune is
   id 1 with status `WantToRead`. In-memory on purpose — *"in Lab 2 these same two tools call
   BookTracker's real services against SQLite."*

4. **Show the loop** — open `./src/Demo4.AgentLoop/Program.cs` and walk the shape, not every line:
   - `for (iteration = 1..MaxIterations)` — **the cap comes first**; never let a model drive an
     unbounded loop.
   - `Messages.Create(...)` with `Tools = AgentTools.Definitions...` — same call as Demo 1, plus
     a tool list.
   - Collect `tool_use` blocks → if none, the model ended its turn (`StopReason` = `EndTurn`):
     print the final answer, done.
   - Otherwise: echo the assistant turn into history, execute each tool, append matching
     `ToolResultBlockParam`s (note `IsError` in the catch block) as the next user turn, go around
     again.

5. **Run it:**

   ```bash
   cd "demos/Day 2/04-tool-calling-agent-loop/src/Demo4.AgentLoop"
   dotnet run
   ```

   The default prompt is exactly the demo prompt — *"Find Dune and mark it as completed."*
   (Pass a different one as an argument: `dotnet run -- "How far am I into Project Hail Mary?"`.)

6. **Expected output** — narrate the log lines as they land:

   ```
   10:52:01 info: AgentLoop[0] User prompt: Find Dune and mark it as completed.
   10:52:03 info: AgentLoop[0] Iteration 1: stop reason "tool_use"
   10:52:03 info: AgentLoop[0] Tool call find_book input={"query":"Dune"} error=False result=[{"Id":1,"Title":"Dune",...,"Status":"WantToRead"}]
   10:52:05 info: AgentLoop[0] Iteration 2: stop reason "tool_use"
   10:52:05 info: AgentLoop[0] Tool call update_reading_progress input={"bookId":1,"status":"Completed"} error=False result={"Id":1,...,"CurrentPage":412,"Status":"Completed"}
   10:52:07 info: AgentLoop[0] Iteration 3: stop reason "end_turn"

   === Final answer ===
   Done! I found "Dune" by Frank Herbert and marked it as completed...

   10:52:07 info: AgentLoop[0] Done in 3 iteration(s). Tokens: ... in / ... out
   ```

   The teaching beat: **two tool calls in the logs before any final text**. Say: *"Nobody told it
   to call find_book first — the tool descriptions did. It planned: look up, get the id, write,
   confirm."*

7. **Optional flourish (if time):** show the error path live —
   `dotnet run -- "Mark book 99 as completed"`. The log shows `error=True` with
   *"No book with id 99. Use find_book to get a valid id."* and Claude recovers (searches or
   explains) instead of the app crashing. That's the `is_error` guardrail earning its keep.

8. **Bridge to Lab 2 Part B:** attendees define the same 2+ tools against real BookTracker
   services and run the loop (or `BetaToolRunner`) until `EndTurn`, with the iteration cap and
   `is_error` handling as checkpoint items.

## Recovery

- **The model answers without calling any tools** (one iteration, straight to `EndTurn`,
  possibly inventing an answer): the fix is the *description*, and fixing it live is a better
  demo than a clean run.
  1. Open `AgentTools.cs`, weaken-then-strengthen: show that a vague description ("Searches
     books") gives the model no trigger; the shipped one says **when** to call ("Use this
     whenever the user names a specific book...").
  2. Also check the system prompt in `Program.cs` — it explicitly says to use the tools and to
     look up before updating. If you edited it, restore it.
  3. Re-run. If it still won't bite (rare), show the **backup screenshot** of the two tool-call
     log lines and walk the sequence from it.
- **It calls `update_reading_progress` with a guessed id** instead of calling `find_book` first:
  point at the log — the `tool_result` will be the `is_error` "No book with id N" message and the
  model self-corrects on the next iteration. That *is* the demo: errors as data.
- **Auth failure:** the app exits immediately with "Set the ANTHROPIC_API_KEY environment
  variable" — export it in *this* terminal (env vars don't cross terminals) and re-run.
- **Hits the 5-iteration cap** (model loops on a failing tool): the final log line says so
  explicitly — great moment to explain *why* the cap exists; then re-run, it's non-deterministic.
- **No network:** fall back to the screenshot + the code walk; steps 2–4 carry most of the value.

## Teaching callouts

- **The description is the contract.** You don't program the plan; you write tool descriptions
  precise enough that the model derives the plan. Treat them like skill descriptions: say *when*.
- **The loop is the reusable pattern.** Every multi-step AI workflow — agents, MCP hosts, Claude
  Code itself — is this same shape: model → tool call → tool result → model → ... → `EndTurn`.
- **Guard the loop.** Max-iteration cap, `is_error` tool results instead of exceptions, and
  cancellation that propagates (`OperationCanceledException` is rethrown, not swallowed).
- **Claude never touches the data.** It requests; your `ExecuteAsync` switch decides what runs.
  That's the governance story that returns in Section 4 — MCP is this exact tool surface exposed
  over a protocol so *any* host can call it.
- **Cost is visible:** the loop sums `Usage` across iterations — three model calls for one user
  question. Multi-step capability isn't free; that's why you cap and log.
- **Same pattern, real services:** BookTracker's production version of this demo lives in
  `src/BookTracker/BookTracker.Api/Tools/BookTrackerTools.cs` and
  `Services/AgentService.cs` — identical loop, wired to EF Core through Core interfaces.
