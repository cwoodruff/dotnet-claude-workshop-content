#!/bin/bash
# PreToolUse hook (matcher: Write|Edit) — blocks direct edits to generated or
# protected files. Migrations must go through `dotnet ef migrations add`;
# lock/props files and CI secrets configs shouldn't be hand-edited by the agent.

INPUT=$(cat)
FILE=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')
[ -z "$FILE" ] && exit 0

case "$FILE" in
  */Migrations/*)
    echo "Blocked: files under Migrations/ are generated. Use the /ef-migration skill (dotnet ef migrations add) instead of editing migration files or the model snapshot." >&2
    exit 2 ;;
  *packages.lock.json)
    echo "Blocked: packages.lock.json is generated. Change the .csproj PackageReference and run dotnet restore." >&2
    exit 2 ;;
  *appsettings.Production.json)
    echo "Blocked: production configuration must not be edited by the agent. Propose the change to the user instead." >&2
    exit 2 ;;
esac

exit 0
