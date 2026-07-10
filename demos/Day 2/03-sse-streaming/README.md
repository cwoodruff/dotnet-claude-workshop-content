# Day 2 Demo 3: Live SSE Streaming Endpoint

| | |
|---|---|
| **Deck** | `decks/Day 2/Day2-Section2-Streaming-Tools-Thinking.pptx` (slide 5, "See It Work") |
| **Section** | Day 2 · Section 2 — Streaming, Tool Calling & Extended Thinking (Block 2A) |
| **Duration** | ~8 min |
| **Risk** | Medium (live API call; buffering proxies can spoil the effect) |
| **Internet** | Yes — Anthropic API |
| **Issue** | <https://github.com/cwoodruff/dotnet-claude-workshop/issues/18> |

## What this demo teaches

- **Why streaming matters:** a non-streaming chat call sits silent for seconds, then dumps the
  whole answer. Streaming surfaces tokens *as they're generated* — perceived latency drops even
  though total time is the same.
- The SDK's `Messages.CreateStreaming(...)` returning an `IAsyncEnumerable` of stream events.
- Picking out text deltas with `TryPickContentBlockDelta` + `Delta.TryPickText`.
- Server-Sent Events from ASP.NET Core Minimal APIs: `Content-Type: text/event-stream`, one
  `data:` frame per chunk, and — the part everyone forgets — `await res.Body.FlushAsync(ct)`
  **per chunk** so the client sees tokens immediately.
- This is exactly what attendees build in Lab 2 Part A against BookTracker.

## Prerequisites / setup

- [ ] .NET 10 SDK installed (`dotnet --version` → 10.x).
- [ ] An Anthropic API key with credit (the Day 2 workshop key is fine).
- [ ] Key stored — from `./src/Demo3.SseStreaming/`:

  ```bash
  cd "demos/Day 2/03-sse-streaming/src/Demo3.SseStreaming"
  dotnet user-secrets set Anthropic:ApiKey "sk-ant-..."
  ```

  (or `export ANTHROPIC_API_KEY=sk-ant-...` in the shell you'll run from — the app checks
  user-secrets first, then the environment variable. **Never** paste the key into a file on screen.)
- [ ] Solution builds: `dotnet build` from `./src/` — 0 warnings, 0 errors.
- [ ] Two terminals ready side by side: one to run the app, one for `curl`.
- [ ] Port **5055** free (this demo deliberately avoids 5255 so it can run alongside BookTracker).
- [ ] Backup: a recorded clip of tokens trickling in, in case the venue network buffers SSE.

## Step-by-step script

1. **Set the problem (30 s).** Say: *"A non-streaming call sits silent for seconds, then dumps
   the whole answer. Same total time — terrible UX. Let's push tokens to the client as Claude
   generates them."*

2. **Show the service** — open `./src/Demo3.SseStreaming/StreamingService.cs`. Walk the three
   beats:
   - `CreateStreaming` returns an `IAsyncEnumerable` of stream events — no callbacks, just
     `await foreach`.
   - `ev.TryPickContentBlockDelta(out var delta) && delta.Delta.TryPickText(out var text)` —
     the stream carries many event types; we only care about text deltas.
   - `yield return text.Text` — the service surfaces plain strings; it knows nothing about HTTP.

3. **Show the endpoint** — open `./src/Demo3.SseStreaming/Program.cs`, the
   `/api/chat/stream` mapping. Point at exactly two lines:
   - `res.Headers.ContentType = "text/event-stream"` — this is all SSE is: a long-lived HTTP
     response with `data:` frames.
   - `await res.Body.FlushAsync(ct)` — say: *"This line is the demo. Delete it and everything
     still 'works', but the client gets one buffered blob at the end."*

   Also note the key handling at the top of `Program.cs`: user-secrets or env var, never
   hardcoded — throw at startup if missing.

4. **Run it** (terminal 1):

   ```bash
   cd "demos/Day 2/03-sse-streaming/src/Demo3.SseStreaming"
   dotnet run
   ```

   Wait for `Now listening on: http://localhost:5055`.

5. **Stream it** (terminal 2):

   ```bash
   curl -N "http://localhost:5055/api/chat/stream?q=In three short sentences, why do chat UIs stream tokens?"
   ```

   The `-N` disables curl's own buffering — mention it, it foreshadows the recovery section.

6. **Expected output:** `data: ...` frames appearing **one at a time**, tokens visibly trickling
   across a few seconds — not one dump at the end:

   ```
   data: Chat
   data:  UIs stream
   data:  tokens because users
   ...
   ```

   Narrate while it types: *"Same total generation time as before — but the user is reading at
   200 ms instead of staring at a spinner for six seconds."*

7. **Optional flourish (if time):** comment out the `FlushAsync` line, restart, re-run the curl —
   everything arrives at once. Restore it. This one-line before/after lands the lesson harder
   than any slide.

8. **Bridge to Lab 2 Part A:** attendees now build the same thing into BookTracker
   (`curl -N http://localhost:5255/api/chat/stream?q=hello`).

## Recovery

- **All the tokens arrive at once (buffering):** the classic failure. Check in order:
  1. `await res.Body.FlushAsync(ct)` is present in the endpoint (step 7 above is this failure,
     performed intentionally).
  2. You used `curl -N` — plain `curl` buffers on the client side and ruins the effect even
     when the server is correct.
  3. Something between curl and Kestrel is buffering: corporate/venue proxies, VPNs, an
     `nginx`/dev-tunnel in front of the app, or antivirus HTTPS inspection. Hit
     `http://localhost:5055` directly — never through a tunnel or forwarded port for this demo.
  4. Still buffered? Fall back to the **recorded clip** and say what the audience would see.
     The code walk (steps 2–3) carries the teaching even without live tokens.
- **401/auth error at startup or first call:** the app throws a clear message if the key is
  missing. Re-run `dotnet user-secrets set Anthropic:ApiKey ...` from the *project* directory
  (user-secrets are per-project — running it elsewhere is the usual mistake). No key ever needs
  to appear on screen.
- **Port 5055 in use:** `dotnet run --urls http://localhost:5155` and adjust the curl.
- **Slow venue network:** slower token trickle actually *improves* this demo — say so.

## Teaching callouts

- **Same total time, far better UX.** Streaming doesn't make the model faster; it makes the
  wait *visible and useful*. This is what every real chat UI does.
- **SSE is just HTTP.** No WebSockets, no SignalR needed for server→client token push: a
  `text/event-stream` content type and `data:` frames. A browser consumes it with `EventSource`.
- **Flush is the contract.** ASP.NET Core buffers responses by default at several layers;
  `FlushAsync` per chunk is what turns "a response" into "a stream".
- **The service/endpoint split matters:** `StreamingService` yields plain strings
  (`IAsyncEnumerable<string>`), so the same service could feed SSE, SignalR, or a console.
  BookTracker's `StreamingService` (`src/BookTracker/BookTracker.Api/Services/StreamingService.cs`)
  has the identical shape behind an interface.
- **Cancellation flows end-to-end:** the endpoint's `CancellationToken` fires when the client
  disconnects (Ctrl-C the curl) and stops the upstream Claude stream — you stop paying for
  tokens nobody is reading.
