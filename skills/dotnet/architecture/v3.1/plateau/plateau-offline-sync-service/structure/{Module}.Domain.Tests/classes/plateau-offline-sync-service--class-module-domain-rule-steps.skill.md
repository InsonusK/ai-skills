---
name: plateau-offline-sync-service--class-module-domain-rule-steps
description: Class {Rule}Steps in {Module}.Domain.Tests of the plateau-offline-sync-service plateau — Reqnroll bindings proving an entity invariant, a domain service, or a strict Value Object against the real type
whenToUse: when writing the step definitions for a {Module}.Domain.Tests feature file
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
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
- Two feature sources, two binding classes: this project's own `/Rules/{Rule}.feature` (entity/domain-service/strict-VO invariants, `Then a domain error "..." is raised`) and, with VP4, the linked `@format` scenarios from `{Module}.Domain.Rules.Spec` (`Then the check fails with error code "..."`) re-proven through the VO constructor. The Gherkin wording of the shared file is fixed by `solution-domain-shared-rules` — bind to it exactly, never reword.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-module-domain-rule-steps
// Plateau: domain-service
// Version: 20260902000000
using Reqnroll;
using {Module}.Domain.Entities;
using {Module}.Domain.ValueObjects;
using Shared.Exceptions;
using Xunit;

namespace {Module}.Domain.Tests.StepDefinitions;

// (a) this project's own entity/VO invariant scenarios
[Binding]
public sealed class {Entity}Steps
{
    private {Entity}? _item;
    private DomainException? _error;

    [Given("a completed item")]
    public void GivenCompleted()
    {
        _item = {Entity}.Create(System.Guid.NewGuid(), new {ValueObject}("x"));
        _item.Complete();
    }

    [When("it is renamed")]
    public void WhenRenamed() => _error = Record.Exception(() => _item!.Rename(new {ValueObject}("y"))) as DomainException;

    [Then("a domain error {string} is raised")]
    public void ThenError(string code)
    {
        Assert.NotNull(_error);
        Assert.Equal(code, _error!.Code);
    }
}

// (b) VP4: the linked @format scenarios from {Module}.Domain.Rules.Spec, re-proven fail-fast
//     through the strict VO constructor. Same Gherkin text {Rule}RuleSteps binds in
//     {Module}.Domain.Rules.Tests against Check(); Reqnroll resolves bindings per assembly.
[Binding]
public sealed class {ValueObject}Steps
{
    private DomainException? _error;

    [When("the {concept} {string} is checked")]
    public void WhenChecked(string value) => _error = Record.Exception(() => new {ValueObject}(value)) as DomainException;

    [Then("the check fails with error code {string}")]
    public void ThenFails(string code)
    {
        Assert.NotNull(_error);
        Assert.Equal(code, _error!.Code);
    }

    [Then("the check passes")]
    public void ThenPasses() => Assert.Null(_error);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]] - [[../../../../../solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create.md|{Rule}Steps.cs.create]]

# Rules
MUST:
- `[Binding] sealed class {Rule}Steps` in `{Module}.Domain.Tests/StepDefinitions`.
- Invoke the real entity method / VO constructor; on a failure scenario assert the exact `DomainException.Code`.
- Reference `{Module}.Domain` only; never reach into `{Module}.Application`.
- For a linked `{Module}.Domain.Rules.Spec` `@format` scenario, bind the shared Gherkin verbatim (`the check fails with error code "..."` / `the check passes`) — that wording is owned by `solution-domain-shared-rules`; never copy the scenario text into a local `.feature`.
- Never apply several plateau templates per class.

# Check list
- [ ] Every `Given/When/Then` has a matching step; the real Domain type is exercised.
- [ ] Failure scenarios assert `DomainException.Code`.
- [ ] Every linked `@format` scenario from `{Module}.Domain.Rules.Spec` has a binding here that goes through the VO constructor, not through `{Rule}.Check()` directly.

# Unittest TestCases
- [ ] WHEN the feature runs THEN each scenario's assertion passes against the real Domain type.
