---
description: Generate or update CHANGELOG.md from git history
argument-hint: "[since-tag-or-ref, e.g. v1.2.0]"
---

Update the changelog covering commits since $ARGUMENTS (default: the latest tag; if no tags, the last 30 commits).

1. `git log <ref>..HEAD --pretty=format:'%h %s'` and group by Conventional Commit type.
2. Render Keep-a-Changelog style sections: Added (feat), Fixed (fix), Changed (refactor/perf), Documentation (docs). Skip chore/test noise unless user-visible.
3. Rewrite each entry for a *user* of the API — behavior and impact, not internal file names. Merge related commits into one entry.
4. Call out breaking changes and new EF migrations in a **⚠ Upgrade notes** block.
5. Prepend the new section to CHANGELOG.md under an Unreleased heading (create the file with a standard header if missing). Show me the section before writing.
