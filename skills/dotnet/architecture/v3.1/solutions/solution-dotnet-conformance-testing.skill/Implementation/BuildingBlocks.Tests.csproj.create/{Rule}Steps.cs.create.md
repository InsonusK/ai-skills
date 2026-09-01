---
description: Step definitions binding a Gherkin feature file to a pipeline behavior's technical contract
project_name: "BuildingBlocks.Tests"
name: "{Rule}Steps"
element_kind: class
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/rulesteps

---

# Goals
- Prove a `BuildingBlocks` pipeline behavior's technical contract — e.g. that `ExceptionHandlingBehavior` catches an unhandled exception and returns a generic error without leaking details — as a scenario, per [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]'s "technical and architectural functions are described through Cucumber/Gherkin scenarios too" principle.

# Core Principles
- `BuildingBlocks` scenarios are technical-contract-shaped, not validation-shaped: given a pipeline condition (e.g. "the inner handler throws"), prove the behavior's observable contract (e.g. "a generic error is returned, nothing leaks") — never a business rule, since `BuildingBlocks` has none.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one behavior's scenarios | {Rule}Steps | ExceptionHandlingBehaviorSteps | {Rule}Steps.cs | ExceptionHandlingBehaviorSteps.cs |

# Implementation changes
```csharp
[Binding]
public sealed class {Rule}Steps
{
    private readonly ExceptionHandlingBehavior<TestRequest, Result> _behavior =
        new(NullLogger<ExceptionHandlingBehavior<TestRequest, Result>>.Instance);
    private Result _result = null!;

    [Given(@"the inner handler throws an exception")]
    public void GivenInnerHandlerThrows()
    {
        // captured by the When step's delegate below
    }

    [When(@"the behavior handles the request")]
    public async Task WhenBehaviorHandles() =>
        _result = await _behavior.Handle(
            new TestRequest(),
            () => throw new InvalidOperationException("boom"),
            CancellationToken.None);

    [Then(@"the result is a generic error")]
    public void ThenGenericError() => Assert.False(_result.IsSuccess);

    [Then(@"no exception details leak into the result")]
    public void ThenNoDetailsLeak() =>
        Assert.DoesNotContain("InvalidOperationException", _result.Errors.First());
}
```

# Rule changes

## MUST
- Exercise the real `BuildingBlocks` class (e.g. `ExceptionHandlingBehavior<,>`) directly — never a hand-written stand-in that mimics its contract.
  - Risk: the scenario can stay green after the real behavior's contract breaks.
  - Fix: construct and call the real class from `BuildingBlocks`.
- Assert the observable technical contract (e.g. no exception details leak), not implementation detail.
  - Risk: asserting internal detail (e.g. exact log message wording) makes the scenario brittle to harmless refactors, unrelated to the contract it exists to prove.
  - Fix: assert only what the contract promises — here, a generic error with no leaked exception detail.

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching, non-duplicated step method.
- [ ] `{Rule}Steps` exercises the real `BuildingBlocks` class.

# Unittest TestCases
- [ ] WHEN the inner handler throws THEN the behavior returns a generic error, never re-throwing.
- [ ] WHEN the inner handler throws THEN no exception message/stack trace appears in the returned `Result`.
