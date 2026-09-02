---
name: solution-domain-behaviour
description: Introduces a module's domain layer (VP1 DomainLogic) — creates {Module}.Domain with its first entity, and defines how entities change state: every method or setter that mutates validates via its own local predicate first and throws DomainException, with bulky logic extracted to static domain services.
whenToUse: when a module first needs a real domain layer (entities with guarded state transitions), when adding or changing an Entity method that mutates state, or when entity behavior logic outgrows the entity and needs a domain service
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - concern/architecture
  - domain
  - entity
  - behavior
  - invariants
  - solution/domain-behaviour
  - stack/dotnet
creates:
  - "{Module}.Domain.csproj"
  - "{Module}.Domain.Entities.{EntityName}.cs"
  - "{Module}.Domain.Services.{Behavior}Service.cs"
  - "Shared.Exceptions.DomainException.cs"
extends:
  - "{Module}.Domain.csproj"
  - "{Module}.Domain.Entities.{EntityName}.cs"
  - Shared.csproj
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/solution-soft-value-objects.skill.md|solution-soft-value-objects]]"
built_on_plateau:
---

# Goal
- **Introduce the domain layer** - Create `{Module}.Domain` with its first entity, the first time a module needs guarded, behavior-rich entities — the project does not exist at the common baseline.
- Define how entities mutate state while keeping invalid states unreachable: every state change validates before assignment via a condition this solution owns and writes locally.
- Allow bulky or multi-step domain logic to be extracted to static domain services without scattering property mutation points.
- Keep entity behavior the single source of truth for invariant enforcement, and `DomainException` the single failure model.

# Capabilities
- A `{Module}.Domain` project referencing only `Shared` and `{Module}.Interfaces` — no repositories, no `DbContext`. (`solution-domain-configuration` (VP2) later adds an `IEntityTypeConfiguration`-only EF Core reference for entity configs; nothing else.)
- Enforced invariant validation on every entity state change; invalid state unreachable by construction.
- Safe extraction of complex logic into reusable static domain services.
- A consistent `Shared.Exceptions.DomainException` error model that `solution-mediator-exception-handler` maps.

# Core Principle
- **Domain project = this feature** - `{Module}.Domain` is created by this solution and by nothing else; a module without it has no domain layer (its `Application` handlers only orchestrate and shape data).
- The entity is the single point of truth for its own state validity; every mutating method or setter validates first, using a condition written locally (in the method or a `private static` helper on the same class).
- This solution needs no shared rules abstraction — the condition is owned by the entity/service that enforces it. [[skills/dotnet/architecture/v3.1/solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] (VP4) may later centralize a duplicated one, but every method here works standalone.
- Invalid state must never be reachable — throw `DomainException` if attempted.
- Bulky behavior goes to static domain service extension methods in `{Module}.Domain/Services`; a property must not have multiple uncoordinated mutation points.

# Boundaries
- Persistence configuration and repositories are **not** added here. `solution-domain-configuration` (VP2) adds a reference to EF Core **scoped to `IEntityTypeConfiguration`** for the per-entity configs; `solution-repository-integration` adds `DbContext`/repositories in `App.Infrastructure`. Neither is present from this solution alone.
- The strict `{ValueObject}` type (in `{Module}.Domain/ValueObjects`, throwing at construction) is [[skills/dotnet/architecture/v3.1/solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] (VP3), which builds on this solution. Entity properties here use `Soft{ValueObject}` or primitives until VP3 is applied.
- `DomainException` is thrown, not caught, here — `solution-mediator-exception-handler` catches it when applied; this solution does not require it.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj]] - hosts `DomainException`
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj]] - `{Module}.Domain` references it for `Soft{ValueObject}` base types
- [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/solution-soft-value-objects.skill.md|solution-soft-value-objects]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs]] - entity property types

NUGET:
- None.

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj]] - create - the module's domain layer project
  - [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md|{Entity}.cs]] - create - the first entity: `Id`, guarded state transitions
  - [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create/{Behavior}Service.cs.create.md|{Behavior}Service.cs]] - create - static extension methods for bulky entity behavior
- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - extend - add `Exceptions/DomainException.cs`
  - [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/Implementation/Shared.csproj.extend/DomainException.cs.create.md|DomainException.cs]] - create - thrown on any invariant violation

# Rule

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create.md#MUST|{Module}.Domain.csproj]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md#MUST|{Entity}.cs]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create/{Behavior}Service.cs.create.md#MUST|{Behavior}Service.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/Implementation/Shared.csproj.extend/DomainException.cs.create.md#MUST|DomainException.cs]]
- Keep `{Module}.Domain` free of `DbContext`, repositories, and any infrastructure reference; this solution adds no NuGet package.
  - Risk: a `DbContext`/repository reference here makes every domain-bearing module carry persistence, contradicting VP1↔VP2 independence.
  - Fix: `solution-domain-configuration` (VP2) is the only solution that may add an EF Core reference to `{Module}.Domain`, and only `IEntityTypeConfiguration`.

## SHOULD
- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create.md#SHOULD|{Entity}.cs]]

# Check list
- [ ] `{Module}.Domain.csproj` created, referencing only `Shared` + `{Module}.Interfaces`; no NuGet package added by this solution.
- [ ] `{Module}.Domain/Entities` exists with the first entity.
- [ ] `Shared.Exceptions.DomainException` created.
- [ ] Every mutation validates before assigning, using a locally-owned condition; `DomainException` on violation.
- [ ] Bulky logic extracted to `{Module}.Domain/Services` static extension methods.
- [ ] No property has multiple uncoordinated mutation points.
- [ ] No `DbContext`/repository/infrastructure reference in `{Module}.Domain` (an `IEntityTypeConfiguration`-only EF Core reference may be added later by `solution-domain-configuration`).
