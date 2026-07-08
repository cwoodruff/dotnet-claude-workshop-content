---
name: ef-migration
description: Create, review, and apply an EF Core migration safely. Use when the user wants to add a migration, change the database schema, add/modify entity properties, or asks about database changes.
argument-hint: "[MigrationName]"
allowed-tools: Bash(dotnet ef *), Bash(dotnet build*), Read, Grep, Glob
---

# EF Core Migration Procedure

Follow every step in order. Never skip the review step.

1. **Verify the model change first.** Confirm the entity and its `IEntityTypeConfiguration<T>` are updated and the solution builds:
   `dotnet build --nologo`
2. **Check for pending migrations** that were never applied: `dotnet ef migrations list --project src-or-Data-project`. If unapplied migrations exist, ask the user whether to apply or fold changes before adding another.
3. **Generate the migration** with a PascalCase, intent-revealing name (e.g. `AddBookRatingTable`, not `Update1`):
   `dotnet ef migrations add $ARGUMENTS --project <Data project> --startup-project <Api project>`
4. **Review the generated migration** — read the new file and check for:
   - Unintended column drops or renames (EF sees renames as drop+add; use `RenameColumn` manually only via a new migration, never by editing)
   - Missing max lengths on new string columns
   - Cascade delete behavior you did not intend
   - Data loss warnings emitted by the tool
5. **Report the review findings** to the user before applying anything.
6. **Apply only with explicit user confirmation**:
   `dotnet ef database update --project <Data project> --startup-project <Api project>`
7. **Never** hand-edit existing migration files or the model snapshot. To fix a bad unapplied migration: `dotnet ef migrations remove`, fix the model, regenerate.

## Rollback

To revert the database to a previous state: `dotnet ef database update <PreviousMigrationName>`, then `dotnet ef migrations remove` for each unapplied migration on top.
