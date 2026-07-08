# BookTracker — Checkpoint c0 (Starter)

The sample application for the **.NET AI with Claude Workshop**. An ASP.NET Core 10 Minimal API
for tracking books, backed by EF Core 10 and SQLite. Every workshop lab evolves this same codebase.

> **c0 — starter checkpoint.** *Day 1 · Section 1 — Foundations: the Claude Code CLI.*
> This is the unmodified starting state — the app you meet before you've configured Claude Code for it.
> It is deliberately *complete enough to feel real* and *flawed enough to teach* — see
> [Teaching gaps](#teaching-gaps).

## Where this fits

`c0` is the baseline. Nothing in `.claude/`, no `CLAUDE.md` worth the name, no AI features — just a
working Books + Authors API with intentional problems. Everything else in `src/` (`c1`–`c10`) is this
same solution after another lab. If you ever want a clean slate, come back here.

## Stack

- **.NET 10** · **ASP.NET Core Minimal API** (no controllers)
- **EF Core 10** · **SQLite**
- **xUnit** for tests

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Claude Code](https://claude.com/claude-code) — the tool you'll drive this codebase with all of Day 1

## Getting started

```bash
cd src/c0/BookTracker

dotnet build BookTracker.sln       # build all four projects
dotnet test BookTracker.sln        # run the test suite
dotnet run --project BookTracker.Api   # http://localhost:5255  (OpenAPI at /openapi)
```

The database is created, migrated, and seeded automatically on first run — no manual setup needed.
By default it lives in a local `booktracker.db` SQLite file (gitignored). The connection string is
in `BookTracker.Api/appsettings.json` under `ConnectionStrings:BookTracker`.

## Project layout

```text
BookTracker.sln
CLAUDE.md                     # minimal at start — you write a real one in Day 1, Lab 1 (c1)
BookTracker.Core/             # domain entities, DTOs, interfaces, services  (depends on nothing)
BookTracker.Data/             # EF Core DbContext, repository, migrations, seed  (depends on Core)
BookTracker.Api/              # Minimal API endpoints + DI wiring  (depends on Core + Data)
BookTracker.Tests/            # xUnit tests  (initially sparse)
```

Dependency direction: `Core` ← `Data` ← `Api`, all referenced by `Tests`. `Core` defines
`IBookRepository` / `IAuthorRepository` *ports* implemented in `Data`, so the domain layer stays
free of EF Core.

## Data model

A book belongs to exactly one author; an author can have many books (one-to-many). `Author` is its
own table, and `Book` carries an `AuthorId` foreign key. Book responses embed the author:

```json
{ "id": 2, "title": "Clean Code", "author": { "id": 2, "name": "Robert C. Martin" }, "totalPages": 464 }
```

## API endpoints

**Books** — under `/api/books`:

| Method | Route                 | Description                                            |
|--------|-----------------------|--------------------------------------------------------|
| GET    | `/api/books`          | List all books (each with its author)                  |
| GET    | `/api/books/{id}`     | Get a book by id (404 if missing)                      |
| GET    | `/api/books/search?q=`| Search books by title                                  |
| POST   | `/api/books`          | Create a book (201; 400 if `authorId` doesn't exist)   |
| PUT    | `/api/books/{id}`     | Update a book (404 if missing; 400 if bad `authorId`)  |
| DELETE | `/api/books/{id}`     | Delete a book (204, or 404)                            |

**Authors** — under `/api/authors`:

| Method | Route                 | Description                                            |
|--------|-----------------------|--------------------------------------------------------|
| GET    | `/api/authors`        | List all authors                                       |
| GET    | `/api/authors/{id}`   | Get an author by id (404 if missing)                   |
| POST   | `/api/authors`        | Create an author (returns 201)                         |
| PUT    | `/api/authors/{id}`   | Update an author (404 if missing)                      |
| DELETE | `/api/authors/{id}`   | Delete an author (204; 404 if missing; 409 if it has books) |

A book references an author by `authorId`, which must already exist — create the author first.

Example:

```bash
curl http://localhost:5255/api/books
curl "http://localhost:5255/api/books/search?q=Clean"

# Create an author, then a book that references it
curl -X POST http://localhost:5255/api/authors \
  -H "Content-Type: application/json" \
  -d '{"name":"Martin Fowler"}'

curl -X POST http://localhost:5255/api/books \
  -H "Content-Type: application/json" \
  -d '{"title":"Refactoring","authorId":6,"isbn":"9780134757599","totalPages":448,"genre":"Software"}'
```

## Database migrations

Migrations live in `BookTracker.Data/Migrations` and are applied automatically at startup.
To add one (requires the EF tools: `dotnet tool install --global dotnet-ef`):

```bash
dotnet ef migrations add <Name> \
  --project BookTracker.Data \
  --startup-project BookTracker.Api
```

## Conventions

These are the rules the codebase expects (you formalize them in `CLAUDE.md` and path-scoped rules
during Day 1):

- DTOs (records) live in `BookTracker.Core/Dtos`. Endpoints **never** return EF entities.
- Endpoints stay thin: parse → validate → call a service → map → typed `Results<...>`.
- All data access is `async`/`await`. Thread the `CancellationToken` through.
- Parameterized queries only — no string-concatenated SQL.
- Every new endpoint group's `Map…Endpoints` method is wired in `Program.cs`.
- Schema changes go through EF Core migrations in `BookTracker.Data`.

## Teaching gaps

The starter ships with **intentional flaws** that attendees discover and fix across the Day 1 labs
(validation holes, an SQL-injection vector, endpoints returning EF entities, missing observability).
Finding them is the exercise — they are **not bugs to be reported**, and the Reading Progress feature
is intentionally absent because it's what you build in **Day 1, Lab 4** (`c4`).

## Workshop resources

- **Guide:** [dotnetclaude.com](https://dotnetclaude.com) — Day 1, Section 1 (Foundations).
- **Deck:** `decks/Day 1/Section1-Foundations-Claude-Code-CLI.pptx`
- **Claude Code docs:** <https://docs.claude.com/en/docs/claude-code>
- **Next checkpoint:** [`c1`](../../c1/BookTracker/README.md) — give the app real project memory with a `CLAUDE.md`.
