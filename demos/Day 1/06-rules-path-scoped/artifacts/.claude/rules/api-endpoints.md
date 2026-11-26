---
paths:
  - "BookTracker.Api/Endpoints/**"
---
# API endpoint conventions

- Handlers stay thin: call a Core service, map the result, return. No EF and no business logic here.
- Accept and return DTOs from `BookTracker.Core/Dtos`; never return an EF entity.
- Use `TypedResults` with an explicit `Results<...>` union. 201 with a Location header for creates,
  204 for deletes, 404 for a missing id before any 400 checks.
- Validate input before calling the service; return `TypedResults.ValidationProblem(...)` on bad input.
- Every handler takes a `CancellationToken` and passes it through.
