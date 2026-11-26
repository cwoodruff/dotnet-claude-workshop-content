# add-api-endpoint: gotchas in this codebase

Things Claude gets wrong here unless told. The obvious steps are in SKILL.md; this file is the traps.

- **`BookRepository.SearchByTitleAsync` builds SQL by string interpolation** into `FromSqlRaw`
  (`$"SELECT * FROM Books WHERE Title LIKE '%{term}%'"`). That is a SQL-injection hole. Never copy
  it into a new query and never pass user input to it; use LINQ (`Where(b => EF.Functions.Like(...))`)
  or `FromSqlInterpolated`.
- **A new endpoint group does nothing until it is mapped.** `Program.cs` calls `app.MapBookEndpoints()`
  and `app.MapAuthorEndpoints()`; a new `Map<Resource>Endpoints` needs its own line there, and a new
  service needs its `AddScoped<IThing, Thing>()` registration.
- **Dropped `CancellationToken`.** Every service and repository method takes `CancellationToken ct`.
  Handlers must accept one and pass it on; a missing token compiles fine and silently loses
  cancellation.
- **Entities leak through `Ok(...)`.** `Book` has an `Author` navigation property; returning the
  entity serializes the graph. Map to `BookDto` / `AuthorDto` (see `BookTracker.Core/Dtos`).
- **Route groups already carry the prefix.** `MapGroup("/api/authors")` means the new route is
  `"/{id:int}/books"`, not `"/api/authors/{id}/books"`.
