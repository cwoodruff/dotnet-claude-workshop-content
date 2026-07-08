---
description: Generate endpoint documentation for the API surface
argument-hint: "[optional resource, e.g. books]"
---

Document the API surface for $ARGUMENTS (default: all endpoint groups in the Api project).

1. Read Program.cs and every endpoint-mapping extension to enumerate routes — from source, not memory.
2. For each endpoint produce:

### `VERB /route`
- **Purpose**: one sentence
- **Auth**: required policy/role, or "anonymous"
- **Request**: body/route/query parameters with types and validation rules actually enforced in code
- **Responses**: each status code with its condition and response shape
- **Example**: one realistic curl invocation

3. Order by resource, then by verb (GET, POST, PUT, PATCH, DELETE). Flag any endpoint whose implemented status codes deviate from the conventions in .claude/rules/aspnet-api.md.

Write the result to `docs/api.md` (create the folder if needed).
