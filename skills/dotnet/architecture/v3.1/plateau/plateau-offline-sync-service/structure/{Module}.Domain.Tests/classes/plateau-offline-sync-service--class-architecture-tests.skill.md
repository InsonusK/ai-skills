---
name: plateau-offline-sync-service--class-architecture-tests
description: The Mono.Cecil architecture-test classes in the plateau-offline-sync-service plateau — {Module}RuleArchitectureTests (in {Module}.Domain.Rules.Tests) and {Module}ArchitectureTests + GuardedPropertyRuleCoverageTests (in {Module}.Domain.Tests), [Fact]s over compiled IL verifying the rule mechanism is wired correctly
whenToUse: when adding or editing a build-time structural check (dead rule, code uniqueness, exception scoping, guarded-property coverage), or checking which test project hosts which check
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]]"
---

# Goal
- Prove structural facts unit/BDD tests cannot: every `Check()` in `{Module}.Domain.Rules` is called by production code (no dead rule); rejection-code constants are unique and well-formed; `DomainException` / `EntityNotLoadedException` are constructed only from their intended namespaces; every entity member writing a rule-guarded property also calls that rule (registry-driven, recursive, `visited`-guarded).

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.extend/{Module}RuleArchitectureTests.cs.create.md|{Module}RuleArchitectureTests.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- **Split by host project — each check lives where it can compile:**
  - `{Module}RuleArchitectureTests` in `{Module}.Domain.Rules.Tests/Architecture/` — **dead-rule detection** + **code-uniqueness/format** (2 `[Fact]`s). Scans `{Module}.Domain.Rules` (+ its callers for dead-rule). Always present with VP4.
  - `{Module}ArchitectureTests` in `{Module}.Domain.Tests/Architecture/` — **exception-type scoping** (1 `[Fact]`). Needs `{Module}.Domain` + `DomainException`. VP1-gated — absent for a rules-only module.
  - `GuardedPropertyRuleCoverageTests` in `{Module}.Domain.Tests/Architecture/` — the registry-driven call-graph check, its own class. VP1-gated.
- Load every target assembly via `typeof(KnownType).Assembly.Location` — never a hardcoded path. Match call targets by simple name (`DeclaringType.Name` + `Name`).
- The coverage registry (`Dictionary<(Entity, Property), Rule[]>`) lives in the test project, never in production code. Guard every recursive walk with `visited.Add(method)`.
- Each check has a **documentary** `.feature` (no real binding) in the same `Architecture/` folder, scenario titles mirroring `[Fact]` names — the `[Fact]` is the sole proof.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-architecture-tests
// Plateau: offline-sync-service
// Version: 20260902000000

// {Module}.Domain.Rules.Tests/Architecture/{Module}RuleArchitectureTests.cs
public sealed class {Module}RuleArchitectureTests
{
    [Fact] public void EveryDomainRuleCheck_IsCalledByProductionCodeOutsideRules() { /* Call/Callvirt scan */ }
    [Fact] public void RejectionCodes_AreUniqueAndFollowModuleDotClassDotReasonFormat() { /* f.HasConstant + regex + uniqueness */ }
}

// {Module}.Domain.Tests/Architecture/{Module}ArchitectureTests.cs
public sealed class {Module}ArchitectureTests
{
    [Fact] public void DomainException_IsThrownOnlyFromValueObjectsOrEntities() { /* Newobj scan outside allowed namespaces */ }
}

// {Module}.Domain.Tests/Architecture/GuardedPropertyRuleCoverageTests.cs — its own class, recursive, visited-guarded
```
Full worked implementations are in the solution's `examples/`.

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Module}ArchitectureTests.cs.create.md|{Module}ArchitectureTests.cs.create]]
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/GuardedPropertyRuleCoverageTests.cs.create.md|GuardedPropertyRuleCoverageTests.cs.create]]

# Rules
MUST:
- Put dead-rule + code-uniqueness in `{Module}RuleArchitectureTests` (`{Module}.Domain.Rules.Tests`); exception-scoping in `{Module}ArchitectureTests` and the coverage check in `GuardedPropertyRuleCoverageTests` (both `{Module}.Domain.Tests`).
- Load every target assembly via `typeof(KnownType).Assembly.Location`.
- Keep the coverage registry in the test project; guard every recursive call-graph walk with a `visited` set.
- Give each check a documentary `.feature` with no fabricated binding, scenario titles mirroring `[Fact]` names, in the same `Architecture/` folder.
- A new multi-field rule adds a registry entry, not a new bespoke test class.
- Never put rule-check logic in an individual property setter for a multi-property rule.
- Never apply several plateau templates per class.

# Check list
- [ ] `{Module}RuleArchitectureTests` (2 `[Fact]`s) in `{Module}.Domain.Rules.Tests/Architecture/`.
- [ ] `{Module}ArchitectureTests` (1 `[Fact]`) + `GuardedPropertyRuleCoverageTests` (own class) in `{Module}.Domain.Tests/Architecture/`.
- [ ] Assemblies loaded via `typeof(...).Assembly.Location`; registry in the test project; `visited` guard present.
- [ ] One documentary `.feature` per check next to its `[Fact]`, titles matching, no fake bindings.

# Unittest TestCases
- [ ] WHEN a `Check()` exists that no production code calls THEN `EveryDomainRuleCheck_IsCalledByProductionCodeOutsideRules` fails.
- [ ] WHEN `DomainException` is constructed outside the domain layer THEN `DomainException_IsThrownOnlyFromValueObjectsOrEntities` fails.
- [ ] WHEN two rejection codes collide THEN `RejectionCodes_AreUniqueAndFollowModuleDotClassDotReasonFormat` fails.
