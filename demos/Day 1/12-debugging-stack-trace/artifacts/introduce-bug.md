# Demo 9b: the staged bug

A "helpful" change that lets a reader re-open a finished book. It compiles, looks reasonable in
review, and breaks the spec rule that `Completed` is terminal (`specs/reading-progress.md` §4 and §6).

**File:** `src/c4/BookTracker/BookTracker.Core/Domain/ReadingProgressRules.cs`

## Before

```csharp
                    break;

                case (ReadingStatus.WantToRead, ReadingStatus.Completed):
                    return RuleResult.Fail("Cannot skip from WantToRead to Completed — start reading first.");
```

## After (5 added lines)

```csharp
                    break;

                case (ReadingStatus.Completed, ReadingStatus.Reading):
                    // Allow re-reading a finished book.
                    current.FinishedOn = null;
                    break;

                case (ReadingStatus.WantToRead, ReadingStatus.Completed):
                    return RuleResult.Fail("Cannot skip from WantToRead to Completed — start reading first.");
```

The new case goes directly above the `(WantToRead, Completed)` case, inside the `switch ((from, to))`.

## Check it

```bash
cd src/c4/BookTracker
dotnet build BookTracker.sln          # still compiles: the bug is logical, not syntactic
dotnet test BookTracker.sln --filter "FullyQualifiedName~ReadingProgressTests" --nologo
```

Expected: 7 passed, 1 failed (`Apply_CompletedToReading_Rejected`, at `ReadingProgressTests.cs:line 94`).
The full capture is in `failing-output.txt`.

## The fix

Delete the five added lines, so `Completed → Reading` falls through to the `default:` rejection
again. Do not change the test.

## Reset after the demo

```bash
git restore BookTracker.Core/Domain/ReadingProgressRules.cs
```
