---
description: A documentary .feature file per Cecil check — describes intent in plain language, with no real step-definition binding; the [Fact] next to it is the actual proof
project_name: "{Module}.Domain.Tests"
name: "{Check}.feature"
element_kind: feature
change_kind: create
tags:
  - solution/cecil-architecture-tests
  - element/check-feature
---

# Goals
- Give a reader a plain-language description of what each structural check proves, discoverable the same way every other rule's `.feature` file is, without pretending a Gherkin step can meaningfully parameterize "is this dead code"

# Core Principles
- This is the documented exception to `solution-dotnet-conformance-testing`'s "every scenario has a matching step definition that calls production code" MUST — architecture/structural facts have no natural input → output shape, so the scenario body is prose, not Given/When/Then assertions, and it carries an explicit banner saying so
- Scenario titles match their `[Fact]` method name 1:1 — a reader lands on the right test by name, not by following a binding
- One `.feature` file per check, living next to the check's own test class in `/Architecture`

# Naming convention
| use case | file name pattern | file name |
| -------- | ------------------ | ---------- |
| Dead-rule detection | `DeadRuleDetection.feature` | `DeadRuleDetection.feature` |
| Exception scoping | `ExceptionScoping.feature` | `ExceptionScoping.feature` |
| Generated-constant uniqueness | `RejectionCodeUniqueness.feature` | `RejectionCodeUniqueness.feature` |
| Guarded-property coverage | `GuardedPropertyRuleCoverage.feature` | `GuardedPropertyRuleCoverage.feature` |

# Implementation changes

Worked example — `DeadRuleDetection.feature`:

```gherkin
# This feature is documentary only — no step definitions bind to it.
# The actual proof is EveryDomainRuleCheck_IsCalledByProductionCodeOutsideRules,
# in {Module}ArchitectureTests.cs, in this same folder.

Feature: Every centralized rule's Check() is actually called by production code

  Scenario: EveryDomainRuleCheck_IsCalledByProductionCodeOutsideRules
    A Check() extension declared in Domain.Rules that no production code outside
    Domain.Rules ever calls is dead weight — its own scenario proves the predicate
    is correct, but proves nothing about whether anything actually uses it.
```

# Rule changes

## MUST
- Carry an explicit banner stating the file is documentary and naming the `[Fact]` that is the real proof
- Scenario title match the `[Fact]` method name exactly

## MUST NOT
- Have a `[Given]`/`[When]`/`[Then]` step definition bound to it — a binding that exists only to satisfy the base conformance rule, without calling anything meaningful, is worse than no binding

# Check list
- [ ] Every check has a companion `.feature` file with a documentary banner
- [ ] Scenario title matches the `[Fact]` method name
- [ ] No step-definition binding exists for this file
