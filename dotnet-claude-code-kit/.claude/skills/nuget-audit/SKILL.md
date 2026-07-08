---
name: nuget-audit
description: Audit NuGet dependencies for vulnerabilities, deprecated packages, and outdated versions across the solution. Use when the user asks about package health, security vulnerabilities in dependencies, or updating packages.
allowed-tools: Bash(dotnet list *), Bash(dotnet restore*), Bash(dotnet add *), Bash(dotnet build*), Read, Grep, Glob
---

# NuGet Dependency Audit

1. Restore first so metadata is current: `dotnet restore`
2. Gather three views of the solution:
   - `dotnet list package --vulnerable --include-transitive`
   - `dotnet list package --deprecated`
   - `dotnet list package --outdated`
3. Produce a report table: **Package | Current | Status (vulnerable/deprecated/outdated) | Severity/Advisory | Recommended version | Breaking-change risk**.
4. Prioritize: vulnerabilities (by severity) → deprecated → major-version-behind → minor/patch.
5. For each vulnerable package, state whether the vulnerable code path is plausibly reachable from this codebase (grep for usage) — don't just list CVEs.
6. Propose an upgrade plan in safe batches (patch/minor first). Only run `dotnet add package` upgrades the user approves, one batch at a time, building and running tests between batches.
7. Flag transitive-only vulnerabilities separately and recommend either a direct pin or upstream bump.
