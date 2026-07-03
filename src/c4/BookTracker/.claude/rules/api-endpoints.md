---
paths:
  - "BookTracker.Api/Endpoints/**/*.cs"
---
# API endpoint conventions

- Keep endpoints thin — business logic lives in Core services, not the handler.
- Accept and return DTOs (in `BookTracker.Core/Dtos`); never return EF entities.
- Validate inputs before they reach the data layer.
- Async all the way; thread the `CancellationToken` through.
