---
description: Analyze the test suite and produce a prioritized list of missing tests
---

Analyze test coverage gaps for $ARGUMENTS (default: whole solution).

1. Enumerate the public surface: endpoints, Core services/domain logic, Data repositories.
2. Enumerate existing tests and map them to that surface.
3. Report:
   - **Untested critical paths** — endpoints or business rules with zero coverage, ranked by risk (writes > reads, money/data-integrity logic first)
   - **Weakly tested areas** — only happy path covered; list the missing failure/boundary cases per item
   - **Test debt** — flaky patterns, real-dependency usage in unit tests, naming violations
4. Finish with a prioritized top-10 list of specific tests to write, each with its exact proposed name (`MethodName_Scenario_ExpectedResult`).

Offer to hand the top items to the test-writer subagent.
