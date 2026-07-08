---
description: Stage and commit current changes with a Conventional Commit message
disable-model-invocation: true
allowed-tools: Bash(git add *), Bash(git status *), Bash(git diff *), Bash(git commit *), Bash(git log *)
---

Commit the current work:

1. Run `git status --short` and `git diff HEAD --stat` to see what changed.
2. If changes span unrelated concerns, propose splitting into multiple commits and ask before proceeding.
3. Write a Conventional Commit message: `type(scope): summary` where type ∈ feat|fix|refactor|test|docs|chore|perf and scope is the project (api, core, data, tests) or feature area. Imperative mood, ≤72-char subject, body explaining *why* when non-obvious.
4. Stage the relevant files explicitly (never `git add -A` blindly — exclude generated artifacts and local config).
5. Commit. Show the final message. Do not push.

$ARGUMENTS
