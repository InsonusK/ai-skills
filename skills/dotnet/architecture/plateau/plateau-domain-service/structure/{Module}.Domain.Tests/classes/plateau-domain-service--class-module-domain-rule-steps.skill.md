---
name: plateau-domain-service--class-module-domain-rule-steps
description: Class {Rule}Steps in {Module}.Domain.Tests of the plateau-domain-service plateau — Reqnroll bindings proving an entity invariant, a domain service, or a strict Value Object against the real type
whenToUse: when writing the step definitions for a {Module}.Domain.Tests feature file
domain: skill
type: template
plateau: domain-service
version: 20261009220001
tags:
  - skill/template/class
  - plateau/domain-service
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
- Domain scenarios enter through the entity, domain service or strict Value Object; supporting references follow the owning test project's mirrored boundary.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects), [binding mechanics](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for layout, bindings, assertions, packages and runner configuration.

# Implementation
The layer-specific action and observation follow the contributing solution linked below; generic binding code comes from the testing skills.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Rules
MUST:
- Apply [production-code bindings](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#exercise-production-code-from-bindings) and [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must).
- Invoke the Domain entry point; do not substitute an Application handler for the behavior being proven.
- Never apply several plateau templates per class.

# Check list
- [ ] The action and observation prove this layer's contract, with the linked testing conventions applied.

# Unittest TestCases
- [ ] WHEN the feature runs THEN each scenario's assertion passes against the real Domain type.
