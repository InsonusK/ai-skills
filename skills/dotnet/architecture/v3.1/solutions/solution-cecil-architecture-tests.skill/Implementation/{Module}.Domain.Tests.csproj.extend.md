---
description: Add the Architecture/ folder holding the two VP1-gated Mono.Cecil checks (exception-scoping, guarded-property-coverage) to {Module}.Domain.Tests
project_name: "{Module}.Domain.Tests"
name: "{Module}.Domain.Tests.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/cecil-architecture-tests
  - element/module-domain-tests-csproj
---

# Goals
- Give `{Module}.Domain.Tests` a build-time guarantee, over compiled IL, that `DomainException`/`EntityNotLoadedException` never leak their layer and that every guarded property's write is paired with its rule — facts plain unit/BDD tests cannot give by construction. These need `{Module}.Domain` entities, so they live here; the two rules-only checks live in `{Module}.Domain.Rules.Tests`.

# Rule changes

## MUST
- Add `/Architecture` under `{Module}.Domain.Tests`, holding `{Module}ArchitectureTests.cs` (exception-scoping), `GuardedPropertyRuleCoverageTests.cs`, and their companion `.feature` files.
- Reference `Mono.Cecil`.
- Load only the module's own `{Module}.Domain` (and, for the coverage registry's rule references, `{Module}.Domain.Rules`) via `typeof(KnownType).Assembly.Location`.
- Never put the dead-rule or code-uniqueness checks here — they scan `{Module}.Domain.Rules` and belong in `{Module}.Domain.Rules.Tests`.

