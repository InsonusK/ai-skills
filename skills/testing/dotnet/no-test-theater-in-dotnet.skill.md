---
version: 20261009220000
name: no-test-theater-in-dotnet
description: The .NET names for the assertion-strength rules of no-test-theater — Ardalis.Result status and validation errors, ordered mock verification, WebApplicationFactory, coverlet branch coverage, Stryker.NET.
whenToUse: When writing or reviewing Reqnroll scenario bindings and assertions in a .NET project.
tags:
  - stack/dotnet
  - concern/testing/unit
  - xunit
  - concern/testing

---

# Goal
- Reqnroll bindings that apply the rules of no-test-theater with the .NET types and tools that carry them.

# Scope
Every rule is in [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md), and which classes need scenarios of their own is in [testing-strategy](skills/testing/core/testing-strategy.skill.md); this skill only names how .NET expresses them. Authoring and placing the scenarios: [cucumber-testing-in-dotnet](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md).

# Core Principle
- A concrete `Result.Status` and its payload prove behavior; `IsSuccess == false` does not.

# Rule

## MUST

### Assert the concrete Ardalis.Result state
Assert the concrete `Ardalis.Result.Status` (`Ok`, `NotFound`, `Invalid`, `Conflict`, …) and, for `Invalid`, every `ValidationErrors` entry by field and message.
- Violation: `Assert.False(result.IsSuccess)` in a step named for a not-found outcome; `Assert.Single(result.ValidationErrors)`.
- Risk: the step passes when the result is `Invalid` or `Conflict` instead of `NotFound`, or when the single error is on the wrong field.
- Fix: `Assert.Equal(ResultStatus.NotFound, result.Status)`; compare each error's identifier and message.

### Verify call order with an ordered verification
Verify the order of orchestrated calls with Moq `MockSequence` or NSubstitute `Received.InOrder` when the order is part of the contract.
- Violation: `mock.Verify(x => x.StepA()); mock.Verify(x => x.StepB());` for a use case where `StepA` must run first.
- Risk: the step passes when `StepB` runs before `StepA` — the orchestration defect itself.
- Fix: one ordered verification over both calls.

### Exercise an endpoint through WebApplicationFactory
Run an endpoint scenario through `WebApplicationFactory` (with Testcontainers where a real dependency is needed), asserting the concrete 401/404/409/422 response where it applies.
- Risk: a binding that calls the handler directly skips routing, authentication and serialization.
- Fix: send the request through the factory's client and assert the full response.

### Read branch coverage from coverlet
Read the branch coverage of the coverlet run in the ReportGenerator report beside the line-coverage badge.
- Risk: 100% line coverage on a method with an `if/else` whose `else` no scenario reaches.
- Fix: add a scenario for the unreached branch.

## SHOULD

### Include mappings in Stryker.NET
Keep mappings inside the scope Stryker.NET mutates through the project's mutation kind.
- Risk: excluding DTOs and mappers leaves `@type/mapping` behavior unmeasured.
- Fix: inspect the survivors and strengthen the scenario's assertion, or record why the mutant is equivalent.

# Check list
- [ ] Every `Ardalis.Result` assertion names a concrete `Result.Status`, and an `Invalid` one each field and message.
- [ ] An orchestration whose order matters is verified with `MockSequence` or `Received.InOrder`.
- [ ] Every endpoint scenario runs through `WebApplicationFactory`.
- [ ] Branch coverage was read, not line coverage alone.
