---
description: Fix a GitHub issue end to end (branch, fix, tests, commit)
argument-hint: "[issue-number]"
disable-model-invocation: true
allowed-tools: Bash(gh *), Bash(git *), Bash(dotnet *)
---

Fix GitHub issue #$ARGUMENTS:

1. `gh issue view $ARGUMENTS` — read the full description and comments.
2. Restate the problem and acceptance criteria in your own words; if ambiguous, ask before coding.
3. Create a branch: `fix/$ARGUMENTS-short-slug`.
4. Locate the defect (read before editing), write a failing test that reproduces it, then implement the minimal fix.
5. `dotnet build --nologo` and `dotnet test --nologo` — everything green.
6. Commit with `fix(scope): summary (#$ARGUMENTS)` and show the diff summary.
7. Offer to open a PR with `gh pr create` — do not push without confirmation.
