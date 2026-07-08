---
name: add-endpoint
description: Add a new Minimal API endpoint following this solution's conventions. Use when the user asks to add a route, endpoint, or new API operation (GET/POST/PUT/DELETE).
argument-hint: "[HTTP-verb] [route] [short description]"
---

# Add a Minimal API Endpoint

Task: implement `$ARGUMENTS` end to end.

1. **Read the neighborhood first.** Open the existing endpoint group for this resource (or the closest one) and mirror its structure exactly: MapGroup usage, TypedResults unions, DTO records, DI parameters.
2. **Define contracts.** Request/response DTOs as `record` types. Never expose EF entities.
3. **Wire the data path.** If a needed repository/service method doesn't exist, add the interface method in Core, implement it in Data with async EF Core + `CancellationToken`, and register it if new.
4. **Implement the endpoint** with:
   - `TypedResults` union return type
   - Input validation before any database call, returning `ValidationProblem` on failure
   - Correct status codes (201 + Location on create via `CreatedAtRoute`/`Created`, 204 on delete, 404 on missing)
   - `CancellationToken` threaded through
   - Structured logging for failures only (don't log every request)
5. **Write tests** in the Tests project: at least one happy path and one failure path per endpoint, named `MethodName_Scenario_ExpectedResult`. Use `WebApplicationFactory<Program>` for integration coverage.
6. **Verify.** `dotnet build --nologo` then `dotnet test --nologo --filter` scoped to the new tests. Report results.
7. **Summarize** the new route, its status codes, and any follow-ups (auth, pagination, caching) you deliberately left out.
