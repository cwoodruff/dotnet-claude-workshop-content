# Security Rules (always loaded)

- Treat every request value (route, query, header, body) as hostile until validated.
- SQL injection: parameterized EF Core queries only. Flag any string concatenation or interpolation flowing into SQL, `Process.Start`, or file paths.
- Secrets never appear in code, config committed to git, logs, or exception messages. Point to user-secrets / environment variables instead.
- Auth checks belong on the endpoint/group (`RequireAuthorization`), not buried in handlers where a new endpoint can forget them.
- Never log PII or credentials; scrub tokens from any diagnostic output.
- Deserialization of external input uses `System.Text.Json` with explicit models — no polymorphic deserialization of untrusted payloads.
- When you spot a security issue while doing unrelated work, report it immediately in your response even if you don't fix it in that change.
