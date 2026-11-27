#!/usr/bin/env bash
# Test every tracked copy of the C# guardrail hook (block-destructive.cs).
# Pipes sample PreToolUse JSON into each copy and checks the exit code:
# 2 = blocked, 0 = allowed. Runs on macOS, Linux and Windows (Git Bash).
# Usage: tools/test-hooks.sh   (from the repo root; needs the .NET 10 SDK)
set -uo pipefail

cases=(
  "2|rm -rf bin"
  "2|rm -fr obj"
  "2|git push origin main --force"
  "2|git push -f"
  "2|sqlite3 booktracker.db \\\"DROP TABLE Books\\\""
  "2|Remove-Item -Recurse -Force .\\\\bin"
  "2|Remove-Item -Force -Recurse bin"
  "2|del /s /q bin"
  "2|git reset --hard HEAD~1"
  "2|git clean -fd"
  "2|DROP DATABASE booktracker"
  "2|rm -r -f bin"
  "2|rm --recursive --force bin"
  "2|git clean -n -f"
  "0|rm notes.txt"
  '2|Remove-Item `\n  -Recurse `\n  -Force bin'
  '2|rm \\\n  -rf bin'
  "2|rd /s /q obj"
  "0|dotnet build"
  "0|dotnet test"
  "0|git push origin main"
  "0|Remove-Item bin"
  "0|git status"
  "2|<not json>"
)

fail=0
first=""
while IFS= read -r hook; do
  [[ -z "$hook" ]] && continue
  if [[ -z "$first" ]]; then first="$hook"
  elif ! cmp -s "$first" "$hook"; then echo "DIFF  $hook differs from $first"; fail=1
  fi
  root="${hook%/.claude/hooks/block-destructive.cs}"
  echo "== $root"
  for c in "${cases[@]}"; do
    want="${c%%|*}"; cmd="${c#*|}"
    json=$(printf '{"tool_input":{"command":"%s"}}' "$cmd")
    [[ "$cmd" == "<not json>" ]] && json="not json"
    got=$(cd "$root" && printf '%s' "$json" \
      | dotnet run .claude/hooks/block-destructive.cs >/dev/null 2>&1; echo $?)
    if [[ "$got" == "$want" ]]; then echo "  ok    exit=$got  $cmd"
    else echo "  FAIL  exit=$got want=$want  $cmd"; fail=1
    fi
  done
done < <(git ls-files '*block-destructive.cs')

[[ -n "$first" ]] || { echo "No block-destructive.cs files are tracked."; exit 1; }
exit $fail
