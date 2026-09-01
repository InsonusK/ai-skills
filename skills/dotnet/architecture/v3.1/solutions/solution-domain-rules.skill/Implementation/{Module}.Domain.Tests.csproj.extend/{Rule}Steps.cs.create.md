---
description: Step definitions proving a redirected rule's @format scenarios hold through the VO constructor / Entity method (fail-fast, DomainException)
project_name: "{Module}.Domain.Tests"
name: "{Rule}Steps.cs"
element_kind: class
change_kind: create
tags:
  - solution/domain-rules
  - element/domain-rule-steps-cs
---

# Goals
- Prove that once `{ValueObject}`/`{EntityName}` was redirected to `{Rule}.Check()`, it still rejects/accepts exactly what the shared `.feature` scenario says — the redirection changed where the condition lives, not what it does

# Implementation changes

Worked example (`Complexity`, redirected VO — the same `Complexity.feature` scenario `ComplexityRuleSteps` already proves against the rule itself, now proven again against the VO constructor):

```csharp
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

The `When` step text (`"ComplexityRules validates it"`) is identical to the one `ComplexityRuleSteps` binds in `{Module}.Domain.Rules.Tests` — Reqnroll resolves bindings independently per test project's own loaded assembly, so the same Gherkin text is free to bind to a different production call in each project without any conflict or shared state.

# Rule changes

## MUST
- Call the VO constructor / Entity method directly — never `{Rule}.Check()` from `{Module}.Domain.Rules` directly, since this class exists to prove the *redirection*, not the rule itself
- Assert `DomainException` with the expected `ErrorCode` on the invalid path
- Only bind scenarios tagged `@format` from the linked spec
- Never reimplement the condition inline instead of exercising the real VO/Entity
- Never bind an `@semantic`/`@domain`-tagged scenario — those have no VO/Entity adapter to prove

# Check list
- [ ] Every `@format`-tagged scenario for a redirected rule has a matching step class here
- [ ] Step definitions call the VO constructor / Entity method, never the rule directly
- [ ] Invalid-path assertions check `DomainException.ErrorCode`, not just "an exception was thrown"
