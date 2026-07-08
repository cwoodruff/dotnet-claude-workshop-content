---
name: async-hygiene
description: Find and fix async/await anti-patterns in C# code — blocking calls, async void, missing cancellation, fire-and-forget. Use when reviewing async code, diagnosing deadlocks or thread-pool starvation, or when the user asks about async correctness.
---

# Async/Await Hygiene Sweep

Scan the target scope (default: whole solution) for each anti-pattern, in this order, and fix with the standard remedy:

| Anti-pattern | Find | Fix |
| --- | --- | --- |
| Sync-over-async | `.Result`, `.Wait()`, `.GetAwaiter().GetResult()` | Make the caller async and await |
| `async void` | any non-event-handler | Return `Task` |
| Missing CancellationToken | async public methods without a token parameter | Add `CancellationToken cancellationToken = default` and thread it through |
| Fire-and-forget | unawaited Task-returning calls | Await, or route through a background service with error handling |
| `Task.Run` in ASP.NET request path | `Task.Run(` in Api project | Remove — the request is already on a thread-pool thread |
| Async lambda passed to sync delegate | `async` lambda to `Action<>` | Change delegate type or restructure |
| Unnecessary `async`/`await` wrapper | method whose only await is the final return | Return the Task directly (unless in a `using`/`try`) |
| Blocking I/O in async method | `File.ReadAllText`, `Thread.Sleep` inside async | Use async equivalents (`ReadAllTextAsync`, `Task.Delay`) |

Process: grep for each pattern → read the surrounding code (some hits are legitimate, e.g. `Main`, console tools) → fix true positives → `dotnet build` → report a table of what changed and any hits deliberately left alone with reasoning.
