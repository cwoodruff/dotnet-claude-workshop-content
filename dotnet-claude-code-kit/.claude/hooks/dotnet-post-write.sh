#!/bin/bash
# PostToolUse hook (matcher: Write|Edit) — builds after any C#-relevant file change
# and surfaces errors/warnings back into Claude's context so it can self-correct.
# Requires: jq, dotnet SDK on PATH. On Windows, run Claude Code from Git Bash.

INPUT=$(cat)
FILE=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')

# Only react to files that affect compilation
case "$FILE" in
  *.cs|*.csproj|*.sln|*.props|*.targets|*.razor) ;;
  *) exit 0 ;;
esac

OUTPUT=$(dotnet build --nologo --no-restore -v q 2>&1)
STATUS=$?

if [ $STATUS -ne 0 ]; then
  echo "BUILD FAILED after editing $FILE:" >&2
  echo "$OUTPUT" | grep -E '(error|warning)' | head -30 >&2
  exit 2   # exit 2 feeds stderr back to Claude so it self-corrects
fi

WARNINGS=$(echo "$OUTPUT" | grep -c 'warning' || true)
if [ "$WARNINGS" -gt 0 ]; then
  echo "Build succeeded with $WARNINGS warning(s) after editing $FILE:" >&2
  echo "$OUTPUT" | grep 'warning' | head -15 >&2
  exit 2   # warnings are treated as failures per CLAUDE.md
fi

exit 0
