#!/usr/bin/env bash
# PreToolUse (Bash) guardrail: block destructive commands.
input=$(cat)
cmd=$(echo "$input" | jq -r '.tool_input.command // empty')

if echo "$cmd" | grep -Eiq 'rm[[:space:]]+-rf|DROP[[:space:]]+TABLE|git[[:space:]]+push.*--force'; then
  echo "Blocked by guardrail: destructive command pattern detected." >&2
  exit 2
fi
exit 0
