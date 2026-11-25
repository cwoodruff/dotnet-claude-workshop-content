# Day 2 Demo 1: First working API call

| | |
|---|---|
| **Deck** | `decks/Day 2/Day2-Section1-CSharp-SDK-Fundamentals.pptx` (slide 9, "Live Demo") |
| **Duration** | ~8 min |
| **Risk** | Low |
| **Internet** | Yes (Anthropic API) |
| **Issue** | https://github.com/cwoodruff/dotnet-claude-workshop/issues/16 |

## What this demo teaches

- Everything goes through `client.Messages.Create` — pick a model, set a `MaxTokens` ceiling, pass the messages.
- Model choice: `Model.ClaudeOpus5_5` is the app default (Sonnet 5.5 for mechanical work).
- Reading `response.Usage.InputTokens` / `OutputTokens` — `Usage` is how you see cost.
- The API is **stateless**: this one call carries everything the model sees.
- The API key lives in **user-secrets**, never in code or committed config.

This is the "Day 2 opens with code, not slides" moment — a real Claude response inside the first 20 minutes.

## Prerequisites / setup

- [ ] .NET 10 SDK installed (`dotnet --version` shows 10.x).
- [ ] An Anthropic API key with credit (Day 2 requires an API key — this is separate from the Day 1 Claude plan).
- [ ] Restore/build once the night before so nothing downloads on stage:

  ```bash
  cd "demos/Day 2/01-first-api-call/src/Demo1.FirstApiCall"
  dotnet build
  ```

- [ ] Set the key in user-secrets (from the project folder above):

  ```bash
  dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-..."
  ```

  Fallback: the app also reads the `ANTHROPIC_API_KEY` environment variable, but lead with user-secrets — that's the pattern Lab 1 uses.
- [ ] **Backup:** capture a screenshot of one successful run (response text + token counts) in case the network or API is down.
- [ ] Terminal font large enough to read from the back of the room.

## Step-by-step script

1. **Set the stage (30 s).** Narrate: *"Day 1 you drove Claude from the terminal. Today you call it from C#. Before any architecture — one working call."*

2. **Show the project (1 min).** Open [`src/Demo1.FirstApiCall/Demo1.FirstApiCall.csproj`](src/Demo1.FirstApiCall/Demo1.FirstApiCall.csproj). Point at the single package:

   ```xml
   <PackageReference Include="Anthropic" Version="13.14.0" />
   ```

   Narrate: *"The official `Anthropic` package, v13.x — the one Anthropic maintains and the one BookTracker uses. Watch the name: the old `tryAGI.Anthropic` v3.x is a different lineage."*

3. **Walk the code (3 min).** Open [`src/Demo1.FirstApiCall/Program.cs`](src/Demo1.FirstApiCall/Program.cs) and walk it top to bottom:
   - The key comes from **user-secrets** via `ConfigurationBuilder` — *"no key in code, no key in appsettings, nothing to accidentally commit."*
   - `new AnthropicClient { ApiKey = apiKey }` — *"in ASP.NET Core this is registered as a Singleton because it holds an `HttpClient`; Scoped or Transient means socket exhaustion."*
   - The `Messages.Create` call — model, `MaxTokens = 4096`, one user message. Point out the `CancellationToken` flowing in.
   - The `OfType<TextBlock>()` filter — *"`ContentBlock` is a union; today we only want the text blocks."*

4. **Run it (2 min).**

   ```bash
   cd "demos/Day 2/01-first-api-call/src/Demo1.FirstApiCall"
   dotnet run
   ```

   **Expected output** (reply text varies):

   ```
   User: Recommend a book like Dune.

   If you loved Dune, try "Hyperion" by Dan Simmons — ... (a real recommendation)

   Usage — input tokens: 14, output tokens: ~150-400
   ```

5. **Land the callouts (1.5 min).** Point at the `Usage` line: *"That's your bill. Input tokens times input price plus output tokens times output price — `Usage` is how you see cost on every single call."* Then: *"And notice what we did NOT send: no session, no state. The API is stateless — remember that, because Demo 2 is about what that forces you to do."*

## Recovery

- **Auth fails (401 / invalid key):** Confirm the key location — it's **user-secrets, not the environment variable** (though the env var works as a fallback here). Re-run from the project folder:

  ```bash
  dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-..."
  dotnet run
  ```

  If it still fails, check for a pasted trailing space or a revoked key; `dotnet user-secrets list` shows what's actually stored.
- **"No API key found" message:** you ran from the wrong directory or the secret was set against a different project — user-secrets are keyed by the `UserSecretsId` in the csproj (`day2-demo1-first-api-call`). `cd` into `src/Demo1.FirstApiCall` and set it again.
- **429 / 529:** transient — re-run once. Narrate that the SDK already retries these twice by default and Polly comes up in Block 1C.
- **Network/API fully down:** show the backup screenshot of a successful run, walk the code anyway, and promise a live re-run after the break. The teaching is the shape of the call, not the specific reply.

## Teaching callouts

- **The API is stateless.** No conversation lives on Anthropic's side; every call is self-contained. This is the setup for Demo 2's history management.
- **`Usage` is how you see cost.** Every response carries exact input/output token counts — build cost visibility in from call one.
- **Sonnet is the app default.** Haiku when cheap-and-fast wins, Opus when the problem is genuinely hard; the model is one enum value away.
- **Secrets hygiene from minute one.** User-secrets (or env vars) — the key never appears in code, config, or git. Same rule the whole workshop enforces.
- **Always set `MaxTokens`.** It's a required ceiling — it caps your worst-case bill for the reply.
