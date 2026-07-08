---
paths:
  - "**/*Data/**/*.cs"
  - "**/Migrations/**"
---

# EF Core Rules

- Entity configuration lives in `IEntityTypeConfiguration<T>` classes, not data annotations and not a monolithic `OnModelCreating`.
- Read-only queries use `AsNoTracking()`. Projections (`Select` into DTO shapes) are preferred over loading full entities for reads.
- No lazy loading. Eager-load explicitly with `Include`/`ThenInclude`, and question any Include deeper than two levels.
- Async EF APIs only (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`) with the caller's `CancellationToken`.
- Migrations are generated artifacts: create them with `dotnet ef migrations add <Name>` (see the `/ef-migration` skill), review them, never hand-edit them. Renaming a migration file breaks the model snapshot.
- Raw SQL only via `FromSqlInterpolated`/`ExecuteSqlInterpolated` — never `FromSqlRaw` with concatenated input.
- Every string property gets an explicit max length; every FK relationship declares its delete behavior deliberately.
- Seed data belongs in `HasData` or a dedicated seeding service, not scattered across migrations.
