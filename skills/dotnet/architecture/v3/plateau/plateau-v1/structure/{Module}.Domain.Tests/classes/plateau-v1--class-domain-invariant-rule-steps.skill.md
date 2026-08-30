---
name: plateau-v1--class-domain-invariant-rule-steps
description: Class {Rule}Steps in {Module}.Domain.Tests, in the v1 plateau
whenToUse: when adding step definitions for a new Domain-invariant Gherkin feature file
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
- Prove every scenario in `Rules/{Rule}.feature` against `{Module}.Domain`'s real implementation of the rule.

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- The step definition class holds no business logic of its own — it only translates Gherkin steps into calls against `{Module}.Domain`'s public API and assertions on the result

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one Domain rule | {Rule}Steps | EmailFormatSteps | {Rule}Steps.cs | EmailFormatSteps.cs |

# Implementation
```csharp
//Skill: class-domain-invariant-rule-steps
//Plateau: v1
//Version: 20260825140000

[Binding]
public sealed class {Rule}Steps
{
    private string _input = string.Empty;
    private ValidationResult _result = null!;

    [Given(@"the input ""(.*)""")]
    public void GivenTheInput(string input) => _input = input;

    [When(@"the {Rule} rule validates it")]
    public void WhenTheRuleValidatesIt() => _result = {Module}.{Rule}Validator.Validate(_input);

    [Then(@"the result is valid")]
    public void ThenTheResultIsValid() => Assert.True(_result.IsValid);

    [Then(@"the result is invalid with error ""(.*)""")]
    public void ThenTheResultIsInvalidWithError(string errorCode) =>
        Assert.Equal(errorCode, _result.ErrorCode);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Rules
MUST:
- Call `{Module}.Domain`'s real entry point (e.g. `{Rule}Validator`) — never re-implement the validation logic inline in a step
- Assert a specific `ErrorCode` in negative scenarios, not just a boolean
MUST NOT:
- Stub or mock the code under test inside these step definitions

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching, non-duplicated step method
- [ ] `{Rule}Steps` calls `{Module}.Domain`'s real public API, not a local re-implementation

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]
