---
paths:
  - "**/*Tests/**/*.cs"
  - "**/*.Tests.csproj"
---

# Testing Rules (xUnit)

- Naming: `MethodName_Scenario_ExpectedResult` (e.g. `GetBook_WhenIdMissing_Returns404`).
- Structure: Arrange / Act / Assert with a blank line between phases. One logical assertion focus per test; multiple asserts are fine when they verify one outcome.
- Data-driven cases use `[Theory]` + `[InlineData]`/`[MemberData]` rather than copy-pasted `[Fact]`s.
- Integration tests for endpoints use `WebApplicationFactory<Program>`; database-backed integration tests use Testcontainers, never a production connection string.
- Unit tests never touch a real database, filesystem, clock, or network. Inject `TimeProvider` instead of `DateTime.UtcNow`.
- Tests must be deterministic and order-independent — no shared mutable static state, no `Task.Delay` for synchronization.
- Prefer real implementations of Core domain logic over mocks; mock only at architectural boundaries (repositories, external clients).
- When fixing a bug, write the failing test first, then the fix, and keep both in the same commit.
