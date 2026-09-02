---
name: plateau-offline-sync-service--class-module-application-rule-steps
description: Class {Rule}Steps in {Module}.Application.Tests of the plateau-offline-sync-service plateau — Reqnroll bindings driving a real handler or validator and asserting its Result
whenToUse: when writing the step definitions for a {Module}.Application.Tests feature file, or adding a scenario for a handler or validator
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
---

# Goal
- Prove every scenario in `Rules/{Rule}.feature` against `{Module}.Application`'s real handler / validator — that it shapes and dispatches correctly and returns the expected `Result`, or that the validator fails the right rule.

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `[Binding] sealed class {Rule}Steps` — Reqnroll bindings.
- Command-shaped: a command goes in, a `Result` comes out, asserted against the real handler. The step never re-implements the orchestration.
- A failure scenario asserts the exact error code/message, not just `IsSuccess == false`.
- Test doubles are trivial fakes of a dependency contract (e.g. a no-op `IPublisher`); the handler / validator under test is the real one.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Step definitions for one handler / validator | `{Rule}Steps` | `GreetSteps` | `{Rule}Steps.cs` | `GreetSteps.cs` |

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-module-application-rule-steps
// Plateau: core
// Version: 20260902000000
using MediatR;
using Reqnroll;
using {Module}.Application.Features.Greet;
using {Module}.Interfaces.Commands;
using Xunit;

namespace {Module}.Application.Tests.StepDefinitions;

[Binding]
public sealed class GreetSteps
{
    private sealed class NoopPublisher : IPublisher
    {
        public Task Publish(object n, CancellationToken ct = default) => Task.CompletedTask;
        public Task Publish<T>(T n, CancellationToken ct = default) where T : INotification => Task.CompletedTask;
    }

    private string _message = "";
    private Result<GreetResult> _result = null!;

    [Given("the greeting message {string}")]
    public void GivenMessage(string message) => _message = message;

    [When("the greet command is handled")]
    public async Task WhenHandled()
    {
        var handler = new GreetHandler(new NoopPublisher());
        _result = await handler.Handle(new GreetCommand(new(_message)), CancellationToken.None);
    }

    [Then("the rendered result is {string}")]
    public void ThenRendered(string expected) => Assert.Equal(expected, _result.Value.Rendered);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Rules
MUST:
- `[Binding] sealed class {Rule}Steps` in `{Module}.Application.Tests/StepDefinitions`.
- Call the real `{Handler}` / `{Validator}`; assert the exact `Result` or failing rule.
- Never re-implement the handler's load/dispatch/return logic in a step; never hand-compute the expected `Result`.
- Never apply several plateau templates per class.

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching step.
- [ ] The real handler/validator is invoked; assertions target its actual output.

# Unittest TestCases
- [ ] WHEN a scenario's command is valid THEN the real handler returns the expected `Result`.
- [ ] WHEN a scenario's command is invalid THEN the assertion names the exact error the real handler/validator returns.
