---
name: plateau-offline-sync-service--class-rule-steps
description: Class {Rule}RuleSteps in {Module}.Domain.Rules.Tests of the plateau-offline-sync-service plateau — Reqnroll bindings proving one Rule's own Check() against every scenario in its .feature file
whenToUse: when writing the step definitions for a {Module}.Domain.Rules.Tests feature file
domain: skill
type: template
plateau: offline-sync-service
version: 20261009220000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/solution-domain-shared-rules.skill|solution-domain-shared-rules]]"
---

# Goal
- Prove every scenario in the rule's `.feature` file directly against `{Rule}.IsValid()` / `{Rule}.Check()` / the `IRuleBuilder` extension — the rule's own correctness in isolation.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/solution-domain-shared-rules.skill|solution-domain-shared-rules]] - [[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create/{Rule}RuleSteps.cs.create|{Rule}RuleSteps.cs]]

# Core Principles
- Apply the linked [testing conventions](#testing-conventions).
- Apply ONE plateau template per class.
- Build the wrapper (`Soft{ValueObject}` or a tuple), call `.Check()`, assert on the `ValidationResult`: passing scenario → `IsValid`; failing → the exact `ErrorCode` present in `Errors`.
- References `{Module}.Domain.Rules` only.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects), [binding mechanics](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for layout, bindings, assertions, packages and runner configuration.

# Implementation
The layer-specific action and observation follow the contributing solution linked below; generic binding code comes from the testing skills.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/solution-domain-shared-rules.skill|solution-domain-shared-rules]] - [[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create/{Rule}RuleSteps.cs.create|{Rule}RuleSteps.cs]]

# Rules
MUST:
- Apply [production-code bindings](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#exercise-production-code-from-bindings) and [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must).
- Reference `{Module}.Domain.Rules` only.
- Never apply several plateau templates per class.

# Check list
- [ ] The action and observation prove this layer's contract, with the linked testing conventions applied.

# Unittest TestCases
- [ ] WHEN the feature runs THEN each scenario's assertion passes against the real rule.
