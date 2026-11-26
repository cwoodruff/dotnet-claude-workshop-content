# Day 1 Demo 8: GitHub MCP Server

| | |
|---|---|
| **Slide deck** | `decks/Day 1/Section3-IDE-Integration-MCP.pptx` (slide 10, "Live Demo") |
| **Scheduled** | 2:30 PM |
| **Duration** | 20 min |
| **Risk** | **High** (live internet + GitHub + a remote MCP server — three external dependencies at once) |
| **Internet** | **Yes** — the only afternoon demo that needs it |
| **GitHub issue** | <https://github.com/cwoodruff/dotnet-claude-workshop/issues/12> |

## What this demo teaches

- The MCP roles live: Claude Code is the **host**, the connector inside it is the **client**, and GitHub's remote MCP endpoint is the **server** exposing **tools** (create an issue), **resources** (repo data), and **prompts**.
- `claude mcp add` / `claude mcp list` — registering a remote HTTP MCP server with a PAT and verifying the connection.
- The **query → action** escalation: read-only queries flow freely; the first *outward* action (creating an issue) trips the **permission prompt** — the same approval model attendees saw for file edits, now guarding a real-world side effect.
- The Day 1 → Day 2 bridge: today you *use* an MCP server someone else built; Day 2 Section 4 you *build* one in C# against the BookTracker database.
- The issue created here ("Add Reading Progress tracking") is the exact feature built in **Lab 4** — the demo literally files the afternoon's work.

## Prerequisites / setup

Do all of this **before the session** (PAT creation is the slow step — it is on the night-before checklist):

- [ ] **GitHub PAT created and staged** (in a password manager or env var — never in a file on screen):
  - **Classic token:** the **`repo`** scope is sufficient and simplest.
  - **Fine-grained token:** grant **Issues: read/write**, **Pull requests: read**, and **Contents: read** on the workshop repo (`cwoodruff/dotnet-claude-workshop` or your demo fork).
- [ ] **Register the GitHub remote MCP server** ahead of time so the demo starts from a working state (you will re-run the command on screen for teaching, where it harmlessly re-registers):

  ```bash
  claude mcp add --transport http github https://api.githubcopilot.com/mcp/ \
    --header "Authorization: Bearer ghp_YOUR_TOKEN_HERE"
  ```

- [ ] Verify the night before **and** at the lunch break: `claude mcp list` shows `github - connected`.
- [ ] **Recorded backup ready** — this is the one High-risk demo of the day. Have a screen recording of the full sequence (register → list issues → propose fix → create issue → permission prompt) queued in a player, tested on the projector. Do not skip this; conference Wi-Fi fails at 2:30 PM, not at 8 AM.
- [ ] The demo repo has a few **real open issues** so the "list issues" prompt returns something interesting. Seed 2–3 small issues the night before if the tracker is empty (e.g. "Add pagination to GET /api/books", "Search endpoint is injection-prone").
- [ ] Terminal running `claude` from `src/c2/BookTracker` (the state before Lab 3), large font.
- [ ] Read `artifacts/offline-fallback.md` in this folder once so the fallback is muscle memory, The finished fallback issue is `src/c3/BookTracker/proposed-issues/reading-progress.md`; c2 has no `proposed-issues/` folder, so Claude creates it if you use the fallback.

> **Security note:** the token goes into the `claude mcp add` command line. Type it off-screen or paste from a manager with the terminal briefly hidden; never leave it visible on the projector, and revoke the demo PAT after the workshop.

## Step-by-step script

1. **Frame it (1 min).** Point at slide 10. Say: "Without a standard, M hosts times N tools means M×N integrations. MCP makes it M+N. You've been the host all day — now we plug a server in. GitHub runs a remote MCP server; we just need to register it."

2. **Register the server (2 min).** Show the command (token pre-filled off-screen or pasted with the terminal momentarily hidden):

   ```bash
   claude mcp add --transport http github https://api.githubcopilot.com/mcp/ \
     --header "Authorization: Bearer ghp_YOUR_TOKEN_HERE"
   ```

   **Expected output:** confirmation that the `github` server was added (HTTP transport). Narrate: "Remote HTTP transport — no Docker, no local process. The PAT is the server's credential, not Claude's."

3. **Verify the connection (1 min).**

   ```bash
   claude mcp list
   ```

   **Expected output:** a line like `github: https://api.githubcopilot.com/mcp/ (HTTP) - ✓ Connected`. Say: "That's the whole health check. If this says connected, Claude has GitHub's tool catalog."

4. **Query — read-only (4 min).** Inside `claude`, run:

   ```text
   List the open issues in this repo, sorted by creation date. Which look like good first
   contributions, and why?
   ```

   **Expected:** Claude calls the GitHub MCP issue tools (visible as tool calls), returns the real open issues, and reasons about which are approachable. Narrate the tool calls as they scroll: "It's not scraping a website — it's calling a typed tool the server exposed."

5. **Query + code grounding (4 min).**

   ```text
   Find the issues related to the Books API. For the top one, read the relevant source and
   propose a concrete fix — don't implement it yet.
   ```

   **Expected:** Claude combines MCP data (the issue) with local file reads (`BookTracker.Api/Endpoints/BookEndpoints.cs`, `BookTracker.Core/Services/BookService.cs`) and proposes a fix without editing. Callout: "Two context sources in one answer — the tracker via MCP, the code via the filesystem. And note it obeyed 'don't implement yet.'"

6. **Action — create the issue (5 min).** This is the money shot:

   ```text
   Create an issue titled "Add Reading Progress tracking" — track current page, total pages, and
   reading status (reading / completed / want-to-read) with start and finish dates. Include
   acceptance criteria, the affected files, and a complexity estimate.
   ```

   **Expected:** Claude drafts the issue body, then the **permission prompt fires** before the create-issue tool call executes. Pause on the prompt — do not approve immediately. Say: "Reading was free. *Writing to the outside world* asks first. Same approval model as file edits, extended to real-world side effects." Approve, then open the created issue in the browser to prove it's real.

7. **Land the bridges (2 min).** "Two things just happened. One: you watched the pattern you'll repeat in Lab 3 in about twenty minutes. Two: that issue is not a prop — it's the **Reading Progress** feature you will spec and build in Lab 4 this afternoon. File it now, build it after the break. And tomorrow you stop using MCP servers and *build one* in C#."

8. **Hand off to Lab 3.** Flag the PAT creation as the slow step: "Start your token now, before you touch the IDE part."

## Recovery

This is the High-risk demo. Decide fast — no failure mode here is worth more than 60 seconds of live debugging.

- **`claude mcp list` shows disconnected / auth error:** almost always the token — wrong scopes, expired, or pasted incomplete. One retry: re-run `claude mcp add` with the staged backup token (keep a second PAT ready). If that fails, go offline (below).
- **GitHub or the MCP endpoint is down, or Wi-Fi dies:** switch immediately to the **offline fallback** — full script in [`artifacts/offline-fallback.md`](artifacts/offline-fallback.md). Summary:
  1. **Query half:** have Claude answer "what's a good first contribution?" from local ground truth — `git log --oneline -20` plus the repo files — instead of the issue tracker.
  2. **Action half:** have Claude **write the proposed issue to a local file** at `proposed-issues/reading-progress.md` with the same title, acceptance criteria, affected files, and complexity estimate. Same structured output, no GitHub or MCP needed (Claude Code still needs its model connection), and `src/c3/BookTracker/proposed-issues/reading-progress.md` is the reference.
  3. Narrate explicitly: "Same query → action pattern; the transport is the only thing that changed."
- **Everything is down (no MCP, flaky Claude connection):** play the **recorded backup** and narrate over it. This preserves the timeline for Lab 3 — attendees on working networks can still do the live version themselves.
- **Slow responses eat the clock:** cut step 5 (the propose-a-fix prompt) — steps 4 and 6 alone carry the teaching (query, then gated action).
- **Permission prompt doesn't appear** (previously granted in a stale session): don't hide it — say "I've pre-approved this tool in an earlier session; on your machines it will ask," and show `/permissions` if time allows.

## Teaching callouts

- "Host, client, server: you're the host, the connector is the client, GitHub is the server. One host, many servers."
- "Tools, resources, prompts — a server exposes capabilities; the model decides when to call them."
- "Reading is free; **acting** asks first. The permission prompt is the boundary between query and side effect."
- "The issue we just filed is Lab 4's feature. File it now, build it after the break — the tracker is the to-do list."
- "Today you *use* an MCP server. Tomorrow (Day 2 Section 4) you *build* one in C# against the BookTracker database."
- "MCP gives Claude the tools; a **skill** teaches it how to use them well. Steering and tooling are complementary."
