---
version: 20261008170000
name: no-test-theater-in-dotnet
description: xUnit/.NET-specific rules for assertion strength — Ardalis.Result status checks, branch vs. line coverage, Stryker.NET mutation testing, and integration-test requirements per API endpoint.
whenToUse: When writing or reviewing Reqnroll scenario bindings and assertions in a .NET project.
tags:
  - stack/dotnet
  - concern/testing/unit
  - xunit
  - concern/testing

---

# Goal
- Reqnroll bindings assert complete .NET outcomes, validation errors and orchestration order.
- Coverage and mutation reports expose untested branches and weak assertions.

# Scope
Apply [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md) together with this .NET extension; choose scenario scope through [testing-strategy-in-dotnet](skills/testing/dotnet/testing-strategy-in-dotnet.skill.md).

# Core Principle
- A concrete result status and its payload prove behavior; a success boolean alone does not.

# Rule

## MUST

### Author scenarios through Reqnroll
Author package tests as Gherkin scenarios following [cucumber-testing-in-dotnet](skills/testing/dotnet/cucumber-testing-in-dotnet.skill.md).
- Risk: direct xUnit tests duplicate scenario coverage and disappear from the living doc.
- Fix: keep xUnit as Reqnroll's runner; justify an exceptional plain test in a comment per the core Cucumber rule.

### Assert concrete result states
Assert the concrete `Ardalis.Result.Status` and complete relevant payload, including each invalid field and its validation message.
- Risk: checking `IsSuccess == false` or counting errors passes for the wrong failure state.
- Fix: compare the expected status, field names, messages and values explicitly.

### Review branch coverage
Review coverlet's branch coverage in ReportGenerator alongside the line coverage badge.
- Risk: line coverage can hide an untested conditional branch.
- Fix: add a scenario for each behavior-changing branch and review both coverage measurements.

### Exercise endpoint boundaries
Exercise each new API endpoint through `WebApplicationFactory` or an applicable integration boundary with a happy scenario and an applicable error scenario.
- Risk: isolated mocks omit routing, authentication and transport failures.
- Fix: assert the full response and concrete 401/404/409/422 behavior where applicable.

### Assert orchestration order
Assert the full expected response and the required order of component calls in usecase and workflow scenarios.
- Risk: independent call checks pass when the workflow executes its operations out of order.
- Fix: use Moq `MockSequence` or NSubstitute `Received.InOrder` when ordering belongs to the contract.

## SHOULD

### Measure mutation strength
Run Stryker.NET through the project's mutation kind on changed production rules, including mappings.
- Risk: omitting mappings leaves `@type/mapping` behavior unmeasured.
- Fix: inspect survivors and strengthen the scenario assertion or record why the mutant is equivalent; thresholds belong to the project's kind configuration.

# Check list
- [ ] Package tests follow [Author scenarios through Reqnroll](#author-scenarios-through-reqnroll).
- [ ] Assertions satisfy [Assert concrete result states](#assert-concrete-result-states).
- [ ] Coverage review follows [Review branch coverage](#review-branch-coverage).
- [ ] Endpoint scenarios follow [Exercise endpoint boundaries](#exercise-endpoint-boundaries).
- [ ] Workflow assertions satisfy [Assert orchestration order](#assert-orchestration-order).
- [ ] Mutation review follows [Measure mutation strength](#measure-mutation-strength).
