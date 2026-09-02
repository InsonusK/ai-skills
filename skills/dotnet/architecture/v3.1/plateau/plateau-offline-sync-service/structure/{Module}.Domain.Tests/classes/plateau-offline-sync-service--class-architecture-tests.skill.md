---
name: plateau-offline-sync-service--class-architecture-tests
description: Classes {Module}ArchitectureTests / GuardedPropertyRuleCoverageTests in {Module}.Domain.Tests of the plateau-offline-sync-service plateau — Mono.Cecil [Fact]s over compiled IL verifying the rule mechanism is wired correctly
whenToUse: when adding a build-time structural check (dead rule, exception scoping, code uniqueness, guarded-property coverage), or editing an existing one
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
- Prove structural facts unit/BDD tests cannot: every `Check()` in `{Module}.Domain.Rules` is called by production code (no dead rule); `DomainException` / `EntityNotLoadedException` are constructed only from their intended namespaces; rejection-code constants are unique and well-formed; every entity member writing a rule-guarded property also calls that rule (registry-driven, recursive, `visited`-guarded).

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Module}ArchitectureTests.cs.create.md|{Module}ArchitectureTests.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- Live under `{Module}.Domain.Tests/Architecture/`. The three single-pass checks (dead-rule, exception-scoping, code-uniqueness) share `{Module}ArchitectureTests` (three `[Fact]`s); the registry-driven call-graph check is its own class `GuardedPropertyRuleCoverageTests`.
- Load each target assembly via `typeof(KnownType).Assembly.Location` — never a hardcoded path. Match call targets by simple name (`DeclaringType.Name` + `Name`), not full symbol resolution.
- Any coverage registry (`Dictionary<(Entity, Property), Rule[]>`) lives in the test project, never in production code. Guard every recursive walk with `visited.Add(method)`.
- Each check has a **documentary** `.feature` (no real binding) with scenario titles mirroring `[Fact]` names — the `[Fact]` is the sole proof.
- **Degrades**: dead-rule and code-uniqueness always run; exception-scoping and guarded-property-coverage are no-ops without `{Module}.Domain` entities + `DomainException` (VP1).

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-architecture-tests
// Plateau: offline-sync-service
// Version: 20260902000000
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace {Module}.Domain.Tests.Architecture;

public sealed class {Module}ArchitectureTests
{
    private static AssemblyDefinition Rules() =>
        AssemblyDefinition.ReadAssembly(typeof({Module}.Domain.Rules.Common /* marker */).Assembly.Location);

    [Fact]
    public void Every_Check_is_called_by_production_code() { /* collect Call/Callvirt targets; assert each Check() appears */ }

    [Fact]
    public void DomainException_is_thrown_only_from_the_domain_layer() { /* Newobj scan outside allowed namespaces */ }

    [Fact]
    public void Rejection_codes_are_unique_and_well_formed() { /* f.HasConstant fields matching *Code; regex + uniqueness */ }
}
```
Full worked implementations are in the solution's `examples/`.

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/GuardedPropertyRuleCoverageTests.cs.create.md|GuardedPropertyRuleCoverageTests.cs.create]]

# Rules
MUST:
- Load every target assembly via `typeof(KnownType).Assembly.Location`.
- Keep the coverage registry in the test project; guard every recursive call-graph walk with a `visited` set.
- Keep the call-graph/registry-driven check in its own class, separate from the single-pass checks.
- Give each check a documentary `.feature` with no fabricated binding; scenario titles mirror `[Fact]` names.
- A new multi-field rule adds a registry entry, not a new bespoke test class.
- Never put rule-check logic in an individual property setter for a multi-property rule.
- Never apply several plateau templates per class.

# Check list
- [ ] `Architecture/` folder with `{Module}ArchitectureTests` (3 `[Fact]`s) + `GuardedPropertyRuleCoverageTests` (own class).
- [ ] Assemblies loaded via `typeof(...).Assembly.Location`; registry in the test project; `visited` guard present.
- [ ] One documentary `.feature` per check, titles matching `[Fact]` names, no fake bindings.

# Unittest TestCases
- [ ] WHEN a `Check()` exists that no production code calls THEN `Every_Check_is_called_by_production_code` fails.
- [ ] WHEN `DomainException` is constructed outside the domain layer THEN the exception-scoping `[Fact]` fails.
- [ ] WHEN two rejection codes collide THEN the code-uniqueness `[Fact]` fails.
