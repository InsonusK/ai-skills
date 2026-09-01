---
description: Step definitions proving one rule's own IsValid()/Check() directly, against every scenario in its .feature file regardless of classification tag
project_name: "{Module}.Domain.Rules.Tests"
name: "{Rule}RuleSteps.cs"
element_kind: class
change_kind: create
tags:
  - solution/domain-rules
  - element/rule-rule-steps-cs
---

# Goals
- Prove `{Rule}`'s own predicate and `Check()` are correct, independent of any adapter that later redirects to it

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | ------------------ | --------- |
| Step definitions for one rule's own logic | `{Rule}RuleSteps` | `ComplexityRuleSteps` | `{Rule}RuleSteps.cs` | `ComplexityRuleSteps.cs` |

# Implementation changes

Worked example (`ComplexityRules`, Format-classified — named wrapper, single value):

```csharp
[Binding]
public sealed class ComplexityRuleSteps
{
    private SoftComplexity _value = null!;
    private ValidationResult _result = null!;

    [Given(@"a complexity value of (-?\d+)")]
    public void GivenAComplexityValueOf(int value) => _value = new SoftComplexity(value);

    [When(@"ComplexityRules validates it")]
    public void WhenComplexityRulesValidatesIt() => _result = _value.Check();

    [Then(@"the result is valid")]
    public void ThenTheResultIsValid() => Assert.True(_result.IsValid);

    [Then(@"the result is invalid with error code ""(.*)""")]
    public void ThenTheResultIsInvalidWithErrorCode(string errorCode) =>
        Assert.Contains(_result.Errors, e => e.ErrorCode == errorCode);
}
```

Worked example (`AccountWithdrawalRule`, Domain-classified — anonymous tuple, already-loaded raw values; the `Given`/`And` steps assemble the tuple directly, no repository, no async):

```csharp
[Binding]
public sealed class AccountWithdrawalRuleSteps
{
    private decimal _balance;
    private decimal _amount;
    private ValidationResult _result = null!;

    [Given(@"an account balance of (\d+)")]
    public void GivenAnAccountBalanceOf(decimal balance) => _balance = balance;

    [Given(@"a withdrawal amount of (\d+)")]
    public void GivenAWithdrawalAmountOf(decimal amount) => _amount = amount;

    [When(@"AccountWithdrawalRule validates it")]
    public void WhenAccountWithdrawalRuleValidatesIt() => _result = (_balance, _amount).Check();

    [Then(@"the result is valid")]
    public void ThenTheResultIsValid() => Assert.True(_result.IsValid);

    [Then(@"the result is invalid with error code ""(.*)""")]
    public void ThenTheResultIsInvalidWithErrorCode(string errorCode) =>
        Assert.Contains(_result.Errors, e => e.ErrorCode == errorCode);
}
```

Even a Domain-classified rule's own step definitions need no repository — this project proves only the comparison inside `Domain.Rules` itself, over values the `Given`/`And` steps hand it directly. The Handler/`{Feature}Check`'s own loading step is proven separately, in `{Module}.Application.Tests`.

# Rule changes

## MUST
- Call the rule's `Check()` (or raw `IsValid()`, for a scenario that only needs the predicate) directly — never a VO, Entity, or validator
- Assemble a Domain-classified rule's wrapper directly from `Given`/`And` step values — never inject a repository or perform I/O in this class
- Assert a specific `ErrorCode`, not just a boolean, on the invalid path
- Never reference `{Module}.Domain`, `{Module}.Application`, or any adapter type
- Never reimplement the rule's condition inline instead of calling it

# Check list
- [ ] Every `Given/When/Then` in the linked `.feature` file has a matching step method here
- [ ] Step definitions call `Check()`/`IsValid()` on the rule directly
- [ ] No repository, `DbContext`, or async call anywhere in this class
