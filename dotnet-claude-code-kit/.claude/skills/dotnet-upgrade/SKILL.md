---
name: dotnet-upgrade
description: Plan and execute an upgrade of a project or solution to a newer .NET version (e.g. .NET 8 to .NET 10). Use when the user asks to upgrade, migrate, or modernize the target framework.
argument-hint: "[target-version e.g. net10.0]"
---

# .NET Version Upgrade

Target: $ARGUMENTS (default: latest LTS if unspecified — confirm with the user).

**Phase 1 — Assess (read-only, report before changing anything):**
1. Inventory every `.csproj`: current `TargetFramework`, SDK style, package versions.
2. Check each NuGet dependency for a version compatible with the target framework.
3. Search for APIs known to be obsolete/removed between versions and for `#if NET…` conditionals.
4. Produce an upgrade plan: order of projects (leaf dependencies first — Core, then Data, then Api, then Tests), expected breaking changes, estimated risk per project.

**Phase 2 — Execute (one project at a time, with user approval of the plan):**
5. Bump `TargetFramework`, update the SDK packages, `dotnet restore`.
6. Build; fix errors, then fix **all warnings** — new analyzers surface real issues.
7. Update packages to target-compatible versions in the same commit as the framework bump for that project.
8. Run tests after each project. Do not proceed to the next project on red.

**Phase 3 — Modernize (optional, separate commits):**
9. Offer targeted modernizations the new TFM enables (collection expressions, primary constructors, `TimeProvider`, updated hosting APIs) — as suggestions, not silent rewrites.

Commit per phase with conventional-commit messages. Never combine the mechanical upgrade with behavioral changes.
