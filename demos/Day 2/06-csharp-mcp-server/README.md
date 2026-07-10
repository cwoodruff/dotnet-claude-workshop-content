# Day 2 Demo 6: C# MCP server → invoke from Claude Code

| | |
|---|---|
| **Slide deck** | `decks/Day 2/Day2-Section4-MCP-Servers-CSharp.pptx` (slide 9, "Live Demo") |
| **Scheduled** | 2:55 PM (Section 4, Block 4B → 4C) |
| **Duration** | ~12 min |
| **Risk** | **High** (live server + live Claude Code connect — record a backup run) |
| **Requires** | Local .NET 10 SDK, Claude Code installed and authenticated |
| **GitHub issue** | <https://github.com/cwoodruff/dotnet-claude-workshop/issues/21> |

This folder contains a **self-contained** MCP server in `src/` (`Demo6.McpServer`) so the demo
needs no database, no checkpoint folder, and no other running process. It is modeled line-for-line
on the real `src/BookTracker/BookTracker.Mcp` project (same packages, same registration pattern,
same tool shape) but backs its tools with an in-memory catalog of 10 seeded books — five of which
match "space," so the canonical demo prompt always returns hits. If you prefer to demo against the
real SQLite-backed server, everything below works identically with `BookTracker.Mcp` — same port,
same URL, same commands.

## What this demo teaches

- **The payoff of Day 2 Section 4:** "You *used* an MCP server yesterday (GitHub's, from Claude
  Code). Today you *built* one — in C#, on ASP.NET Core." This is the emotional high point of the
  section; lean into it.
- The official C# SDK surface: **`ModelContextProtocol`** + **`ModelContextProtocol.AspNetCore`**
  (v1.4.0) — `AddMcpServer().WithHttpTransport().WithToolsFromAssembly()` in DI, `app.MapMcp()` on
  the pipeline. Three lines to become an MCP host.
- Transport is **Streamable HTTP** — the current standard, not the older HTTP+SSE. `MapMcp()`
  serves it at the **root** of the app, so the URL clients register is just
  `http://localhost:5100` (no `/mcp` or `/sse` suffix).
- Tools are attributed methods: `[McpServerToolType]` on the class, `[McpServerTool]` per method,
  `[Description]` as the contract with the model. Services (`IBookCatalog`, `ILogger<T>`) are
  DI-injected into the tool method by the SDK.
- The **thin governed gateway** architecture: every tool validates input, logs the invocation, and
  delegates to a service. Claude calls *your* tools; it never touches the data store.
- Port discipline: the MCP server owns **5100**; `BookTracker.Api` owns **5255**. Two processes,
  two ports.

## Prerequisites / setup

Stage all of this **before the session** (this is a High-risk demo — it is on the night-before
checklist):

- [ ] **Record a backup run the night before** — screen-record the full flow below (server start →
  `claude mcp add` → `claude mcp list` → the space-books prompt with the tool call visible). If
  anything fails live, you play this. Non-negotiable for a High-risk demo.
- [ ] .NET 10 SDK on the demo machine: `dotnet --version` prints a 10.x version.
- [ ] The project builds clean: from this folder, `dotnet build src` → `0 Warning(s), 0 Error(s)`.
- [ ] Claude Code installed and authenticated (`claude --version` works; you signed in earlier
  for the Day 2 demos).
- [ ] **Port 5100 is free**: `lsof -i :5100` returns nothing. (`BookTracker.Api` on 5255 may keep
  running — different port, no conflict.)
- [ ] **No stale `booktracker` MCP registration** from a prior rehearsal:
  `claude mcp remove booktracker` (ignore the error if it doesn't exist).
- [ ] Two terminals arranged side by side, large font: **left** = server (its log is where the
  audience watches `search_books` fire), **right** = Claude Code.
- [ ] Smoke test the full flow once, then `claude mcp remove booktracker` so the live `add` is real.

## Step-by-step script

1. **Frame it (30 s).** Point at slide 9. Say: "Yesterday you were the MCP *host*, using GitHub's
   server from Claude Code. This is the other side: a server *we* wrote, in C#, on ASP.NET Core.
   Three lines of hosting code, attributed methods as tools. Let's connect Claude Code to it."

2. **Show the code briefly (1 min).** Open `src/Program.cs` — point at
   `AddMcpServer().WithHttpTransport().WithToolsFromAssembly()` and `app.MapMcp()`. Then flash
   `src/Tools/BookTools.cs`: `[McpServerToolType]`, `[McpServerTool(Name = "search_books")]`, the
   `[Description]` attributes. Say: "The description is the contract with the model — same
   discipline as skills and in-process tools: say *when* to call it."

3. **Start the server (left terminal).** From this demo folder:

   ```bash
   dotnet run --project src
   ```

   **Expected output:**

   ```text
   info: Microsoft.Hosting.Lifetime[14]
         Now listening on: http://localhost:5100
   info: Microsoft.Hosting.Lifetime[0]
         Application started. Press Ctrl+C to shut down.
   ```

   Say: "Port 5100 — the BookTracker API already owns 5255. The MCP endpoint is served at the
   root of this app over Streamable HTTP."

4. **Register it with Claude Code (right terminal).**

   ```bash
   claude mcp add --transport http booktracker http://localhost:5100
   ```

   **Expected output:** confirmation that the HTTP MCP server `booktracker` was added
   (e.g. `Added HTTP MCP server booktracker with URL: http://localhost:5100 to local config`).

   Note the URL is the bare root — `MapMcp()` in this server (and in `BookTracker.Mcp`) maps the
   Streamable HTTP endpoint at `/`, so there is **no** `/mcp` or `/sse` path segment.

5. **Prove it's connected.**

   ```bash
   claude mcp list
   ```

   **Expected output:** a line like

   ```text
   booktracker: http://localhost:5100 (HTTP) - ✓ Connected
   ```

   Pause on this. Say: "Connected means Claude Code completed the MCP `initialize` handshake with
   *our* server and discovered its tools — `search_books`, `get_book_details`, `add_book`."

6. **Invoke it from Claude Code.** Start `claude` in the right terminal and prompt:

   ```text
   Search BookTracker for books about space
   ```

   **Expected:** Claude Code calls the `booktracker` server's `search_books` tool (you'll see the
   tool-use approval/announcement in the Claude Code UI — approve it), and simultaneously the
   **left terminal** logs:

   ```text
   info: Demo6.McpServer.Tools.BookTools[0]
         Tool invoked: search_books query=space
   ```

   Claude's answer lists real seeded data: *The Martian*, *Project Hail Mary*, *Packing for Mars*,
   *A Brief History of Time*, *Cosmos*. Point back and forth between the two terminals: "Prompt on
   the right, C# breakpoint-able code executing on the left."

7. **Optional stretch (if time, ~1 min):** follow up with *"Get the details of book 2"* —
   `get_book_details` fires — or *"Add 'Contact' by Carl Sagan, sci-fi, 432 pages"* — `add_book`
   fires and a re-search finds it. One extra tool call is plenty; don't chase all three.

8. **Land it (30 s).** "That's the full loop: you used an MCP server yesterday; you built one
   today. Lab 4 has you do exactly this against the real BookTracker database." Hand off to the
   slide-10 security note, then Lab 4.

## Recovery

**High risk — the recorded backup run is your floor. Never debug live past ~90 seconds; narrate
over the recording instead.**

- **`claude mcp list` shows the server as failed / not connected:**
  - Check the **transport URL** first — it must be exactly `http://localhost:5100` with **no path
    suffix**. A stale `/sse` or `/mcp` suffix from muscle memory (other servers use those) is the
    #1 mistake. Fix: `claude mcp remove booktracker`, re-add with the bare URL.
  - Confirm the transport flag was `--transport http` (Streamable HTTP), not `sse`.
- **Server not actually listening:** the left terminal must show `Now listening on:
  http://localhost:5100`. If `dotnet run` picked another port, you launched from the wrong
  directory (the port lives in `src/Properties/launchSettings.json`) — restart from this folder
  with `dotnet run --project src`, or force it: `dotnet run --project src --urls http://localhost:5100`.
  Quick probe from any terminal: `curl -i http://localhost:5100` — any HTTP response (even
  405/406) proves it's listening; connection refused proves it isn't.
- **Port conflict:** `Address already in use` on startup usually means a prior rehearsal instance
  is still alive — `lsof -i :5100` then `kill <pid>`. If you accidentally aimed the client at
  **5255**, that's the BookTracker **API**, not the MCP server — it will never complete the MCP
  handshake; re-add with 5100.
- **Stale registration weirdness:** `claude mcp remove booktracker` then re-add; re-run
  `claude mcp list`.
- **Claude answers without calling the tool:** re-prompt more explicitly — "Use the booktracker
  MCP server's search_books tool to find books about space." If it still won't, fall back to the
  recording; the teaching point survives.
- **Anything else** (Claude Code auth hiccup, terminal freeze): play the recorded run and narrate
  the two-terminal choreography over it. The lesson is the architecture, not the live luck.

## Teaching callouts

- **"I used one yesterday, I built one today."** Day 1 you were the host consuming GitHub's MCP
  server; now you shipped the server side in C#. Same protocol, other chair — this is the
  section's payoff line.
- **Thin governed gateway.** Claude never touches the data store. Every tool validates its input,
  logs the invocation (who/what/when — the left terminal *is* the audit trail), and delegates to
  a service. The demo's `IBookCatalog` stands where the real server's `IBookService` and
  `DbContext` sit — the governance shape is identical.
- **The description is the contract.** The model decides *whether* and *how* to call your tool
  from the `[Description]` text alone — same discipline as tool definitions in Section 2 and
  skills on Day 1.
- **Streamable HTTP at the root, own port.** `MapMcp()` = one endpoint at `/`; the server owns
  5100 while the API owns 5255. Any MCP host — Claude Code today, VS Code or your own
  `AnthropicClient` app tomorrow — can connect to this same URL.
- **Segue to slide 10:** a governed gateway is only as safe as its rules — authentication,
  argument validation, and invocation logging are not optional in production (the real
  `BookTracker.Mcp` adds a bearer-token gate and an `AuditLogger` for exactly this).
