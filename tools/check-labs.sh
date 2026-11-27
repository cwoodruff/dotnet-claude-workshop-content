#!/usr/bin/env bash
# Build and test every lab folder (src/c0 .. src/c10) in the content repo, and optionally show what
# changes between neighbouring folders. Run it from the content repo root:
#
#   tools/check-labs.sh           # build + test all eleven folders
#   tools/check-labs.sh --diff    # also list the files that differ between cN and cN+1
#
# Warnings count as failures, because attendees see them. Tests that need Docker (Qdrant, SQL
# Server) skip themselves when Docker isn't running; they show up in the "skipped" count.
set -uo pipefail

show_diff=false
[[ "${1:-}" = "--diff" ]] && show_diff=true

if [[ ! -d src/c0/BookTracker ]]; then
  echo "Run this from the content repo root (src/c0/BookTracker not found)." >&2
  exit 1
fi

export DOTNET_NOLOGO=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
failed=0

printf '%-5s %-8s %-9s %s\n' "lab" "build" "warnings" "tests"
for n in $(seq 0 10); do
  dir="src/c$n/BookTracker"
  build_out=$(dotnet build "$dir/BookTracker.sln" --nologo --no-incremental -v q 2>&1)
  build_status=$?
  warnings=$(printf '%s\n' "$build_out" | grep -Eo '[0-9]+ Warning\(s\)' | tail -1 | grep -Eo '^[0-9]+')
  warnings=${warnings:-?}

  tests="not run"
  if [[ $build_status -eq 0 ]]; then
    test_out=$(dotnet test "$dir/BookTracker.sln" --nologo --no-build 2>&1)
    test_status=$?
    tests=$(printf '%s\n' "$test_out" | grep -E 'Passed!|Failed!' | sed -E 's/^(Passed|Failed)! +- //; s/, Duration.*//' | head -1)
    [[ $test_status -ne 0 ]] && failed=1
  fi

  [[ $build_status -ne 0 ]] && failed=1
  [[ "$warnings" != "0" ]] && failed=1
  printf '%-5s %-8s %-9s %s\n' "c$n" "$([[ $build_status -eq 0 ]] && echo ok || echo FAILED)" "$warnings" "$tests"
done

if $show_diff; then
  for n in $(seq 0 9); do
    echo
    echo "== c$n -> c$((n + 1))"
    diff -rq -x bin -x obj -x '*.db*' "src/c$n/BookTracker" "src/c$((n + 1))/BookTracker" \
      | sed -E "s|src/c[0-9]+/BookTracker/?||g"
  done
fi

exit $failed
