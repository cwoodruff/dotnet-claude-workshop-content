# CLAUDE.md — .NET Solution Guide

<!-- Template: replace bracketed placeholders with your solution's specifics.
     Keep this file under ~200 lines. Procedures belong in .claude/skills/,
     path-scoped conventions belong in .claude/rules/. -->

## Project overview

[SolutionName] is an ASP.NET Core [10] solution: [one-sentence description].

```
[SolutionName]/
  [SolutionName].Api/      # ASP.NET Core Minimal API — endpoints, DI wiring, middleware
  [SolutionName].Core/     # Domain entities, interfaces, business rules (no project dependencies)
  [SolutionName].Data/     # EF Core DbContext, configurations, migrations (depends on Core only)
  [SolutionName].Tests/    # xUnit test project
```

Dependency direction: `Api → Data → Core`. Core never references Data or Api.

## Build, test, and run

- Build: `dotnet build`
- Test: `dotnet test --nologo`
- Run API: `dotnet run --project [SolutionName].Api`
- Format check: `dotnet format --verify-no-changes`
- Add a migration: use the `/ef-migration` skill — never hand-edit migration files

Run `dotnet build` after any multi-file change and fix all warnings, not just errors. Treat analyzer warnings as failures.

## Conventions (always apply)

- Target framework: `net10.0`. Nullable reference types are **enabled**; never introduce `#nullable disable`.
- File-scoped namespaces, `var` when the type is apparent, expression-bodied members only for one-liners.
- `async`/`await` all the way down. Never call `.Result` or `.Wait()`. Accept a `CancellationToken` in every async public API method and pass it through.
- Use `record` types for DTOs and request/response models. Domain entities are classes.
- Dependency injection everywhere: constructor injection only, no service locator, no `new`-ing services.
- All EF Core queries in the Data project behind repository or service interfaces defined in Core.
- Return `Results.Problem(...)` / `TypedResults` from Minimal API endpoints — correct status codes matter (201 for creation with Location header, 204 for deletes, 404 before 400 checks where relevant).
- Logging via `ILogger<T>` with structured message templates (`"Fetched {BookId}"`), never string interpolation into the log call.

## What NOT to do

- Never concatenate user input into SQL. EF Core parameterization or `FromSqlInterpolated` only.
- Never modify files under `Migrations/` directly (a hook blocks this — use `dotnet ef migrations add`).
- Never commit secrets. Connection strings and API keys come from user-secrets, environment variables, or `appsettings.Development.json` (gitignored).
- Don't add NuGet packages without stating why; prefer BCL solutions first.
- Don't suppress warnings with pragmas; fix the cause.

## Workflow expectations

- Explore before implementing: read the relevant Core interfaces and existing endpoints before adding new ones so patterns stay consistent.
- Small, verifiable steps: after each change, build; after each feature, run the affected tests.
- Test naming: `MethodName_Scenario_ExpectedResult`.
- Commits follow Conventional Commits (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`).

## Where the rest of the guidance lives

- `.claude/rules/` — path-scoped conventions (C# style, API design, EF Core, testing, security). These load automatically when you touch matching files.
- `.claude/skills/` — step-by-step procedures (`/ef-migration`, `/add-endpoint`, `/gen-tests`, …).
- `.claude/agents/` — specialized subagents (code-reviewer, test-writer, security-auditor, …).
- `.claude/settings.json` — hooks that auto-build after writes and protect migration files.
