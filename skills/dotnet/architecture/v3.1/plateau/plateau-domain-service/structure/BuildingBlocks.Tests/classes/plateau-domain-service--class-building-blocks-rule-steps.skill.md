---
name: plateau-domain-service--class-building-blocks-rule-steps
description: Class {Rule}Steps in BuildingBlocks.Tests of the plateau-domain-service plateau — Reqnroll bindings driving a real pipeline behavior and asserting the returned Result
whenToUse: when writing the step definitions for a BuildingBlocks.Tests feature file, or adding a scenario for a pipeline behavior
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
---

# Goal
- Prove every scenario in `Rules/{Rule}.feature` against a real `BuildingBlocks` pipeline behavior — drive it through a hand-built next-delegate and assert on the returned `Result`.

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `[Binding] sealed class {Rule}Steps` — Reqnroll bindings.
- Instantiate the real behavior; supply a `RequestHandlerDelegate<TResponse>` that records whether it ran and/or throws; assert on the actual `Result` status and message.
- A private sample request `record` (implementing `ICommand<Result<string>>`) stands in for a real command — the behavior is generic.
- Never re-implement the behavior in the step; never hand-compute the expected `Result`.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Step definitions for one behavior | `{Rule}Steps` | `PipelineSteps` | `{Rule}Steps.cs` | `PipelineSteps.cs` |

# Implementation
```csharp
// Skill: plateau-domain-service--class-building-blocks-rule-steps
// Plateau: core
// Version: 20260902000000
using Ardalis.Result;
using BuildingBlocks.MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Reqnroll;
using Shared.MediatR;
using Xunit;

namespace BuildingBlocks.Tests.StepDefinitions;

[Binding]
public sealed class PipelineSteps
{
    private record Req(string Value) : ICommand<Result<string>>;

    private Result<string> _result = null!;

    [When("an inner step throws an exception")]
    public async Task WhenThrows()
    {
        var behavior = new ExceptionHandlingBehavior<Req, Result<string>>(
            NullLogger<ExceptionHandlingBehavior<Req, Result<string>>>.Instance);
        _result = await behavior.Handle(new Req("x"),
            () => throw new InvalidOperationException("boom"), CancellationToken.None);
    }

    [Then("the result is an error with message {string}")]
    public void ThenError(string message)
    {
        Assert.Equal(ResultStatus.Error, _result.Status);
        Assert.Equal(message, _result.Errors.First());
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Rules
MUST:
- `[Binding] sealed class {Rule}Steps` in `BuildingBlocks.Tests/StepDefinitions`.
- Instantiate and call the real behavior; assert the exact `Result` status/message it returns.
- Never re-implement the behavior or hand-compute the expected result.
- Never apply several plateau templates per class.

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching step.
- [ ] The real behavior class is instantiated and invoked.

# Unittest TestCases
- [ ] WHEN the feature runs THEN each scenario asserts the real behavior's actual `Result`.
