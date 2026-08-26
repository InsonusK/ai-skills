---
name: class-contract-shape-rule-steps
description: Class {Rule}Steps in {Module}.Interfaces.Tests, in the service-with-api plateau
whenToUse: when adding step definitions for a new contract-shape Gherkin feature file
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/class
  - plateau/service-with-api
created_by:
  - "[[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]"
---

# Goal
- Prove every scenario in `Rules/{Rule}.feature` against `{Module}.Interfaces`'s real declaration.

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- Shape-shaped, not validation-shaped: prove the contract survives round-tripping (or compares correctly), never "is this input valid"

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one contract | {Rule}Steps | ChangeCustomerEmailCommandSteps | {Rule}Steps.cs | ChangeCustomerEmailCommandSteps.cs |

# Implementation
```csharp
//Skill: class-contract-shape-rule-steps
//Plateau: service-with-api
//Version: 20260825120000

[Binding]
public sealed class {Rule}Steps
{
    private {Dto} _original = null!;
    private {Dto} _roundTripped = null!;

    [Given(@"a {Rule} value with ""(.*)""")]
    public void GivenAValue(string value) => _original = new {Dto}(value);

    [When(@"it is serialized and deserialized")]
    public void WhenRoundTripped() =>
        _roundTripped = JsonSerializer.Deserialize<{Dto}>(JsonSerializer.Serialize(_original))!;

    [Then(@"the result equals the original")]
    public void ThenEqualsOriginal() => Assert.Equal(_original, _roundTripped);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Rules
MUST:
- Assert against the real `{Dto}`/command/query type — never a hand-written stand-in shape
MUST NOT:
- Duplicate a validation check here that belongs to `{Module}.Application.Tests`/`{Module}.Domain.Tests`

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching, non-duplicated step method
- [ ] `{Rule}Steps` asserts against the real declared type, not a stand-in shape

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]
