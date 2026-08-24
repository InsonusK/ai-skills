---
name: class-domain-rule-steps
description: Step definitions proving a redirected rule's @format scenarios hold through the VO constructor / Entity method, in the shared-rules plateau
whenToUse: when a VO or Entity condition was redirected to a centralized rule and its shared @format scenarios need proving again at this adapter
domain: skill
type: template
plateau: shared-rules
version: 20260824150000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Prove that once `{ValueObject}`/`{EntityName}` was redirected to `{Rule}.Check()`, it still rejects/accepts exactly what the shared `.feature` scenario says

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Implementation
```csharp
//Skill: class-domain-rule-steps
//Plateau: shared-rules
//Version: 20260824150000

[Binding]
public sealed class ComplexityVoSteps
{
    private int _input;
    private DomainException? _thrown;

    [Given(@"a complexity value of (-?\d+)")]
    public void GivenAComplexityValueOf(int value) => _input = value;

    [When(@"ComplexityRules validates it")]
    public void WhenComplexityRulesValidatesIt()
        => _thrown = Record.Exception(() => new Complexity(_input)) as DomainException;

    [Then(@"the result is valid")]
    public void ThenTheResultIsValid() => Assert.Null(_thrown);

    [Then(@"the result is invalid with error code ""(.*)""")]
    public void ThenTheResultIsInvalidWithErrorCode(string errorCode)
    {
        Assert.NotNull(_thrown);
        Assert.Equal(errorCode, _thrown!.ErrorCode);
    }
}
```

The `When` step text is identical to the one `{Rule}RuleSteps` binds in `{Module}.Domain.Rules.Tests` — Reqnroll resolves bindings independently per test project's own loaded assembly, so the same Gherkin text is free to bind to a different production call in each project.

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Rules
MUST:
- Call the VO constructor / Entity method directly — never `{Rule}.Check()` from `{Module}.Domain.Rules` directly
- Assert `DomainException` with the expected `ErrorCode` on the invalid path
- Only bind scenarios tagged `@format`
MUST NOT:
- Bind an `@semantic`/`@domain`-tagged scenario — those belong to `{Module}.Application.Tests`

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Check list
- [ ] Every `@format`-tagged scenario for a redirected rule has a matching step class here
- [ ] Step definitions call the VO constructor / Entity method, never the rule directly

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]
