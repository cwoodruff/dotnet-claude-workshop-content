#!/bin/bash
# Stop hook — when a turn ends with uncommitted C# changes, verify formatting.
# Non-blocking on failure (exit 0) but prints the offending files so the user sees them.

CHANGED=$(git diff --name-only HEAD 2>/dev/null | grep -c '\.cs$' || true)
[ "$CHANGED" -eq 0 ] && exit 0

if ! dotnet format --verify-no-changes --no-restore > /tmp/format-check.log 2>&1; then
  echo "⚠ dotnet format found style violations in this session's changes. Run 'dotnet format' or ask Claude to fix them:"
  grep -E '\.cs' /tmp/format-check.log | head -10
fi
exit 0
