# Book Reviews & Ratings

## 1. Context & scope
Let readers record a star rating and written review for a book so the catalog captures opinion,
not just metadata. Ratings also give the recommendation feature (Day 2) a real signal to work from.
**In scope:** create/list/delete reviews for a book; a 1–5 star rating with review text; an aggregate
average-rating read.
**Out of scope:** per-user reviews (no user concept — reviewer is a free-text name), editing an
existing review, voting/replies, moderation, and pagination of the review list.

## 2. API contract
- `GET  /api/books/{id}/reviews` → `200 ReviewDto[]` (newest first) | `404` (book not found)
- `POST /api/books/{id}/reviews` → body `{ reviewer, rating, body }`
  → `201 ReviewDto` (+ `Location`) | `400` (rule violation, with message) | `404` (book not found)
- `DELETE /api/books/{id}/reviews/{reviewId}` → `204` | `404` (book or review not found)
- `GET  /api/books/{id}/rating` → `200 { average, count }` | `404` (book not found)

## 3. Data model & persistence
- `Review`: `Id`, `BookId` (FK → Books), `Reviewer` (string), `Rating` (int 1–5), `Body` (string),
  `CreatedOn` (`DateTimeOffset`). Many reviews per book.
- `ReviewDto` already exists in `Core/Dtos` and is the read model — endpoints never return the entity.
- `average` and `count` on `/rating` are **derived** (computed from stored rows), never stored.
- New EF Core migration `AddReviews` (FK to Books, index on `BookId`).

## 4. Business rules
- **Rating:** integer `1 <= Rating <= 5`; anything outside is a `400`.
- **Reviewer:** required, non-blank, trimmed, max 100 chars.
- **Body:** required, non-blank, trimmed, max 2000 chars.
- **CreatedOn:** set by the server at insert time — never taken from the request.
- **Average:** rounded to one decimal; `average = 0`, `count = 0` when a book has no reviews.
- Deleting a review is idempotent from the caller's view but returns `404` if it never existed.

## 5. Tests required
Valid review returns `201` with `Location` and server `CreatedOn` · `rating` of 0 and 6 rejected ·
blank `reviewer` rejected · blank `body` rejected · over-length `body` rejected · `POST` to a missing
book → `404` · list returns reviews newest-first · delete returns `204` then a second delete → `404` ·
`/rating` averages correctly and returns `0/0` for a book with no reviews.

## 6. Prohibitions & acceptance criteria
**Prohibitions:** (1) never trust client-supplied `CreatedOn` or `Id`; (2) never return the `Review`
entity across the API boundary — map to `ReviewDto`; (3) no string-concatenated SQL — parameterized
EF Core queries only; (4) validation lives in Core, not in the endpoint handler.
**Done when:** all §5 tests pass; endpoints honor §2 (correct status codes, `201`+`Location` on create,
`204` on delete); rules live in `Core`, data access in `Data`, endpoints stay thin.
