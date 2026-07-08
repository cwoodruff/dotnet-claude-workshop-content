---
paths:
  - "**/*Api/**/*.cs"
  - "**/Endpoints/**/*.cs"
  - "**/Program.cs"
---

# ASP.NET Core Minimal API Rules

- Group endpoints with `MapGroup` per resource (e.g. `/books`) and register groups from extension methods (`app.MapBookEndpoints()`), keeping `Program.cs` thin.
- Use `TypedResults`, not bare `Results`, so endpoint signatures declare `Results<Ok<T>, NotFound>`-style unions and OpenAPI stays accurate.
- Status codes: 201 + `Location` header on create, 204 on successful delete, 404 for missing resources, 400 with `ValidationProblem` for invalid input, 409 for conflicts.
- Validate all request models before touching the database. Missing validation is a defect, not a nice-to-have.
- Every endpoint that queries or writes data accepts a `CancellationToken` and passes it to EF Core / downstream calls.
- Request/response DTOs are `record` types in the Api project; never expose EF entities directly from endpoints.
- Errors return RFC 7807 problem details; never leak exception messages or stack traces to clients.
- Pagination on any collection endpoint that can grow unbounded: `page`/`pageSize` with a sane max.
