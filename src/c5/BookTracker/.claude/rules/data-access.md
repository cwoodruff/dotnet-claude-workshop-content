---
paths:
  - "BookTracker.Data/**/*.cs"
---
# Data-access conventions

- Parameterized queries / EF LINQ only — never build SQL by string concatenation.
- Use the async EF APIs; thread the `CancellationToken` through.
- Schema changes go through EF Core migrations in `BookTracker.Data`.
