---
name: class-rule-rule-steps
description: Class {Rule}RuleSteps in {Module}.Domain.Rules.Tests, in the v1 plateau
whenToUse: when proving a rule's own IsValid()/Check(), against every scenario in its .feature file regardless of classification tag
domain: skill
type: template
plateau: v1
version: 20260824150000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Prove `{Rule}`'s own predicate and `Check()` are correct, independent of any adapter that later redirects to it

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create/{Rule}RuleSteps.cs.create.md|{Rule}RuleSteps.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | ------------------ | --------- |
| Step definitions for one rule's own logic | `{Rule}RuleSteps` | `ComplexityRuleSteps` | `{Rule}RuleSteps.cs` | `ComplexityRuleSteps.cs` |

# Implementation
```csharp
//Skill: class-rule-rule-steps
//Plateau: v1
//Version: 20260824150000

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

Domain-classified rules need no repository here either — the `Given`/`And` steps assemble the wrapper directly. See [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create/{Rule}RuleSteps.cs.create.md|{Rule}RuleSteps.cs.create]] for the full `AccountWithdrawalRuleSteps` worked example.

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create/{Rule}RuleSteps.cs.create.md|{Rule}RuleSteps.cs.create]]

# Rules
MUST:
- Call the rule's `Check()`/`IsValid()` directly — never a VO, Entity, or validator
- Assert a specific `ErrorCode`, not just a boolean, on the invalid path
MUST NOT:
- Reference `{Module}.Domain`, `{Module}.Application`, or any adapter type
- Perform I/O or inject a repository

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create/{Rule}RuleSteps.cs.create.md|{Rule}RuleSteps.cs.create]]

# Check list
- [ ] Every `Given/When/Then` in the linked `.feature` file has a matching step method here
- [ ] Step definitions call `Check()`/`IsValid()` on the rule directly
- [ ] No repository, `DbContext`, or async call anywhere in this class

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create/{Rule}RuleSteps.cs.create.md|{Rule}RuleSteps.cs.create]]
