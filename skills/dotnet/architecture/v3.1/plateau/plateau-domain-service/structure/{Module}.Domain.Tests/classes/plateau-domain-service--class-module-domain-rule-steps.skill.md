---
name: plateau-domain-service--class-module-domain-rule-steps
description: Class {Rule}Steps in {Module}.Domain.Tests of the plateau-domain-service plateau — Reqnroll bindings proving an entity invariant, a domain service, or a strict Value Object against the real type
whenToUse: when writing the step definitions for a {Module}.Domain.Tests feature file
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
- Prove every scenario in `Rules/{Rule}.feature` against `{Module}.Domain`'s real entity method / domain service / strict Value Object constructor.

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `[Binding] sealed class {Rule}Steps` — Reqnroll bindings.
- Validator-shaped: construct the entity / VO, invoke the real method, capture the outcome; on a failure scenario assert `DomainException.Code`.
- `Record.Exception(() => ...) as DomainException` is the idiom for a throw expectation.
- References `{Module}.Domain` only.

# Implementation
```csharp
// Skill: plateau-domain-service--class-module-domain-rule-steps
// Plateau: domain-service
// Version: 20260902000000
using Reqnroll;
using {Module}.Domain.Entities;
using {Module}.Domain.ValueObjects;
using Shared.Exceptions;
using Xunit;

namespace {Module}.Domain.Tests.StepDefinitions;

[Binding]
public sealed class {Entity}Steps
{
    private DomainException? _error;

    [When("a title {string} is constructed")]
    public void WhenTitle(string v) => _error = Record.Exception(() => new {ValueObject}(v)) as DomainException;

    [Then("a domain error {string} is raised")]
    public void ThenError(string code)
    {
        Assert.NotNull(_error);
        Assert.Equal(code, _error!.Code);
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Rules
MUST:
- `[Binding] sealed class {Rule}Steps` in `{Module}.Domain.Tests/StepDefinitions`.
- Invoke the real entity method / VO constructor; on a failure scenario assert the exact `DomainException.Code`.
- Reference `{Module}.Domain` only; never reach into `{Module}.Application`.
- Never apply several plateau templates per class.

# Check list
- [ ] Every `Given/When/Then` has a matching step; the real Domain type is exercised.
- [ ] Failure scenarios assert `DomainException.Code`.

# Unittest TestCases
- [ ] WHEN the feature runs THEN each scenario's assertion passes against the real Domain type.
