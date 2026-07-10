# Day 2 Demo 8: GitHub Actions AI Review on a PR

| | |
|---|---|
| **Slide deck** | `decks/Day 2/Day2-Section5-AI-Testing-CICD.pptx` (slide 9, "Live Demo," part 2) |
| **Scheduled** | 4:00 PM |
| **Duration** | ~8 min |
| **Risk** | **HIGH** (live GitHub Actions + live Anthropic API — have the recorded backup ready) |
| **Requirements** | Internet; a GitHub repo with Actions enabled; `ANTHROPIC_API_KEY` repo secret; a staged demo PR; `gh` CLI authenticated |
| **GitHub issue** | <https://github.com/cwoodruff/dotnet-claude-workshop/issues/23> |

## What this demo teaches

- The **review skill, now in CI**: the same code review attendees ran locally with `/code-review` on Day 1 becomes an automated PR gate — the Day 1 steering touchpoint made concrete.
- The cleanest path is the **official `anthropics/claude-code-action`** — it handles checkout, diff, and PR plumbing and posts the comment for you. The DIY alternative is Claude Code **headless** (`claude -p`, print mode) over the diff plus `gh pr comment`.
- **Cost tiering in CI**: `--model claude-haiku-4-5` is the right tier for mechanical review work and keeps every PR review cheap.
- What the review hunts for: security issues, .NET issues, missing error handling, and **CLAUDE.md / `.claude/rules/` violations** — your project conventions become enforceable review criteria.

Artifacts in this folder:

- `artifacts/ai-code-review.yml` — the complete workflow (official action, with the commented headless-CLI alternative at the bottom). Copy to `.github/workflows/ai-code-review.yml`.
- `artifacts/demo-pr-checklist.md` — the night-before staging checklist. **Do all of it.**

## Prerequisites / setup

Everything here is on `artifacts/demo-pr-checklist.md` in more detail — the short list:

- [ ] **`ANTHROPIC_API_KEY` repo secret** set (`gh secret set ANTHROPIC_API_KEY`) on the demo repo, with credit verified.
- [ ] **Actions enabled** on the repo (fresh forks have it disabled) and workflow permissions set to read/write.
- [ ] `ai-code-review.yml` merged to **`main`** the night before — `pull_request` workflows run the file from the base branch, so it must land before the demo PR opens.
- [ ] **Staged draft PR** with review bait (e.g. a small endpoint that interpolates user input into SQL or skips validation) on a pushed branch, ready to mark "Ready for review".
- [ ] **Recorded backup** of a successful dry run (PR → Actions run → AI comment), plus screenshots of the run log and the comment, saved locally.
- [ ] `gh auth status` green on the demo machine; browser logged in to GitHub.

## Step-by-step script

1. **Frame it (45 s).** "Yesterday you ran `/code-review` by hand. That review is a *skill* — a playbook. Playbooks that live in one developer's terminal don't scale; playbooks in CI review every PR whether or not anyone remembers. Same discipline, promoted into the pipeline."

2. **Show the workflow file (90 s).** Open `.github/workflows/ai-code-review.yml` in the demo repo (same content as `artifacts/ai-code-review.yml`). Walk top to bottom:
   - `on: pull_request` — every PR against `main` gets reviewed.
   - `anthropics/claude-code-action@v1` — "the official action. It checks out, diffs the PR, runs the review, and posts the comment. You bring two things: the API key secret and the prompt."
   - The prompt — "security, .NET issues, missing error handling, and CLAUDE.md violations. Your conventions file just became a review rubric."
   - `--model claude-haiku-4-5` — "Haiku in CI. Mechanical review work doesn't need the big model, and this keeps every PR at pennies."
   - Scroll to the commented block at the bottom: "If you'd rather own the plumbing: `claude -p` — print mode, headless — over the raw diff, then `gh pr comment`. Same review, more control. Lab 5 Part C lets you pick either."

3. **Open the PR (live moment).** Bring up the staged draft PR in the browser and click **Ready for review** (or, from the terminal):

   ```bash
   gh pr ready demo/ai-review-bait
   ```

   **Expected:** the "AI Code Review" check appears on the PR within seconds and starts running.

4. **Show the run while it works (2–3 min of narration).** Click through to the **Actions** tab → the running workflow → the job log. Narrate the stages as they stream: checkout → the action diffs the PR → Claude reviews → comment posts. Fill the wait with the flow diagram: "**diff → review → comment.** That's the whole machine. The diff is the context, the prompt is the rubric, the comment is the deliverable."

5. **The payoff.** Switch back to the PR's **Conversation** tab and refresh.

   **Expected output:** an AI review comment on the PR — a short markdown bullet list of findings with file references. With the staged bait, it should flag the SQL interpolation / missing validation you planted. Read the best finding aloud: "It caught the injection. Nobody on your team had to remember to look."

6. **Land it (45 s).** "Two things to take away. One: this is the *same* review skill from Day 1 — local `/code-review` and this workflow share the rubric. Two: Haiku makes it cheap enough to run on literally every PR. Lab 5 Part C: you wire this into your own fork in about eight minutes — the YAML is provided."

7. **Hand off to Lab 5.** Mention Part D uses the identical headless pattern for release notes on tag push — commits in, grouped notes out, Haiku again.

## Recovery

**High risk — the backup is part of the plan, not an apology.**

- **Actions is slow or the runner queues** (>3 min): don't stall. Open the **dry-run PR from last night** — a real prior run with a real comment — and walk its Actions log and comment instead. Keep the live run going in a background tab; if it lands before you finish, switch back for the applause moment.
- **Actions/API is down**: show the **screenshots** (run log + PR comment) or play the **recorded backup**, and teach the flow on the static images: *diff → review → comment*. The workflow diffs the PR against the base branch, feeds that diff to Claude with the review rubric as the prompt, and posts whatever comes back as a PR comment. The teaching survives without the live run.
- **Workflow doesn't trigger at all**: most likely the YAML isn't on `main` (base branch) or Actions is disabled on the fork — say so out loud, it's the #1 thing attendees will hit in Lab 5, then fall back to the recorded run.
- **Auth/secret failure in the log**: point at the failing step, explain `ANTHROPIC_API_KEY` comes from repo secrets (never committed), and pivot to the backup.

## Teaching callouts

- "This is the Day 1 steering touchpoint paying off: the CI review *is* a code-quality skill — the same `/code-review` you run locally, now triggered on every PR."
- "The official action handles the plumbing — checkout, diff, comment. You bring the key and the rubric."
- "Haiku keeps CI cheap — the right model tier for mechanical review work."
- "CLAUDE.md violations are a review category. Your conventions doc is now enforced, not aspirational."
- "diff → review → comment. Once you see the shape, release notes on tag push is the same machine with a different prompt."
