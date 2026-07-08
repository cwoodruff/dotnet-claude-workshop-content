---
name: summarize-changes
description: Summarize uncommitted changes and flag risks. Use when the user asks what changed, wants a commit message, or asks to review their diff.
---

## Current changes

!`git diff HEAD`

## Untracked files

!`git status --short`

## Instructions

Summarize the changes above in three to five bullets grouped by project (Api / Core / Data / Tests). Then flag risks: missing validation, forgotten CancellationTokens, hand-edited migration files, secrets in config, tests not updated alongside behavior changes. If the diff is empty, say there are no uncommitted changes. End with a suggested Conventional Commit message.
