# Day 1 Demo 1: Installation Walk-Through

| | |
|---|---|
| **Slide deck** | `decks/Day 1/Section1-Foundations-Claude-Code-CLI.pptx` (slide 7, "Get Running") |
| **Scheduled** | 9:25 AM |
| **Duration** | 20 min |
| **Risk** | Medium (live install — network + machine variance) |
| **Internet** | Yes: the install and sign-in, and Claude Code's model connection for the rest of the day |
| **GitHub issue** | <https://github.com/cwoodruff/dotnet-claude-workshop/issues/3> |

## What this demo teaches

- The install paths per OS: macOS/Linux is a one-line npm install; Windows runs everything inside **WSL2** (not PowerShell or CMD — hooks and subagents later today need a Linux shell).
- Authentication options: browser **OAuth with a Claude.ai Pro/Max subscription** (recommended today, no keys to manage) vs. an `ANTHROPIC_API_KEY` (the pay-as-you-go path attendees will use on Day 2).
- The two ways to run the same agent: the interactive REPL (`claude`) and one-shot mode (`claude "prompt"`).
- "Same agent, three surfaces" — terminal today; VS Code and JetBrains come in Section 3.

## Prerequisites / setup

Stage all of this **before doors open** (it is on the night-before checklist):

- [ ] A clean terminal on the demo machine, large font, dark-on-light or high-contrast theme readable from the back row.
- [ ] Node.js 18+ installed and on PATH (`node --version` — an old Node is the #1 install failure).
- [ ] **Either** Claude Code already uninstalled so the install is genuinely live, **or** (safer) already installed and verified, and you narrate the one-liner while it no-ops. Decide before you start; do not discover mid-demo.
- [ ] You are logged in to Claude.ai in the demo machine's default browser with a **paid** (Pro/Max) account, so the OAuth hop is one click.
- [ ] If demoing the Windows path: a WSL2 Ubuntu terminal ready and tested, or a screenshot of the flow if you're on a Mac.
- [ ] A TA is briefed and circulating — this demo doubles as the straggler-rescue window.
- [ ] Smoke test the night before: `claude "What is today's date?"` returns a sensible answer.

## Step-by-step script

1. **Frame it (30 s).** Point at slide 7. Say: "macOS and Linux, one line. Windows, everything runs inside WSL2 — not PowerShell or CMD, because hooks and subagents need a Linux shell later today."

2. **Install.** Type and run, narrating that this is the same command on macOS, Linux, and inside WSL2:

   ```bash
   npm install -g @anthropic-ai/claude-code
   ```

   **Expected output:** npm resolves and installs the package, ending with something like `added N packages in Xs` (or `changed 1 package` if already installed — say so out loud, it's fine). Takes 15–60 s on conference Wi-Fi.

3. **Verify the binary.**

   ```bash
   claude --version
   ```

   **Expected output:** a version string (e.g. `2.x.x (Claude Code)`). Say: "That's your readiness check — if this prints a version, the install is done. This exact command is the first line of Lab 1 Part A."

4. **First run → OAuth.**

   ```bash
   claude
   ```

   **Expected:** on a machine that has never authenticated, Claude Code prompts to sign in and opens the browser for OAuth. Click through with the pre-logged-in Claude.ai account; the terminal confirms authentication and drops into the REPL.

   While the browser hop happens, say: "Two ways to authenticate. Today: sign in with your Claude.ai subscription — browser OAuth, no keys to manage. Tomorrow, Day 2, you'll use an API key (`ANTHROPIC_API_KEY`) because you'll be *building against* the API, not using the agent. Note the plan requirement: Claude Code needs a **paid** plan — Free doesn't include it — and subscription usage is a shared pool on a rolling 5-hour window, so go easy on casual Claude.ai chatting before labs."

   If your machine is already authenticated, show the REPL opening instantly and narrate what first-run looks like instead.

5. **Show the REPL, then exit.** In the REPL, run `/help` and point at the command list ("we'll use `/init`, `/memory`, `/clear` today"). Exit with `Ctrl+C` (twice) or `/exit`.

6. **One-shot smoke test.**

   ```bash
   claude "What is today's date?"
   ```

   **Expected output:** a coherent answer with today's date, then the process exits. Say: "That's one-shot mode — a single request, then exit. Ideal for scripts and CI. The REPL you just saw is where we'll live all day."

7. **Land the surfaces point (30 s).** "Same agent, three surfaces: terminal now, VS Code and JetBrains in Section 3. Everything you configure today — CLAUDE.md, rules, hooks — follows the agent to every surface."

8. **Straggler sweep.** Ask for hands: who does *not* get a version from `claude --version`? Pair each blocked person with the TA immediately — do not debug individual machines from the podium. Then hand back to slides 8–10 to set up Demo 2 at 9:45.

## Recovery

- **Install fails on the demo machine:** don't debug live past ~90 seconds. Fall back to an attendee who is already set up — put their screen on the projector (or narrate over their shoulder) and let the TA take your machine. The show goes on.
- **Old Node version** (the most common failure): `npm install -g` errors or `claude` crashes at startup. Fix is `node --version` → upgrade to 18+ (nvm: `nvm install --lts && nvm use --lts`) → reinstall. Hand to the TA if it's an attendee machine.
- **Windows attendee in PowerShell/CMD:** the fix is not troubleshooting PowerShell — it's moving them into the WSL2 Ubuntu terminal and reinstalling there. If WSL2 itself isn't installed, that's a 10+ minute detour: TA pairs with them through Lab 1 Part A while the room moves on.
- **OAuth browser hop fails / wrong account:** sign out of Claude.ai in the browser, re-run `claude`, sign in with the paid account. If the room's network blocks the callback, use the API-key path as a visible fallback (`export ANTHROPIC_API_KEY=...` from a key you have staged — never show the key on screen).
- **Conference Wi-Fi dies mid-install:** switch to your phone hotspot. Keep it handy: every later demo still needs Claude Code's model connection, even when it only reads local files.

## Teaching callouts

- "Same agent, three surfaces — today we run it in the terminal."
- "Windows means WSL2, not PowerShell — hooks and subagents need a Linux shell this afternoon."
- "OAuth with your subscription today; an API key tomorrow when you start *building* against the API."
- "If `claude --version` prints a version, you're ready — that's the whole readiness check."
