# Reading Progress

## 1. Context & scope
Track a reader's progress through a book: current page, reading status, and start/finish dates.
**In scope:** one progress record per book; update + read endpoints; business rules.
**Out of scope:** per-user progress (no user concept), history/audit, partial-page tracking.

## 2. API contract
- `GET  /api/books/{id}/reading-progress` → `200 ReadingProgressDto` | `404` (no record / no book)
- `PUT  /api/books/{id}/reading-progress` → body `{ currentPage, status }`
  → `200 ReadingProgressDto` | `400` (rule violation, with message) | `404` (book not found)

## 3. Data model & persistence
- `ReadingProgress`: one row per `BookId` (unique). `TotalPages` is **not stored** — it derives from
  `Book.TotalPages`. Status stored as a string; dates as `DateOnly?`.
- New EF Core migration `AddReadingProgress` (FK to Books, unique index on BookId).

## 4. Business rules
- **Page:** `0 <= CurrentPage <= Book.TotalPages` (inclusive).
- **Status state machine (forward-only):** `WantToRead → Reading → Completed`; same→same is a no-op.
- **Dates:** entering `Reading` sets `StartedOn` (today, if unset); entering `Completed` sets
  `FinishedOn` (today) and forces `CurrentPage = TotalPages`; `FinishedOn >= StartedOn`.

## 5. Tests required
Valid in-range update · page over/under bounds rejected · `WantToRead→Reading` sets `StartedOn` ·
`Reading→Completed` sets `FinishedOn` and fills pages · skip rejected · **`Completed→Reading`
rejected** · GET 404 when no record.

## 6. Prohibitions & acceptance criteria
**Prohibitions:** (1) no skipping `WantToRead → Completed`; (2) `Completed` is **terminal** — no
transition out of it; (3) `CurrentPage` may never exceed `TotalPages` or be negative.
**Done when:** all §5 tests pass; endpoints honor §2; rules live in `Core/Domain`, data access in Data.
