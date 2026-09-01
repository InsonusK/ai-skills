---
name: checks-degrade-without-domain-layer
description: What the mandatory Cecil companion does for a module that applies VP4 but has no domain layer
problem: solution-cecil-architecture-tests is the mandatory companion of solution-domain-rules (VP4). Two of its four checks scan {Module}.Domain entities and DomainException, which only exist with DomainLogic (VP1). VP4 is not gated on VP1, so a rules-only module can apply VP4 with no entities.
decision: The companion is still applied. Dead-rule detection and code-uniqueness/format run always (they scan {Module}.Domain.Rules). Exception-scoping and guarded-property-coverage are no-ops (pass trivially, an empty scan) when {Module}.Domain has no entities; they become real once VP1 is added.
tags:
  - solution/cecil-architecture-tests
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The four checks:

| Check | Scans | Needs |
| --- | --- | --- |
| Dead-rule detection | `{Module}.Domain.Rules` + its callers | `{Module}.Domain.Rules` (VP4) |
| Code/constant uniqueness & format | `{Module}.Domain.Rules` | `{Module}.Domain.Rules` (VP4) |
| Exception-type scoping | every assembly, for `new DomainException(...)` outside the allowed layer | `DomainException` + entities (VP1) |
| Guarded-property rule coverage | `{Module}.Domain` entity members writing rule-guarded properties | `{Module}.Domain` entities (VP1) |

[[skills/dotnet/architecture/v3.1/solutions/solution-domain-rules.skill/adr/rules-project-references-interfaces-only.md|VP4 is not gated on VP1]] — a rules-only module (validation/policy, no entities) can apply it. For such a module the bottom two checks have nothing to scan.

# Selected variant

**Selected variant:** [[#Apply all four; the domain-dependent two are empty no-ops until VP1]]

# Searched variants

## Make VP4 require VP1 so all four always apply

### Description
Gate CentralizedRules on DomainLogic.

### Benefits
- The companion is always fully meaningful.

### Costs
- Contradicts the Variability Map and `rules-project-references-interfaces-only` — a rules-only module becomes impossible for no real reason.

## Split the companion into "rules checks" and "domain checks"

### Description
Two solutions: one with the two rules checks (companion of VP4), one with the two domain checks (companion of VP1).

### Benefits
- Each solution's checks all apply.

### Costs
- Four checks, built on one Cecil harness, sharing helpers and a registry — splitting duplicates the harness.
- `guarded-property-coverage` needs the rule registry that the rules side owns — the split cuts across a real dependency.

## Apply all four; the domain-dependent two are empty no-ops until VP1 (selected)

### Description
`solution-cecil-architecture-tests` stays one solution, mandatory with VP4. `{Module}RuleArchitectureTests` (dead-rule, code-uniqueness) runs against `{Module}.Domain.Rules`. `{Module}ArchitectureTests` (exception-scoping) and `GuardedPropertyRuleCoverageTests` load `{Module}.Domain`; with no entity types their scans are empty and the `[Fact]`s pass trivially. When VP1 is later added, the same tests start finding real members to check — no new test, no re-application of this solution.

### Benefits
- One solution, one harness, one registry.
- Adding VP1 to a module needs no change to its architecture-test setup.
- Honest: the checks are present and green, and become load-bearing exactly when their target exists.

### Costs
- A green `guarded-property-coverage` on a rules-only module proves nothing — a reader must know it is dormant until VP1. Stated in the skill's top note and here.
- `{Module}.Domain.Tests` must exist even for a module whose `{Module}.Domain` has no entities yet — a near-empty test project. Acceptable: `solution-dotnet-conformance-testing` creates one test project per production project anyway.
