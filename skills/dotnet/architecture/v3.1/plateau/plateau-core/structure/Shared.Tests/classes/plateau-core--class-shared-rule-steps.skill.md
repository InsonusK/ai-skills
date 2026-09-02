---
name: plateau-core--class-shared-rule-steps
description: Class {Rule}Steps in Shared.Tests of the plateau-core plateau — Reqnroll bindings proving one Shared primitive's scenarios against the real type
whenToUse: when writing the step definitions for a Shared.Tests feature file, or adding a scenario for a Shared primitive
domain: skill
type: template
plateau: core
version: 20260902000000
tags:
  - skill/template/class
  - plateau/core
created_by:
  - "[[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
---

# Goal
- Prove every scenario in `Rules/{Rule}.feature` against `Shared`'s real primitive / marker behavior.

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `[Binding] sealed class {Rule}Steps` — Reqnroll bindings, Cucumber-expression steps (`{string}`, `{int}`).
- Value-shaped: given one or more primitive values, prove how they compare/combine — never "is this input valid" (a module concern).
- Asserts against the real declared type from `Shared`, never a hand-written stand-in.
- xunit.v3 `Assert`; runs on Microsoft.Testing.Platform.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Step definitions for one primitive | `{Rule}Steps` | `CommandMarkersSteps` | `{Rule}Steps.cs` | `CommandMarkersSteps.cs` |

# Implementation
```csharp
// Skill: plateau-core--class-shared-rule-steps
// Plateau: core
// Version: 20260902000000
using System.Reflection;
using Reqnroll;
using Shared.MediatR;
using Xunit;

namespace Shared.Tests.StepDefinitions;

[Binding]
public sealed class CommandMarkersSteps
{
    private Type[] _markers = [];

    [When("the request markers are inspected")]
    public void WhenInspected() =>
        _markers = [typeof(ICommand), typeof(ICommand<>), typeof(IQuery<>), typeof(INotificationEvent)];

    [Then("each one is in namespace {string}")]
    public void ThenNamespace(string ns) => Assert.All(_markers, t => Assert.Equal(ns, t.Namespace));

    [Then("each one declares no instance members")]
    public void ThenNoMembers() => Assert.All(_markers, t => Assert.Empty(
        t.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)));
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Rules
MUST:
- `[Binding] sealed class {Rule}Steps` in `Shared.Tests/StepDefinitions`.
- Assert against the real `Shared` type; give every `Given/When/Then` in the feature a matching, non-duplicated step.
- Keep scenarios cross-cutting — never a module-specific concept.
- Never apply several plateau templates per class.

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching step method.
- [ ] Assertions target the real `Shared` type.

# Unittest TestCases
- [ ] WHEN the feature runs THEN every step binds and the scenario passes.
