---
name: plateau-domain-service--class-module-interfaces-rule-steps
description: Class {Rule}Steps in {Module}.Interfaces.Tests of the plateau-domain-service plateau — Reqnroll bindings pinning a public contract's shape against the real declared type
whenToUse: when writing the step definitions for a {Module}.Interfaces.Tests feature file, or adding a scenario that pins a contract's marker or fields
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
- Prove every scenario in `Rules/{Rule}.feature` against the module's real public contract types — a command implements the right marker, a DTO carries the expected fields.

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `[Binding] sealed class {Rule}Steps` — Reqnroll bindings.
- Contract-shaped: construct the declared type, assert it is assignable to the right marker / exposes the right members.
- References only the module's `Interfaces` — never a handler, validator, or domain type.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Step definitions for one contract group | `{Rule}Steps` | `ContractsSteps` | `{Rule}Steps.cs` | `ContractsSteps.cs` |

# Implementation
```csharp
// Skill: plateau-domain-service--class-module-interfaces-rule-steps
// Plateau: core
// Version: 20260902000000
using Ardalis.Result;
using Reqnroll;
using {Module}.Interfaces.Commands;
using {Module}.Interfaces.Events;
using Shared.MediatR;
using Xunit;

namespace {Module}.Interfaces.Tests.StepDefinitions;

[Binding]
public sealed class ContractsSteps
{
    private CreateTaskCommand? _command;

    [When("a CreateTaskCommand is created with title {string}")]
    public void WhenCreated(string title) => _command = new CreateTaskCommand(title, 1);

    [Then("it implements ICommand of Result of CreateTaskResult")]
    public void ThenShape() => Assert.IsAssignableFrom<ICommand<Result<CreateTaskResult>>>(_command);

    [Then("TaskClosed implements INotificationEvent")]
    public void ThenEvent() => Assert.True(typeof(INotificationEvent).IsAssignableFrom(typeof(TaskClosed)));
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Rules
MUST:
- `[Binding] sealed class {Rule}Steps` in `{Module}.Interfaces.Tests/StepDefinitions`.
- Construct and inspect the real contract type; give every `Given/When/Then` a matching step.
- Never reference `{Module}.Application` or `{Module}.Domain`.
- Never apply several plateau templates per class.

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching step.
- [ ] Only `{Module}.Interfaces` types are referenced.

# Unittest TestCases
- [ ] WHEN the feature runs THEN each scenario constructs a real contract type and its assertion passes.
