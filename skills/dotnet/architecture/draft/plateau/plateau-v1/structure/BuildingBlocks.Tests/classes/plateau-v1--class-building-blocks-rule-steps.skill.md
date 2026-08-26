---
name: plateau-v1--class-building-blocks-rule-steps
description: Class {Rule}Steps in BuildingBlocks.Tests, in the v1 plateau
whenToUse: when adding step definitions for a new pipeline-behavior technical-contract Gherkin feature file
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
---

# Goal
- Prove each pipeline behavior's technical contract via its own `{Rule}Steps` class: `ExceptionHandlingBehaviorSteps` proves an unhandled exception is converted to a safe, generic error result; `ValidationBehaviorSteps` proves a failing validator short-circuits the pipeline with `ResultStatus.Invalid` while an empty validator set reaches the next behavior.

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- Technical-contract-shaped: given a pipeline condition, prove the behavior's observable contract — never a business rule, since `BuildingBlocks` has none

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one behavior | {Rule}Steps | ExceptionHandlingBehaviorSteps | {Rule}Steps.cs | ExceptionHandlingBehaviorSteps.cs |
| Step definitions for one behavior | {Rule}Steps | ValidationBehaviorSteps | {Rule}Steps.cs | ValidationBehaviorSteps.cs |

# Implementation
```csharp
//Skill: class-building-blocks-rule-steps
//Plateau: v1
//Version: 20260825140000

[Binding]
public sealed class {Rule}Steps
{
    private {Rule}Behavior<{Command}, Result<string>>? _behavior;
    private Result<string>? _result;

    [Given("a MediatR pipeline with {Rule}Behavior")]
    public void GivenAPipeline()
    {
        // arrange the behavior under test; no inner-handler call yet
    }

    [When("<the condition this behavior reacts to>")]
    public async Task WhenCondition() =>
        _result = await _behavior!.Handle(
            new {Command}(),
            () => Task.FromResult(Result.Success("ok")),
            CancellationToken.None);

    [Then("<the behavior's observable contract>")]
    public void ThenContract() => Assert.False(_result!.IsSuccess);

    private record {Command} : ICommand<Result<string>>;
}
```

Two concrete instances of this pattern exist in this plateau: `ExceptionHandlingBehaviorSteps` (drives a real `ExceptionHandlingBehavior<GreetCommand, Result<string>>`, its own private `GreetCommand`, asserting the caught exception becomes `Result<string>` failure with the generic "An unexpected error occurred..." message) and `ValidationBehaviorSteps` (drives a real `ValidationBehavior<DummyCommand, Result<string>>` with either a failing `FluentValidation` validator or none registered, its own private `DummyCommand`/`FailingValidator`, asserting `ResultStatus.Invalid` short-circuits the pipeline versus an empty validator set reaching `next()`).

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Rules
MUST:
- Exercise the real `BuildingBlocks` class directly — never a hand-written stand-in
- Assert only the observable technical contract, not implementation detail

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching, non-duplicated step method
- [ ] `{Rule}Steps` exercises the real `BuildingBlocks` class

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]
