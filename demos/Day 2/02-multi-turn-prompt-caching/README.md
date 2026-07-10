# Day 2 Demo 2: Multi-turn + prompt-cache token counts

| | |
|---|---|
| **Deck** | `decks/Day 2/Day2-Section1-CSharp-SDK-Fundamentals.pptx` (slide 9, "Live Demo") |
| **Duration** | ~8 min |
| **Risk** | Low |
| **Internet** | Yes (Anthropic API) |
| **Issue** | https://github.com/cwoodruff/dotnet-claude-workshop/issues/17 |

## What this demo teaches

- The **system prompt is your assistant's constructor** — it scopes the assistant to BookTracker once, up front.
- Multi-turn = **you** keep the history: stored turns map to the `Messages` list and the new user turn is appended on every call (the API is stateless).
- **Prompt caching** with `CacheControlEphemeral`: call 1 *writes* the cache (`Usage.CacheCreationInputTokens` non-zero), call 2 *reads* it (`Usage.CacheReadInputTokens` non-zero) — and the cached input bills at **~10%** of normal.
- Caching is a **byte-exact prefix match**: any change upstream (the demo's "silent invalidator" is a timestamp in the system prompt) invalidates everything below it.
- The **model-dependent minimum prefix**: ~2,048 tokens on Sonnet 4.6 (~4,096 on Haiku/Opus). Below that the cache *silently* doesn't fire.

## Prerequisites / setup

- [ ] Demo 1 worked (same key, same package, same machine).
- [ ] Restore/build once the night before:

  ```bash
  cd "demos/Day 2/02-multi-turn-prompt-caching/src/Demo2.PromptCaching"
  dotnet build
  ```

- [ ] Set the key in user-secrets (this project has its own `UserSecretsId`, so set it here too):

  ```bash
  dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-..."
  ```

  Fallback: the `ANTHROPIC_API_KEY` environment variable also works.
- [ ] Do one full dry run the night before and note the token numbers you saw — quoting real numbers ("9,000 cached tokens billed as ~900") lands the ~10% point.
- [ ] **Backup:** screenshot of a successful two-call run showing non-zero `CacheCreationInputTokens` then non-zero `CacheReadInputTokens`.

## Step-by-step script

1. **Bridge from Demo 1 (30 s).** Narrate: *"Demo 1 proved the API is stateless. So two things are now your job: carrying the conversation history, and not paying full price to resend the same giant prefix every call. This demo does both."*

2. **Show the system prompt (2 min).** Open [`src/Demo2.PromptCaching/Program.cs`](src/Demo2.PromptCaching/Program.cs), scroll to `BuildSystemPrompt` at the bottom:
   - A real BookTracker domain briefing: role, style rules, domain model, worked examples, plus a 150-book catalog snapshot.
   - Narrate: *"It's deliberately big. Caching has a model-dependent minimum — about 2,048 tokens on Sonnet 4.6. A three-line system prompt silently never caches; `CacheCreationInputTokens` just stays 0. Pad the prompt with real domain content and the cache actually fires."*
   - Point at the `--break-cache` block at the top of the method — *"ignore that for now; it's the sabotage switch."*

3. **Show the two mechanisms (2 min).** Scroll up:
   - **History:** the `history` list — call 1 sends one user turn; then the code *appends the assistant reply and the next user turn* and sends the whole list again. *"That's multi-turn. There is no session object — this list IS the conversation."*
   - **Caching:** in `SendAsync`, the `System` block:

     ```csharp
     System = new List<TextBlockParam>
     {
         new() { Text = systemPrompt, CacheControl = new CacheControlEphemeral() },
     },
     ```

     *"One property. `CacheControlEphemeral` marks everything up to and including this block as cacheable."*

4. **Run it (2 min).**

   ```bash
   cd "demos/Day 2/02-multi-turn-prompt-caching/src/Demo2.PromptCaching"
   dotnet run
   ```

   **Expected output** (numbers approximate):

   ```
   System prompt: ~13,000 chars (~3,300 tokens)

   Assistant: <two-sentence Dune-alike recommendation>

   --- Call 1 (expect a cache WRITE) ---
     InputTokens:              ~30
     OutputTokens:             ~60
     CacheCreationInputTokens: ~3,500   <- you just WROTE the cache
     CacheReadInputTokens:     0

   Assistant: <two-sentence opposite recommendation>

   --- Call 2 (expect a cache READ — cached input bills at ~10%) ---
     InputTokens:              ~100
     OutputTokens:             ~60
     CacheCreationInputTokens: 0
     CacheReadInputTokens:     ~3,500   <- you READ it — billed at ~10%
   ```

   Point at `CacheCreationInputTokens` on call 1, then `CacheReadInputTokens` on call 2. Narrate: *"Call two re-sent the same ~3,500-token briefing but paid roughly a tenth for it. In a chat endpoint doing this on every request, that's most of your input bill gone."*

5. **The silent invalidator (1.5 min).** Now run it broken:

   ```bash
   dotnet run -- --break-cache
   ```

   This prepends `Current time: <now>` to the system prompt. **Expected:** `CacheCreationInputTokens` non-zero on *both* calls, `CacheReadInputTokens` **0** on call 2, plus the program's own diagnostic line. Show the one guilty line in `BuildSystemPrompt` (`sb.AppendLine($"Current time: {DateTime.Now:O}")`). Narrate: *"One changing byte at the top of the prefix and every call is a full-price cache write. Timestamps, request IDs, 'today's date' — classic silent invalidators. Nothing errors; you just quietly pay 10x."*

6. **Land it (30 s).** *"Two rules: put the stable stuff first and mark it; and verify with `Usage` — if `CacheReadInputTokens` is 0 in production, you're not caching, you're paying."*

## Recovery

- **`CacheReadInputTokens` stays 0 (without `--break-cache`):** the prefix changed between calls or is too small. Checklist, in order:
  1. Did you accidentally run with `--break-cache`? Re-run plain.
  2. More than ~5 minutes between the two calls? The ephemeral cache TTL expired — the demo makes both calls in one process, so this only bites if you're re-running call-by-call. Re-run the app.
  3. System prompt under the minimum (~2,048 tokens on Sonnet 4.6)? The app prints its size at startup — if someone trimmed `BuildSystemPrompt`, restore the catalog loop.
  Turn the failure into the lesson: this exact debugging path is the teaching point.
- **Auth fails:** same as Demo 1 — `dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-..."` from *this* project folder (separate `UserSecretsId`), or fall back to `ANTHROPIC_API_KEY`.
- **429 / 529:** transient; re-run. Note the SDK's built-in retries and the Polly discussion coming in Block 1C.
- **API fully down:** show the backup screenshot of the two-call run and walk the code; the numbers on the screenshot carry the point.

## Teaching callouts

- **Caching is a prefix match.** Byte-exact, top-down. Any change upstream — a timestamp, a reordered field, one edited word — invalidates everything after it. Structure prompts stable-first.
- **It fails silently.** No error, no warning — just `CacheReadInputTokens: 0` and a 10x bill. `Usage` is your only proof caching works; check it.
- **Minimum cacheable size is model-dependent.** ~2,048 tokens on Sonnet 4.6, ~4,096 on Haiku/Opus. Short system prompts never cache — make the cached content substantial (domain briefing, RAG context, few-shot examples).
- **Cache writes cost a small premium; reads bill at ~10%.** Caching wins whenever the prefix is stable and shared across repeat calls — exactly the system-prompt case.
- **History is yours to manage.** The message list is the conversation. In Lab 1, attendees persist it in `IDistributedCache` keyed by session id — this demo's `List<MessageParam>` is the in-memory version of the same idea.
