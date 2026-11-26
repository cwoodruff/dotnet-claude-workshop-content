---
paths:
  - "BookTracker.Data/**"
---
# Data-access conventions

- EF Core LINQ or `FromSqlInterpolated` only. Never build SQL from strings
  (`BookRepository.SearchByTitleAsync` is the known bad example; don't copy it).
- Read-only queries use `AsNoTracking()` and project to what the caller needs.
- Use the async EF APIs and pass the `CancellationToken` to every call.
- Schema changes go through `dotnet ef migrations add` in `BookTracker.Data`; never hand-edit a
  file under `Migrations/`.
