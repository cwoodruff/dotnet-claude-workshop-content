---
name: gen-tests
description: Generate an xUnit test suite for a class, endpoint, or file. Use when the user asks for tests, test coverage, or to test something they just wrote.
argument-hint: "[type-or-file-to-test]"
---

# Generate xUnit Tests

Target: $ARGUMENTS

1. **Read the target and its dependencies** before writing anything. Identify: public surface, branches, edge cases (null/empty, boundary values, cancellation, concurrency), and failure modes (not found, validation, exceptions).
2. **Enumerate the test cases first** as a checklist in your response — happy paths, each failure path, each boundary — and note which are unit vs. integration.
3. **Write the tests:**
   - Naming: `MethodName_Scenario_ExpectedResult`
   - Arrange / Act / Assert with blank-line separation
   - `[Theory]` + `[InlineData]` for parametric cases
   - Mock only at boundaries (repository interfaces, external clients); use real Core logic
   - Inject `TimeProvider` / fakes rather than touching real time, filesystem, or network
   - Async tests return `Task`, never `async void`
4. **Match existing test project conventions** — fixture patterns, builder/mother helpers, assertion library already in use. Do not introduce a new mocking or assertion library without asking.
5. **Run the new tests** (`dotnet test --nologo --filter <TestClassName>`) and fix failures until green.
6. **Report** the coverage achieved and any behaviors you could not test without refactoring (and why).
