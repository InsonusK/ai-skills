---
name: class-rule-steps
description: Class {Rule}Steps in BuildingBlocks.Tests, in the shared-rules plateau
whenToUse: when adding step definitions for a new pipeline-behavior technical-contract Gherkin feature file
domain: skill
type: template
plateau: shared-rules
version: 20260824150000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
---

# Goal
- Prove `ExceptionHandlingBehavior`'s technical contract: it catches an unhandled exception and returns a generic error without leaking details.

__Applied solutions:__
- [[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../../solutions/solution-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- Technical-contract-shaped: given a pipeline condition, prove the behavior's observable contract — never a business rule, since `BuildingBlocks` has none

__Applied solutions:__
- [[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../../solutions/solution-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one behavior | {Rule}Steps | ExceptionHandlingBehaviorSteps | {Rule}Steps.cs | ExceptionHandlingBehaviorSteps.cs |

# Implementation
```csharp
//Skill: class-rule-steps
//Plateau: shared-rules
//Version: 20260824150000

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

__Applied solutions:__
- [[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../../solutions/solution-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Rules
MUST:
- Exercise the real `BuildingBlocks` class directly — never a hand-written stand-in
- Assert only the observable technical contract, not implementation detail

__Applied solutions:__
- [[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../../solutions/solution-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching, non-duplicated step method
- [ ] `{Rule}Steps` exercises the real `BuildingBlocks` class

__Applied solutions:__
- [[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../../solutions/solution-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]
