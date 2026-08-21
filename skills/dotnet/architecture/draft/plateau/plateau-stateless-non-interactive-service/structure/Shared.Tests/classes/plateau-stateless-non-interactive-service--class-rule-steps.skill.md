---
name: class-rule-steps
description: Class {Rule}Steps in Shared.Tests, in the stateless-non-interactive-service plateau
whenToUse: when adding step definitions for a new Shared-primitive Gherkin feature file
domain: skill
type: template
plateau: stateless-non-interactive-service
version: 20260822120000
tags:
  - skill/template/class
  - plateau/stateless-non-interactive-service
created_by:
  - "[[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
---

# Goal
- Prove every scenario in `Rules/{Rule}.feature` against `Shared`'s real primitive/result-helper behavior.

__Applied solutions:__
- [[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../../solutions/solution-conformance-testing.skill/Implementation/Shared.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- Value-shaped: prove how primitive values compare or combine, never a business/orchestration rule

__Applied solutions:__
- [[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../../solutions/solution-conformance-testing.skill/Implementation/Shared.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one primitive | {Rule}Steps | ConflictResultSteps | {Rule}Steps.cs | ConflictResultSteps.cs |

# Implementation
```csharp
//Skill: class-rule-steps
//Plateau: stateless-non-interactive-service
//Version: 20260822120000

[Binding]
public sealed class {Rule}Steps
{
    private {Primitive} _first = null!;
    private {Primitive} _second = null!;
    private bool _areEqual;

    [Given(@"two {Rule} values ""(.*)"" and ""(.*)""")]
    public void GivenTwoValues(string a, string b)
    {
        _first = new {Primitive}(a);
        _second = new {Primitive}(b);
    }

    [When(@"they are compared")]
    public void WhenCompared() => _areEqual = _first.Equals(_second);

    [Then(@"they are considered equal")]
    public void ThenEqual() => Assert.True(_areEqual);

    [Then(@"they are considered different")]
    public void ThenDifferent() => Assert.False(_areEqual);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../../solutions/solution-conformance-testing.skill/Implementation/Shared.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Rules
MUST:
- Assert against the real `Shared` type — never a hand-written stand-in

__Applied solutions:__
- [[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../../solutions/solution-conformance-testing.skill/Implementation/Shared.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching, non-duplicated step method
- [ ] `{Rule}Steps` asserts against the real `Shared` type

__Applied solutions:__
- [[../../../../../solutions/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] - [[../../../../../solutions/solution-conformance-testing.skill/Implementation/Shared.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]
