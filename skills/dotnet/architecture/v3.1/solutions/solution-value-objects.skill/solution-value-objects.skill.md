---
name: solution-value-objects
description: Defines the strict Domain-side {ValueObject} (VP3) — an immutable type in {Module}.Domain that inherits the permissive Soft{ValueObject} and enforces its invariant at construction, throwing DomainException on violation. Requires a domain layer (VP1).
whenToUse: when a value on an entity property carries a Domain-side invariant that must be unbreakable — creating the strict {ValueObject} that inherits Soft{ValueObject} and validates in its constructor
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - concern/architecture
  - domain
  - ddd
  - value-object
  - solution/value-objects
  - stack/dotnet
creates:
  - "{Module}.Domain.ValueObjects.{ValueObject}.cs"
extends:
  - "{Module}.Domain.csproj"
  - "{Module}.Domain.Entities.{EntityName}.cs"
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/solution-soft-value-objects.skill.md|solution-soft-value-objects]]"
built_on_plateau:
adr:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/adr/soft-and-strict-value-object-split.md|Soft and strict Value Object as two separate solutions]]"
---

# Goal
- Give a domain concept a strict, self-validating type in `{Module}.Domain` — invalid instances cannot be constructed.
- Reuse the shape declared once as `Soft{ValueObject}` (`{ValueObject} : Soft{ValueObject}`), never re-declaring it.
- Keep the invariant condition local to the `{ValueObject}` (a `private static` predicate), so this solution stands alone; [[skills/dotnet/architecture/v3.1/solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] (VP4) may later centralize it.

# Capabilities
- Entity properties typed as `{ValueObject}` — the type system guarantees the invariant holds for any value stored.
- Structural equality (inherited from the `Soft{ValueObject}` record).
- A single failure model: `Shared.Exceptions.DomainException` on construction with an invalid value.

# Core Principle
- **Requires a domain layer** - `{ValueObject}` lives in `{Module}.Domain/ValueObjects` — this solution is VP3 and `depends_on solution-domain-behaviour` (VP1). Per the [Variability Map](skills/dotnet/architecture/v3.1/variability-map.md), VP3 requires VP1 (directly, or transitively via Persistence).
- `{ValueObject}` inherits `Soft{ValueObject}` and adds only the constructor validation — it never re-declares the properties.
- `{ValueObject}` is immutable, has no identity, and validates via its own `private static` predicate, throwing `DomainException` on failure.
- A multi-property `{ValueObject}` keeps the `protected` parameterless constructor its `Soft` base declares, for EF Core materialization once persisted.
- DTOs and other modules still reference `Soft{ValueObject}`, never `{ValueObject}` (see the `response-dto-uses-soft-value-objects` ADR in `solution-soft-value-objects`).

# Boundaries
- The permissive `Soft{ValueObject}` base is `solution-soft-value-objects` (common); this solution only adds the strict subtype.
- Persistence mapping (`OwnsOne`) of a `{ValueObject}` is `solution-domain-configuration` (VP2).
- Whether the `{ValueObject}`'s condition matches the same concept's boundary validator (`solution-dto-property-validators`) is not enforced here — `solution-domain-rules` (VP4) is the mechanism for one shared declaration.

# Adr
- [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/adr/soft-and-strict-value-object-split.md|Soft and strict Value Object as two separate solutions]]
  - Selected variant: `{ValueObject} : Soft{ValueObject}`, Soft common in Interfaces, strict a VP3 solution in Domain.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj]] - the project `{ValueObject}` is created in
  - [[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/Implementation/Shared.csproj.extend/DomainException.cs.create.md|DomainException.cs]] - thrown on an invalid value
- [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/solution-soft-value-objects.skill.md|solution-soft-value-objects]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs]] - the base type `{ValueObject}` inherits

NUGET:
- None.

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - add a `/ValueObjects` folder, reference `{Module}.Interfaces`
  - [[skills/dotnet/architecture/v3.1/solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs]] - create - strict type, inherits `Soft{ValueObject}`, validates in constructor
  - [[skills/dotnet/architecture/v3.1/solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs]] - extend - use `{ValueObject}` on entity properties

# Workflow

## Add a strict Value Object
1. `Soft{ValueObject}` already exists in `{Module}.Interfaces` (from `solution-soft-value-objects`).
2. Create `{ValueObject} : Soft{ValueObject}` in `{Module}.Domain/ValueObjects`; its constructor calls a `private static bool IsValid(...)` and throws `DomainException("{Module}.{Concept}.Invalid", "...")` on failure.
3. Change the owning entity's property type from `Soft{ValueObject}`/primitive to `{ValueObject}`.
4. `{Module}.Application` maps `{ValueObject}` → `Soft{ValueObject}` when producing a DTO.

# Rule

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend.md#MUST|{Module}.Domain.csproj]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md#MUST|{ValueObject}.cs]]
- Declare `{ValueObject}` as inheriting `Soft{ValueObject}` — never re-declare the properties.
  - Risk: a parallel declaration drifts from the base and breaks the `Soft` → strict mapping.
  - Fix: `public record {ValueObject} : Soft{ValueObject}` adding only a validating constructor.
- Validate in the constructor and throw `DomainException` — never expose a way to hold an invalid `{ValueObject}`.
  - Risk: a bypassable validator lets invalid domain state exist, defeating the point of the type.
  - Fix: the only constructor validates; there is no other path to an instance.
- Keep `{ValueObject}` free of identity, mutation, and infrastructure references.
  - Risk: a mutable or identity-bearing "value object" is really an entity, and an infrastructure reference couples `{Module}.Domain` to persistence.
  - Fix: immutable record, value equality, BCL + `{Module}.Interfaces` + `Shared` only.

# Check list
- [ ] `{ValueObject}` is `record {ValueObject} : Soft{ValueObject}` in `{Module}.Domain/ValueObjects`.
- [ ] Constructor validates via a `private static` predicate and throws `DomainException` on failure.
- [ ] No public setters, no identity, no infrastructure reference.
- [ ] Owning entity properties are typed `{ValueObject}` where the value carries a Domain invariant.
- [ ] DTOs/other modules still use `Soft{ValueObject}`.
