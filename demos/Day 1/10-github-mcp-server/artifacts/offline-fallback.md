# Demo 8 offline fallback: same pattern, no GitHub

Use this when GitHub, the MCP endpoint, or the GitHub token fails. The lesson stays the same: a
**query** (read something) followed by an **action** (write something), with Claude asking before
it writes. Only the transport changes: local files instead of the GitHub MCP server. Claude Code
still needs its own model connection; if that is down too, play the recorded backup.

Run from `src/c2/BookTracker`, the same folder as the live demo.

## 1. Query half (about 3 minutes)

Instead of listing GitHub issues, ground Claude in local history:

```text
Run git log --oneline -20 and read the BookTracker solution. What has been happening in this
codebase, and what would be a good first contribution? Cite the files you used.
```

If `git log` is thin (a fresh clone of the content repo has little history), point Claude at the
sample history instead: `src/c3/BookTracker/proposed-issues/recent-history.txt`.

Narrate: "Same question I asked the issue tracker. The answer comes from the repo itself."

## 2. Action half (about 4 minutes)

Instead of creating a GitHub issue, have Claude write the issue to a file:

```text
Write a GitHub-style issue to proposed-issues/reading-progress.md titled "Add Reading Progress
tracking": track current page, total pages, and reading status (reading / completed /
want-to-read) with start and finish dates. Include acceptance criteria, the affected files, and a
complexity estimate.
```

Expected: Claude proposes the file, the **permission prompt fires before the write**, you pause on
it, then approve. The finished reference version is
`src/c3/BookTracker/proposed-issues/reading-progress.md`; open it next to Claude's draft.

## 3. Land it

"Same query then action. The only thing that changed is where the data lives. In Lab 3 you can do
either; this file is also what Lab 4 builds from."

## Reset

```bash
rm -r proposed-issues    # c2 doesn't ship this folder
```
