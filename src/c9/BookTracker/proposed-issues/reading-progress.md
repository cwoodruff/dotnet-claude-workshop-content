# Add a Reading Progress feature

> **Offline fallback for Lab 3.** This is the issue body Claude would otherwise create on GitHub via
> the GitHub MCP server. When the network is down, writing it here *is* the "action" half of the lab.
> The feature itself is implemented in **Day 1 Lab 4** (C4).

**Type:** feature · **Labels:** enhancement, good first issue · **Complexity:** Medium (~½ day)

## Summary

BookTracker can catalog books and their reviews, but a reader can't record **how far they've gotten**
in a book. Add a Reading Progress feature so a user can track the page they're on and the status of
each book (want to read → reading → completed), with start and finish dates.

## Motivation

This is the missing half of a "book tracker": the catalog (books/authors/reviews) already exists, but
there's no per-reader progress. It's also the data surface later AI features build on (an agent tool
to update progress, an MCP tool to read it).

## Acceptance criteria

- [ ] A `ReadingProgress` record links a book to a current page and a status
      (`WantToRead`, `Reading`, `Completed`).
- [ ] `CurrentPage` is validated: `0 ≤ CurrentPage ≤ Book.TotalPages`. Out-of-range is rejected.
- [ ] Status transitions are enforced: moving to `Reading` sets a **StartedOn** date; moving to
      `Completed` sets a **FinishedOn** date and pins `CurrentPage` to `TotalPages`.
- [ ] `GET /api/books/{id}/progress` returns the current progress as a DTO (404 if the book or
      progress doesn't exist).
- [ ] `PUT /api/books/{id}/progress` updates page/status, returning the updated DTO; invalid input
      returns a validation problem.
- [ ] Endpoints stay thin and return DTOs (never EF entities); logic lives in a Core service.
- [ ] xUnit tests cover page validation and each status transition.

## Affected areas

- **BookTracker.Core** — `ReadingProgress` entity, DTOs, `IReadingProgressService` + rules.
- **BookTracker.Data** — repository, EF migration, seed.
- **BookTracker.Api** — `Endpoints/ReadingProgressEndpoints.cs`, DI wiring in `Program.cs`.
- **BookTracker.Tests** — service-rule + status-transition coverage.

## Notes

- Follow the house conventions in `.claude/rules/` (thin endpoints, DTOs, async + `CancellationToken`,
  EF LINQ, migrations in `BookTracker.Data`).
- Use the `add-api-endpoint` skill for the new routes.
