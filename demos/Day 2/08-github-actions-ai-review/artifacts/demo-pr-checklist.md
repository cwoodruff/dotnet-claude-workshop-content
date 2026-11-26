# Demo 8: night-before checklist

Do every item. This is the high-risk demo of the afternoon; the checklist is what makes it boring.

## Repo

- [ ] Use a demo repo you control (a fork of the content repo works). Note its name; every `gh`
      command below runs inside it.
- [ ] **Actions enabled.** Forks start with Actions disabled: *Settings → Actions → General →
      Allow all actions*.
- [ ] **Workflow permissions** set to *Read and write* (same page), so the job can comment.
- [ ] **Secret set:** `gh secret set ANTHROPIC_API_KEY` (paste the key at the prompt, never on
      screen). Confirm the account has credit.
- [ ] Copy `artifacts/ai-code-review.yml` to `.github/workflows/ai-code-review.yml`, commit, and
      **merge it to `main`**. A `pull_request` workflow runs the base branch's copy, so it must be on
      `main` before the demo PR opens.

## The bait PR

- [ ] Branch `demo/ai-review-bait` from `main`, working in the copy of `src/c9/BookTracker`.
- [ ] Add one small, plausible change with real findings in it, for example a search endpoint that
      builds SQL by interpolation and skips validation:

      ```csharp
      group.MapGet("/search-raw", async (string q, BookTrackerDbContext db) =>
          await db.Books.FromSqlRaw($"SELECT * FROM Books WHERE Title LIKE '%{q}%'").ToListAsync());
      ```

      It compiles, returns an EF entity, drops the `CancellationToken`, and interpolates user input
      into SQL: four findings for the review to catch.
- [ ] Push the branch and open it as a **draft** PR: `gh pr create --draft --fill`. The workflow
      skips drafts, so nothing runs until you mark it ready on stage.

## Dry run and backups

- [ ] Do a full dry run on a second bait branch: mark it ready, wait for the comment. Note how long
      the run takes (typically 1 to 3 minutes).
- [ ] Keep that dry-run PR open. It is your first fallback: a real run with a real comment.
- [ ] Screenshot the Actions log and the PR comment from the dry run, and screen-record the whole
      sequence (ready → run → comment). Store them locally; don't rely on conference Wi-Fi to fetch them.

## On the day

- [ ] `gh auth status` is green on the demo machine; the browser is signed in to GitHub.
- [ ] The demo PR tab and the dry-run PR tab are open; the recording is queued in a player.
- [ ] Stage command ready: `gh pr ready demo/ai-review-bait`.

## After the workshop

- [ ] Close the bait PRs and delete their branches.
- [ ] Rotate or delete the API key used for the demo repo secret.
