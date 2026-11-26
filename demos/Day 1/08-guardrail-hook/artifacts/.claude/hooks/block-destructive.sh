#!/usr/bin/env bash
# PreToolUse (Bash) guardrail: refuse destructive shell commands.
# Claude Code sends the tool call as JSON on stdin. Exit 2 blocks the call and the stderr
# message goes back to Claude as the reason. Exit 0 lets it run.
input=$(cat)
cmd=$(printf '%s' "$input" | jq -r '.tool_input.command // empty')

block() {
  echo "Blocked by guardrail: $1 is not allowed. Use a safer alternative (for example dotnet clean, or delete specific files)." >&2
  exit 2
}

if printf '%s' "$cmd" | grep -Eq 'rm[[:space:]]+-[A-Za-z]*r[A-Za-z]*f|rm[[:space:]]+-[A-Za-z]*f[A-Za-z]*r'; then
  block "recursive force delete ('rm -rf')"
fi
if printf '%s' "$cmd" | grep -Eq 'git[[:space:]]+reset[[:space:]]+--hard'; then
  block "discarding commits and changes ('git reset --hard')"
fi
if printf '%s' "$cmd" | grep -Eq 'git[[:space:]]+push.*(--force|-f([[:space:]]|$))'; then
  block "force push ('git push --force')"
fi
if printf '%s' "$cmd" | grep -Eq 'git[[:space:]]+clean[[:space:]]+-[A-Za-z]*f'; then
  block "deleting untracked files ('git clean -f')"
fi
if printf '%s' "$cmd" | grep -Eiq 'drop[[:space:]]+(table|database)'; then
  block "dropping a table or database"
fi
exit 0
