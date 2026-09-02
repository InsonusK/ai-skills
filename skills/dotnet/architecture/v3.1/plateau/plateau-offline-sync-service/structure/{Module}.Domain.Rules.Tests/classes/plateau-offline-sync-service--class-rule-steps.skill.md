---
name: plateau-offline-sync-service--class-rule-steps
description: Class {Rule}RuleSteps in {Module}.Domain.Rules.Tests of the plateau-offline-sync-service plateau — Reqnroll bindings proving one Rule's own Check() against every scenario in its .feature file
whenToUse: when writing the step definitions for a {Module}.Domain.Rules.Tests feature file
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Prove every scenario in the rule's `.feature` file directly against `{Rule}.IsValid()` / `{Rule}.Check()` / the `IRuleBuilder` extension — the rule's own correctness in isolation.

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create/{Rule}RuleSteps.cs.create.md|{Rule}RuleSteps.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `[Binding] sealed class {Rule}RuleSteps` — Reqnroll bindings.
- Build the wrapper (`Soft{ValueObject}` or a tuple), call `.Check()`, assert on the `ValidationResult`: passing scenario → `IsValid`; failing → the exact `ErrorCode` present in `Errors`.
- References `{Module}.Domain.Rules` only.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-rule-steps
// Plateau: offline-sync-service
// Version: 20260902000000
using FluentValidation.Results;
using Reqnroll;
using {Module}.Domain.Rules;
using {Module}.Interfaces.ValueObjects;
using Xunit;

namespace {Module}.Domain.Rules.Tests.StepDefinitions;

[Binding]
public sealed class {ValueObject}RuleSteps
{
    private ValidationResult _result = null!;

    [When("the value {string} is checked")]
    public void WhenChecked(string value) => _result = new Soft{ValueObject}(value).Check();

    [Then("the check fails with error code {string}")]
    public void ThenFails(string code)
    {
        Assert.False(_result.IsValid);
        Assert.Contains(_result.Errors, e => e.ErrorCode == code);
    }

    [Then("the check passes")]
    public void ThenPasses() => Assert.True(_result.IsValid);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create/{Rule}RuleSteps.cs.create.md|{Rule}RuleSteps.cs.create]]

# Rules
MUST:
- `[Binding] sealed class {Rule}RuleSteps` in `{Module}.Domain.Rules.Tests/StepDefinitions`.
- Call the real `{Rule}.Check()`; assert the exact `ErrorCode` on a failure scenario.
- Reference `{Module}.Domain.Rules` only.
- Never apply several plateau templates per class.

# Check list
- [ ] Every `Given/When/Then` has a matching step; the real `Check()` is invoked.
- [ ] Failure scenarios assert the exact `ErrorCode`.

# Unittest TestCases
- [ ] WHEN the feature runs THEN each scenario's assertion passes against the real rule.
