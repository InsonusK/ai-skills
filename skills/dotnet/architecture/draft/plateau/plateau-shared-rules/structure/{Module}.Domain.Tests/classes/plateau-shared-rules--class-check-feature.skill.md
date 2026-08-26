---
name: plateau-shared-rules--class-check-feature
description: Documentary .feature file per Cecil check, in the shared-rules plateau
whenToUse: when adding a new architecture check, or looking up what an existing one proves without reading its Cecil implementation
domain: skill
type: template
plateau: shared-rules
version: 20260824150000
tags:
  - skill/template/feature
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]]"
---

# Goal
- Give a reader a plain-language description of what each structural check proves, discoverable the same way every other rule's `.feature` file is — without pretending a Gherkin step can meaningfully parameterize "is this dead code"

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Check}.feature.create.md|{Check}.feature.create]]

# Core Principles
- Documentary only — no step definitions bind to it. The actual proof is the `[Fact]` with the same name, in the same folder
- Scenario title matches the `[Fact]` method name 1:1

# Implementation
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

Four files exist, one per check, in `/Architecture`: `DeadRuleDetection.feature`, `ExceptionScoping.feature`, `RejectionCodeUniqueness.feature`, `GuardedPropertyRuleCoverage.feature`. See [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Check}.feature.create.md|{Check}.feature.create]] for the naming convention and the full worked example.

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Check}.feature.create.md|{Check}.feature.create]]

# Rules
MUST:
- Carry an explicit banner stating the file is documentary and naming the `[Fact]` that is the real proof
- Scenario title match the `[Fact]` method name exactly
MUST NOT:
- Have a `[Given]`/`[When]`/`[Then]` step definition bound to it

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Check}.feature.create.md|{Check}.feature.create]]

# Check list
- [ ] Every check has a companion `.feature` file with a documentary banner
- [ ] Scenario title matches the `[Fact]` method name
- [ ] No step-definition binding exists for this file

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Check}.feature.create.md|{Check}.feature.create]]
