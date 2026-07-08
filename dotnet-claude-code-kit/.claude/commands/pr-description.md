---
description: Generate a pull request description from the current branch's changes
allowed-tools: Bash(git *), Read, Grep, Glob
---

Generate a PR description for the current branch versus $ARGUMENTS (default base: main).

1. `git log <base>..HEAD --oneline` and `git diff <base>...HEAD --stat` for scope.
2. Read enough of the changed files to describe behavior, not just file names.

Output in this exact structure, in Markdown ready to paste:

## Summary
2–4 sentences: what this PR does and why.

## Changes
Grouped by project (Api / Core / Data / Tests), bullet per meaningful change.

## Testing
What was added/updated, and the command to run them.

## Breaking changes / migration notes
Say "None" explicitly if none. Include any new EF migration by name.

## Reviewer notes
Where to focus, known trade-offs, follow-ups deliberately deferred.
