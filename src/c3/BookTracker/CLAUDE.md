# BookTracker

ASP.NET Core 10 **Minimal API** for tracking books and reading progress, backed by EF Core 10.
Sample app for the *.NET AI with Claude* workshop — it evolves across every lab.

## Stack

- C# / .NET 10 · ASP.NET Core 10 Minimal API (no controllers)
- EF Core 10 · SQLite (dev) — see `appsettings.json` for the connection string
- xUnit for tests

## Commands

```bash
dotnet build                                   # build the solution
dotnet test                                    # run the xUnit tests
dotnet run --project BookTracker.Api           # run the API (OpenAPI at /openapi)

# migrations (EF Core CLI; run from src/BookTracker)
dotnet ef migrations add <Name> --project BookTracker.Data --startup-project BookTracker.Api
dotnet ef database update      --project BookTracker.Data --startup-project BookTracker.Api
```

## Architecture

Four projects, with a deliberate dependency direction:

- **BookTracker.Core** — entities, DTOs, service interfaces, domain logic. Depends on nothing.
- **BookTracker.Data** — EF Core `DbContext`, migrations, seed, service implementations. → Core.
- **BookTracker.Api** — Minimal API endpoints, DI, request/response mapping. → Core, Data.
- **BookTracker.Tests** — xUnit. → all of the above.

Endpoints are grouped under `BookTracker.Api/Endpoints/`. Data access lives in
`BookTracker.Data`. Keep the dependency direction intact.

## Conventions

Path-scoped conventions live in `.claude/rules/` (loaded automatically when Claude works in the
matching files): `api-endpoints.md`, `data-access.md`.

## Notes

- The starter has intentional gaps (missing validation, an entity-returning endpoint, etc.) — they're
  there to find and fix, not to copy.
