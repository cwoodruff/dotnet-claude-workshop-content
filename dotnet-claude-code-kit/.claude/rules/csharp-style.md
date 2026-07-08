---
paths:
  - "**/*.cs"
---

# C# Style Rules

- File-scoped namespaces (`namespace Foo;`), one top-level type per file, filename matches type name.
- Nullable reference types stay enabled. Resolve warnings by fixing the design, not by sprinkling `!`. A justified null-forgiving operator needs a trailing comment explaining why.
- Prefer pattern matching (`is`, `switch` expressions) over chained `if`/`else` type checks.
- Use collection expressions (`[]`) and target-typed `new()` where the type is apparent.
- `readonly` fields wherever possible; prefer `init` setters over mutable properties on models.
- Guard clauses with `ArgumentNullException.ThrowIfNull(x)` / `ArgumentException.ThrowIfNullOrWhiteSpace(x)` at public API boundaries.
- LINQ: prefer method syntax; never materialize (`ToList()`) mid-pipeline without a reason; be explicit when a query is intentionally executed.
- String comparisons involving user or external data specify a `StringComparison`.
- No regions. No commented-out code — delete it; git remembers.
