---
name: add-api-endpoint
description: Use when adding a new HTTP endpoint to BookTracker.Api, such as "add an endpoint",
  "add a GET for ...", "create a POST/PUT/DELETE route", or "expose ... over the API". Covers the
  route, the handler, request/response DTOs, the Core service call, and wiring it into Program.cs.
---
# Add an API endpoint

## Procedure
1. Read `references/gotchas.md` first, then one existing group in `BookTracker.Api/Endpoints/`
   (for example `BookEndpoints.cs`) so the new route matches the house style.
2. Put the request/response records in `BookTracker.Core/Dtos`. Never return an EF entity.
3. Put any logic in a Core service behind an interface in `BookTracker.Core/Services`; the handler
   only calls the service and maps the result.
4. Add the route to the resource's `Map<Resource>Endpoints` group with `TypedResults` and an explicit
   `Results<...>` union. Take a `CancellationToken` and pass it all the way down.
5. If you created a new group or service, wire it in `BookTracker.Api/Program.cs`.
6. Run `dotnet build BookTracker.sln --nologo` and fix every warning, then add a test next to the
   existing ones in `BookTracker.Tests`.
