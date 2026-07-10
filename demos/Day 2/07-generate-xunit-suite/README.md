# Day 2 Demo 7: Generate xUnit Suite for AgentService

| | |
|---|---|
| **Slide deck** | `decks/Day 2/Day2-Section5-AI-Testing-CICD.pptx` (slide 9, "Live Demo," part 1) |
| **Scheduled** | 3:42 PM |
| **Duration** | ~8 min |
| **Risk** | Low |
| **Requirements** | Claude Code installed and authenticated; the BookTracker solution builds; no API key needed beyond your Claude Code auth |
| **GitHub issue** | <https://github.com/cwoodruff/dotnet-claude-workshop/issues/22> |

## What this demo teaches

- **Describe *what*, not *how*.** You tell Claude which behaviors need coverage — happy path, multi-tool run, tool error, cancellation — and it reads the implementation and writes the cases.
- Claude reuses **existing test patterns**: it picks up the conventions already in `BookTracker.Tests` (the `AgentTestHarness` scripted-LLM doubles, `MethodName_Scenario_ExpectedResult` naming, NSubstitute) instead of inventing its own style.
- The **"adversarial test engineer"** re-prompt reliably surfaces edge cases the first pass missed — iteration cap, unknown tool names, empty tool results, missing `CancellationToken` propagation.
- The loop ends the same way all testing ends: `dotnet test` to green.

This demo runs Claude Code against the existing `src/BookTracker` solution — there is no separate demo project. (The demo plan references checkpoint folders like `src/c8/BookTracker`; this repo ships a single `src/BookTracker`, so run everything from there.)

The class under test is `BookTracker.Api/Services/AgentService.cs` — the provider-neutral tool-calling loop. Its seams are `IAgentLlm` / `IAgentConversation` (the LLM) and `BookTrackerTools` (tool dispatch), which is exactly what makes it unit-testable without the SDK.

## Prerequisites / setup

- [ ] `cd "src/BookTracker"` and confirm a clean baseline: `dotnet build BookTracker.sln` succeeds.
- [ ] Claude Code authenticated: `claude "say hi"` responds.
- [ ] **Stage the generation honestly.** The reference solution already ships `BookTracker.Tests/Agent/AgentServiceTests.cs` and `AgentEdgeCaseTests.cs`. For a genuine live generation, temporarily move them out of the project (keep `AgentTestHarness.cs` — it's the pattern you want Claude to find and reuse):

  ```bash
  mkdir -p /tmp/demo7-stash
  mv BookTracker.Tests/Agent/AgentServiceTests.cs BookTracker.Tests/Agent/AgentEdgeCaseTests.cs /tmp/demo7-stash/
  dotnet test BookTracker.sln   # confirm still green without them
  ```

  (Restore with `git checkout -- BookTracker.Tests/Agent/` after the demo.)
- [ ] Terminal font large; you will be showing generated C# on the projector.
- [ ] Night before: run the whole script end-to-end once so you know roughly what Claude generates and how long it takes.

## Step-by-step script

1. **Frame it (30 s).** "We've built the agent loop; now we make AI write the tests. The rule: describe *what* needs testing, not *how*. Claude reads `AgentService` and the existing test patterns and does the mechanical work."

2. **Show the target briefly.** Open `BookTracker.Api/Services/AgentService.cs` — point at `RunAsync`, the `MaxIterations` cap, the per-tool `try/catch`, and the `OperationCanceledException` rethrow. "Four behaviors worth covering. Watch the prompt name the *behaviors*, not the assertions."

3. **Start Claude Code from the solution folder.**

   ```bash
   cd "src/BookTracker"
   claude
   ```

4. **Run the generation prompt.** Paste exactly:

   ```text
   Generate xUnit tests for AgentService using the patterns in BookTracker.Tests and NSubstitute.
   Cover: happy path, a multi-tool run, a tool that errors, and a cancelled CancellationToken.
   ```

   **Expected:** Claude reads `AgentService.cs`, discovers `BookTracker.Tests/Agent/AgentTestHarness.cs` (the `ScriptedLlm` / `ScriptedConversation` doubles), and writes a test class in `BookTracker.Tests/Agent/` with tests along the lines of:

   - `RunAsync_HappyPath_RunsOneToolThenReturnsFinalText`
   - `RunAsync_MultiTool_RunsTwoToolsInOrderBeforeFinal`
   - `RunAsync_ToolThrows_RecordsErrorOutcomeAndDoesNotCrash`
   - `RunAsync_CancelledToken_PropagatesCancellation`

   Narrate while it works: "Notice it found the existing harness and is reusing it rather than mocking `IAgentLlm` from scratch — that's why we said *using the patterns in BookTracker.Tests*."

5. **Run the tests.**

   ```bash
   dotnet test BookTracker.sln
   ```

   **Expected output:** green — `Passed!` with the new tests included. If anything fails to compile, see Recovery; the fix is part of the show.

6. **Run the adversarial prompt.** Back in Claude Code, paste exactly:

   ```text
   You are an adversarial test engineer. Find the untested edge cases in AgentService that could
   break in production, then write failing tests that expose them.
   ```

   **Expected:** Claude proposes cases the first prompt missed — typically the **max-iteration cap** (an LLM that never stops asking for tools), an **unknown tool name**, an **empty/whitespace tool result**, token accounting across turns, or cancellation mid-loop between tool rounds — and writes them (e.g. an `AgentEdgeCaseTests.cs`). Say: "This is the pass that finds what you'd miss. A persona flip, and it hunts instead of confirms."

7. **Finish with the full suite.**

   ```bash
   dotnet test BookTracker.sln
   ```

   **Expected output:** all tests green. If an "exposing" test legitimately fails, even better — narrate that a failing test that exposes a real gap is the *point*, then let Claude fix the production code or adjust the test, and re-run to green.

8. **Land it (30 s).** "Ten minutes of typing you didn't do. In Lab 5 Part A you'll run these exact two prompts yourself."

## Recovery

- **A generated test fails to compile:** don't hand-edit. Paste the compiler error straight back into Claude Code ("`dotnet test` output: `<error>` — fix the test") and let it repair its own code. This is itself a teaching moment: the loop is generate → build → feed errors back.
- **Claude writes mocks instead of using the harness:** re-prompt with "Use the ScriptedLlm/ScriptedConversation doubles in BookTracker.Tests/Agent/AgentTestHarness.cs instead of mocking IAgentLlm."
- **Generation is slow or wanders:** you have the reference suite stashed in `/tmp/demo7-stash` (and in git). Restore it, walk through the four tests it contains, and narrate what the prompts would have produced.

## Teaching callouts

- "Describe *what* needs testing, not *how* — Claude reads the implementation and writes the cases."
- "Point it at existing patterns; the prompt said `using the patterns in BookTracker.Tests` and it reused the harness instead of inventing a style."
- "The adversarial pass finds what you'd miss — same code, different persona, different questions."
- "AI writes the tests; `dotnet test` is still the judge. Green is the only exit criterion."
- "Compile error? Paste it back. The model fixes its own output faster than you can."
