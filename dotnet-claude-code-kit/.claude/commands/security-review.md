---
description: Security-focused code review for ASP.NET Core
argument-hint: "[optional path, or diff for uncommitted changes only]"
---

Perform a security review of $ARGUMENTS (default scope: uncommitted changes, falling back to the whole Api and Data projects if the working tree is clean).

Check for: SQL injection (string building into queries), missing input validation on request models and route/query parameters, authentication/authorization gaps (endpoints missing RequireAuthorization, IDOR patterns), sensitive data exposure (secrets in code/config/logs, EF entities serialized to clients, stack traces in responses), missing rate limiting on write endpoints, CSRF exposure on cookie-authenticated endpoints, and insecure deserialization of external payloads.

Output a structured report:

| Severity | Location | Description | Remediation |
| --- | --- | --- | --- |

Severity scale: Critical / High / Medium / Low. After the table, list the categories you checked that came back clean. Do not fix anything — report only.
