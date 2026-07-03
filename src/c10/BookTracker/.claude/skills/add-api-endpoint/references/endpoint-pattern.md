# BookTracker endpoint pattern

The canonical BookTracker endpoint is a thin Minimal API handler that delegates to a Core service
and returns a DTO. Match this shape.

## Shape

- **One `MapGroup` per resource**, in a `static class` with a `Map<Resource>Endpoints` extension
  method on `IEndpointRouteBuilder`. Tag the group (`.WithTags("...")`).
- **Handler → Core service.** The handler resolves an `I<Resource>Service` from DI, awaits it, and
  maps the result to a response. No EF, no business logic in the handler.
- **Return a `*Dto`** (or a collection of them), never an EF entity. DTOs live in
  `BookTracker.Core/Dtos`.
- **Always take `CancellationToken`** and thread it through to the service.
- **Validate input** before calling the data layer; return `TypedResults.ValidationProblem(...)` on
  bad input.
- **Use `TypedResults`** with an explicit `Results<...>` union: `Ok`/`NotFound`/`Created`/`NoContent`
  as appropriate.

## Live example

See `GET /api/books` and `GET /api/books/{id}` in
`BookTracker.Api/Endpoints/BookEndpoints.cs` — a clean group that returns `BookDto`, threads the
`CancellationToken`, and returns `NotFound` for a missing id:

```csharp
var group = app.MapGroup("/api/books").WithTags("Books");

group.MapGet("/{id:int}", async Task<Results<Ok<BookDto>, NotFound>> (
    int id, IBookService service, CancellationToken ct) =>
{
    var book = await service.GetByIdAsync(id, ct);
    return book is null ? TypedResults.NotFound() : TypedResults.Ok(book);
});
```

Wire the group in `Program.cs` (`app.Map<Resource>Endpoints();`) and register the service there too.
