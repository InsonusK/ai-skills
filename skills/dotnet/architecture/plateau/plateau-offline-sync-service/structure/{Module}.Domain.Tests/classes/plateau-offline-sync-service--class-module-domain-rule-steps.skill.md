---
name: plateau-offline-sync-service--class-module-domain-rule-steps
description: Class {Rule}Steps in {Module}.Domain.Tests of the plateau-offline-sync-service plateau — Reqnroll bindings proving an entity invariant, a domain service, or a strict Value Object against the real type
whenToUse: when writing the step definitions for a {Module}.Domain.Tests feature file
domain: skill
type: template
plateau: offline-sync-service
version: 20261009220000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]]"
---

# Goal
- Prove every scenario in `features/{Rule}.feature` against `{Module}.Domain`'s real entity method / domain service / strict Value Object constructor.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Core Principles
- Apply the linked [testing conventions](#testing-conventions).
- Apply ONE plateau template per class.
- Validator-shaped: construct the entity / VO, invoke the real method, capture the outcome; on a failure scenario assert `DomainException.Code`.
- References `{Module}.Domain` only.
- Two feature sources, two binding classes: this project's own `/features/{Rule}.feature` (entity/domain-service/strict-VO invariants, `Then a domain error "..." is raised`) and, with VP4, the linked `format/` scenarios from `{Module}.Domain.Rules.Spec` (`Then the check fails with error code "..."`) re-proven through the VO constructor. The Gherkin wording of the shared file is fixed by `solution-domain-shared-rules` — bind to it exactly, never reword.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects), [binding mechanics](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for layout, bindings, assertions, packages and runner configuration.

# Implementation
The layer-specific action and observation follow the contributing solution linked below; generic binding code comes from the testing skills.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Rules
MUST:
- Apply [production-code bindings](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#exercise-production-code-from-bindings) and [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must).
- Reference `{Module}.Domain` only; never reach into `{Module}.Application`.
- For a linked `{Module}.Domain.Rules.Spec` `@format` scenario, bind the shared Gherkin verbatim (`the check fails with error code "..."` / `the check passes`) — that wording is owned by `solution-domain-shared-rules`; never copy the scenario text into a local `.feature`.
- Never apply several plateau templates per class.

# Check list
- [ ] Every linked `@format` scenario from `{Module}.Domain.Rules.Spec` has a binding here that goes through the VO constructor, not through `{Rule}.Check()` directly.

# Unittest TestCases
- [ ] WHEN the feature runs THEN each scenario's assertion passes against the real Domain type.
