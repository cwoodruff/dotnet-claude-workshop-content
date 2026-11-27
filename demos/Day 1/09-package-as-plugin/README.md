# Day 1 Demo D6b: Package as Plugin

| | |
|---|---|
| **Slide deck** | `decks/Day 1/Section2-Steering-Claude-Code.pptx` |
| **Scheduled** | 12:20 PM (Section 2, final demo — the Lab 2 lead-in) |
| **Duration** | 4 min |
| **Risk** | Low (one read-only `claude plugin validate`) |
| **Internet** | Yes, for Claude Code's model connection (nothing else) |
| **GitHub issue** | https://github.com/cwoodruff/dotnet-claude-workshop/issues/11 |

## What this demo teaches

- A **plugin bundles** everything Section 2 built — skills + subagents + hooks (+ commands,
  output styles) — into **one installable, versionable unit**.
- The manifest (`.claude-plugin/plugin.json`) is tiny because **components are discovered by
  convention** from the folder layout (`skills/`, `agents/`, `commands/`), not enumerated in the
  manifest.
- The two distribution paths: **commit `.claude/` into the repo** (the whole team inherits the
  setup on clone) or **publish to a marketplace** so anyone installs it with one command. The .NET
  kit is the finished example.

## Prerequisites / setup

- [ ] Copy this demo's artifact into the BookTracker solution (run from the repo root):

      ```bash
      mkdir -p "src/c1/BookTracker/.claude/.claude-plugin"
      cp "demos/Day 1/09-package-as-plugin/artifacts/.claude-plugin/plugin.json" "src/c1/BookTracker/.claude/.claude-plugin/plugin.json"
      ```

      Lab 2's attendee manifest is `src/c2/BookTracker/.claude/.claude-plugin/plugin.json`;
      this demo's version adds the subagent to the description and an `author` field.
- [ ] Run `claude plugin validate src/c1/BookTracker/.claude` once and confirm it passes and lists
      `booktracker-kit:add-api-endpoint` and `booktracker-kit:test-writer`.
- [ ] The previous demos' artifacts staged (D4 skill, D4b rules, D5 agent, D6 hook +
      settings) — the plugin story only lands if the kit visibly *exists*:

      ```bash
      find "src/c1/BookTracker/.claude" -type f -not -name ".DS_Store"
      ```

- [ ] No fresh session needed. Editor + terminal is enough.
- [ ] Optional: `git status` clean-ish in `src/c1/BookTracker` so the closing `git add .claude`
      shows a tidy, plausible diff. (Don't actually commit during the demo unless you want to —
      staging is the visual.)

## Step-by-step script

1. **Recap what you built** (30 seconds). Show the tree:

   ```bash
   find .claude -type f -not -name ".DS_Store"
   ```

   Narrate: "In forty minutes we built a skill, two rules, a subagent, a guardrail hook, and the
   settings that wire them. That's a team asset. How do we ship it?"

2. **Show the manifest.** Open `.claude/.claude-plugin/plugin.json`:

   ```json
   {
     "name": "booktracker-kit",
     "version": "0.1.0",
     "description": "BookTracker steering kit: API/data rules, the add-api-endpoint skill, the test-writer subagent, and a destructive-command guardrail hook.",
     "author": {
       "name": "BookTracker Team"
     }
   }
   ```

   Narrate: "That's the whole manifest — name, version, description. Notice what's *not* here:
   no list of skills, no list of agents."

3. **Land the convention point** by asking Claude Code what it sees:

   ```bash
   claude plugin validate .claude
   ```

   Expected: `skills: booktracker-kit:add-api-endpoint`, `agents: booktracker-kit:test-writer`, and
   **Validation passed** (ignore the README/license/icon notes; those are for public directory
   submissions).

   > "Components are discovered **by convention** from the folder layout. We put the manifest in
   > `.claude/`, so the skill folder and the subagent we already built *are* the plugin. Rules and
   > the `settings.json` hooks aren't plugin parts; they travel with the repo."

4. **Show distribution path #1 — commit the kit:**

   ```bash
   git add .claude
   git status
   ```

   Expected output: `git status` lists the staged `.claude/` files (skill, rules, agent, hook,
   settings, plugin manifest). Narrate: "Path one: check `.claude/` into the repo. Every teammate
   who clones gets the skill, the rules, the subagent, and — most importantly — the guardrail.
   Onboarding is `git clone`."

5. **End on distribution path #2 — one install.** Put the command on screen (running it is
   optional; it needs GitHub access and adds about 20 seconds):

   ```bash
   claude plugin install dotnet-kit --marketplace cwoodruff/dotnet-claude-code-kit
   ```

   Narrate: "Path two: publish to a marketplace. This is the finished version of what you're about
   to build: the .NET kit, nine skills, eight commands, six subagents, three C# hooks, one command
   to install. It's the Lab 2 bonus."

6. **Bridge to Lab 2** (this demo is the lead-in):

   > "In Lab 2 you build your own kit: migrate your CLAUDE.md conventions into rules, write a
   > skill with real gotchas, wire the guardrail hook, and stage it all with git. Everything you
   > just watched, you now do."

## Recovery

If `claude plugin validate` says "No manifest found", the manifest is in the wrong folder: it has
to be `.claude/.claude-plugin/plugin.json`. If the manifest file is missing, re-run the copy
command from setup (or even create the three-line JSON live — it's small enough to type, and
typing it reinforces how minimal the manifest is). If `git add .claude` stages surprise files,
that's fine — narrate what each one is; it's a recap, not a mistake.

## Teaching callouts

- **The manifest is small on purpose** — convention over configuration. The folder layout you
  learned in D4–D6 *is* the packaging format.
- **Version the kit like code.** `version: 0.1.0` matters: steering assets evolve, and teams
  should review changes to hooks and rules like any other code review — a hook edit is a
  behavior change for every developer's agent.
- **The guardrail is the killer argument for committing `.claude/`:** a safety hook that only
  exists on your machine protects only you.
- **The manifest location moved.** Older guidance put `plugin.json` at the plugin root; Claude
  Code 2.1.332 only reads `.claude-plugin/plugin.json` and fails validation otherwise. Re-run
  `claude plugin validate` the night before (it's on the checklist in
  `docs/Day 1/_instructor/demo-plan.md`).
- Closing line for the section: "Steering isn't prompt tricks — it's an artifact you build,
  version, and ship with the repo."
