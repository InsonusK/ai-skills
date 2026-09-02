---
name: checks-degrade-without-domain-layer
description: What the mandatory Cecil companion does for a module that applies VP4 but has no domain layer
problem: solution-cecil-architecture-tests is the mandatory companion of solution-domain-rules (VP4). Two of its four checks scan {Module}.Domain entities and DomainException, which only exist with DomainLogic (VP1). VP4 is not gated on VP1, so a rules-only module can apply VP4 with no entities.
decision: The companion is still applied, split by which test project can host each check. Dead-rule detection and code-uniqueness/format live in {Module}.Domain.Rules.Tests (always present with VP4) as {Module}RuleArchitectureTests. Exception-scoping and guarded-property-coverage live in {Module}.Domain.Tests as {Module}ArchitectureTests + GuardedPropertyRuleCoverageTests — which, like {Module}.Domain itself, only exist once VP1 is added; for a rules-only module they are simply absent, not empty no-ops.
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

**Selected variant:** [[#Apply all four; split by host project — the domain-dependent two are absent until VP1]]

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

## Apply all four; split by host project — the domain-dependent two are absent until VP1 (selected)

### Description
`solution-cecil-architecture-tests` stays one solution, mandatory with VP4, but its `extends` targets two test projects. `{Module}RuleArchitectureTests` (dead-rule, code-uniqueness) goes in `{Module}.Domain.Rules.Tests`, which always exists with VP4. `{Module}ArchitectureTests` (exception-scoping) and `GuardedPropertyRuleCoverageTests` go in `{Module}.Domain.Tests`, which — like `{Module}.Domain` itself — only exists with VP1; for a rules-only module those two `.cs` files are simply not created. When VP1 is later added, `{Module}.Domain.Tests` appears and this solution's `{Module}.Domain.Tests.csproj.extend` files apply then.

### Benefits
- One solution, one harness, one registry — the split is only across `extends` targets, not across solutions.
- Each check lives in a project that can actually compile it — no near-empty `{Module}.Domain.Tests` referencing a non-existent `{Module}.Domain`.
- Honest: a rules-only module has exactly the two checks that mean something for it, and no dormant green `[Fact]`s pretending to prove more.

### Costs
- The solution's `{Module}.Domain.Tests.csproj.extend` mutations are conditional on VP1 — one more "applies only when X" note to carry. Acceptable: every per-entity solution already carries the same conditionality.
