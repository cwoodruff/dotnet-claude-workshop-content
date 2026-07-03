---
name: add-api-endpoint
description: Use when adding a new HTTP endpoint to BookTracker.Api — creating the route,
  handler, request/response DTOs, and wiring it into the Minimal API. Triggers on requests like
  "add an endpoint", "expose a new route", "create a GET/POST for ...".
---
# Add an API endpoint

## Procedure
1. Read `references/endpoint-pattern.md` and an existing endpoint to match the house style.
2. Define request/response DTOs in `BookTracker.Core/Dtos`.
3. Add the handler and map the route in `BookTracker.Api/Endpoints`.
4. Build, then add a test mirroring `BookTracker.Tests`.

## Gotchas
- Never return an EF entity — map to a DTO.
- Validate inputs before touching the data layer.
- Keep the handler thin; put logic in a Core service.
