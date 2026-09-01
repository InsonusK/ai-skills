---
name: rules-project-references-interfaces-only
description: Whether CentralizedRules (VP4) requires DomainLogic (VP1)
problem: v3's solution-domain-rules assumed a domain layer — its Requirements listed solution-domain-behaviour and the strict value-objects solution, and it built_on_plateau plateau-service-with-validated-module-interaction. The v3.1 Variability Map records VP4 with no constraint. Does the mechanism actually need entities?
decision: {Module}.Domain.Rules.csproj references only {Module}.Interfaces (for Soft{ValueObject} types) and FluentValidation — never {Module}.Domain. VP4 is not gated on VP1. Entity / strict-VO redirects apply only when those features are present; the always-common Application-side consumers (PropertyValidator, DtoValidator) are enough on their own.
tags:
  - solution/domain-rules
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The v3.1 Variability Map's VP4 (CentralizedRules) row carries no `Constraint`. But the v3 `solution-domain-rules` reads as if a domain layer is required: it lists `solution-domain-behaviour` and the strict `solution-value-objects` in Requirements, `extends` `{Module}.Domain.Entities`/`ValueObjects`, and `built_on_plateau` a plateau that already has entity behaviour. If the mechanism genuinely needs entities, VP4 should require VP1 and the map is wrong.

# Selected variant

**Selected variant:** [[#Domain.Rules references Interfaces + FluentValidation only; VP4 not gated on VP1]]

# Searched variants

## VP4 requires VP1 (port v3's assumptions)

### Description
`solution-domain-rules` `depends_on solution-domain-behaviour`; the Variability Map gains a `VP4 requires VP1` constraint.

### Benefits
- Matches how v3 always applied it (persistence + domain always present).
- The `extends` on `{Module}.Domain.Entities`/`ValueObjects` is always meaningful.

### Costs
- Not actually true: the centralized-rule value already exists when the *only* duplication is between a `Soft{ValueObject}` `PropertyValidator` and a `{Dto}Validator` — both common, both Application-side, no entity involved.
- `{Module}.Domain.Rules.csproj` per v3's own checklist references "FluentValidation and `{Module}.Interfaces` … nothing else" — it never referenced `{Module}.Domain`. The "Domain" in the name is about the *concept*, not a project reference.
- Forcing VP1 makes a rules-only module (a pure validation/policy module) impossible.

## Domain.Rules references Interfaces + FluentValidation only; VP4 not gated on VP1 (selected)

### Description
`{Module}.Domain.Rules.csproj` references `{Module}.Interfaces` (Soft VO types) + FluentValidation, nothing else. `solution-domain-rules` `depends_on` = `solution-sln-structure`, `solution-dto-property-validators`, `solution-dotnet-conformance-testing`. The `extends` entries for `{Module}.Domain.ValueObjects.{ValueObject}.cs` and `{Module}.Domain.Entities.{EntityName}.cs` are conditional: they are applied only for a module that also has VP3 / VP1. The Variability Map keeps VP4 with no constraint.

### Benefits
- Matches the actual project reference graph (already true in v3).
- A rules-only module is possible.
- The Variability Map stays correct.

### Costs
- The `extends` list mixes always-applicable (`{Dto}Validator`) and conditional (`{EntityName}.cs`) entries — the skill body has to say which is which.
- The mandatory Cecil companion's four checks degrade to an applicable subset when there is no domain layer (dead-rule + code-uniqueness still work; exception-scoping + guarded-property-coverage need entities) — documented in `solution-cecil-architecture-tests`.
