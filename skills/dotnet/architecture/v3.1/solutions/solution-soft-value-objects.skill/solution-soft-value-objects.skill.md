---
name: solution-soft-value-objects
description: Defines the permissive Soft{ValueObject} type in {Module}.Interfaces — a plain, validation-free record that DTOs, commands, queries, and other modules consume to avoid primitive obsession at the transport boundary. The strict Domain-side {ValueObject} is a separate solution (solution-value-objects, VP3).
whenToUse: when a DTO, command, query, or cross-module value carries business meaning and should not be a bare primitive — declaring the permissive Interfaces-side Soft{ValueObject} shape, or deciding whether a value needs one
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - concern/architecture
  - ddd
  - value-object
  - solution/soft-value-objects
  - stack/dotnet
creates:
  - "{Module}.Interfaces.ValueObjects.Soft{ValueObject}.cs"
extends:
  - "{Module}.Interfaces.csproj"
  - Shared.csproj
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
built_on_plateau:
adr:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/adr/soft-and-strict-value-object-split.md|Soft (permissive) and strict Value Object as two separate solutions]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/adr/response-dto-uses-soft-value-objects.md|ResponseDto/RequestDto use Soft{ValueObject} or primitive, never the domain {ValueObject}]]"
---

# Goal
- Eliminate primitive obsession at the transport boundary by encoding a value's business meaning into a dedicated `Soft{ValueObject}` type.
- Give DTOs, commands, queries, and other modules a value-object-shaped type they can hold without referencing `{Module}.Domain` or accepting throw-on-construct semantics.
- Base equality on value, not reference — two `Soft{ValueObject}` instances with the same data are equal.

# Capabilities
- A stable, validation-agnostic public shape for a domain value, referenceable across module and DTO boundaries.
- Structural equality for free (record type).
- A base type the strict Domain-side `{ValueObject}` (from [[skills/dotnet/architecture/v3.1/solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]]) inherits, so the shape is declared once.

# Core Principle
- **Semantics belong to types** - If a primitive carries business meaning (an email, a money amount, an ISO country code), it gets a `Soft{ValueObject}`; a bare `string`/`decimal` on a public contract is primitive obsession.
- **Soft is permissive on purpose** - `Soft{ValueObject}` is a plain record with no validation — it allows invalid values so a DTO carrying bad client data can still be deserialized and then rejected by the collect-all validator, instead of failing at the boundary with an opaque error.
- **Declared once, in Interfaces** - The shape lives in `{Module}.Interfaces/ValueObjects`; the strict `{ValueObject}` inherits it and adds enforcement, never re-declares it. `{Module}.Interfaces` never references `{Module}.Domain`.
- **Soft can stand alone** - A value that only ever needs the permissive strength (a presentation-only field) gets a `Soft{ValueObject}` with no matching strict `{ValueObject}` — a complete, valid application of this solution.
- **Shared for cross-module values** - A `Soft{ValueObject}` used by two or more modules lives in `Shared`, not duplicated per module.

# Boundaries
- The strict, self-validating `{ValueObject}` (in `{Module}.Domain`, inheriting `Soft{ValueObject}` and throwing `DomainException` at construction) is **not** part of this solution — it is [[skills/dotnet/architecture/v3.1/solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] (VP3), which requires a domain layer.
- FluentValidation of a `Soft{ValueObject}` property is not provided here — [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] owns that.
- Persistence mapping of a `Soft{ValueObject}`/`{ValueObject}` (`OwnsOne`) is owned by `solution-domain-configuration`.

# Adr
- [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/adr/soft-and-strict-value-object-split.md|Soft and strict Value Object as two separate solutions]]
  - Selected variant: `Soft{ValueObject}` (common, Interfaces) and strict `{ValueObject}` (VP3, Domain) are separate solutions, `{ValueObject} : Soft{ValueObject}`.
- [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/adr/response-dto-uses-soft-value-objects.md|ResponseDto/RequestDto use Soft{ValueObject} or primitive]]
  - Selected variant: DTO properties use `Soft{ValueObject}` (or the primitive), never the domain `{ValueObject}`; `{Module}.Application` maps between them.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/{Module}.Interfaces.csproj.create.md|{Module}.Interfaces.csproj]] - hosts `Soft{ValueObject}`
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj]] - hosts cross-module `Soft{ValueObject}` types

NUGET:
- None.

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj]] - extend - add a `ValueObjects/` folder
  - [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs]] - create - permissive value-object record, allows invalid values

# Workflow

## Add a value-object-shaped field
1. Identify a primitive on a public contract (DTO / command / query field, or a cross-module value) that carries business meaning.
2. Declare `Soft{ValueObject}` in `{Module}.Interfaces/ValueObjects` — a `record`, single-property (with an implicit conversion to/from the primitive) or multi-property.
3. If two or more modules use it, move it to `Shared/ValueObjects` instead.
4. DTOs and other modules reference `Soft{ValueObject}`. If the value has a Domain-side invariant, `solution-value-objects` later adds the strict `{ValueObject} : Soft{ValueObject}`.

# Rule

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend.md#MUST|{Module}.Interfaces.csproj]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md#MUST|Soft{ValueObject}.cs]]
- Keep `Soft{ValueObject}` free of any validation, `Check()`, or throw.
  - Risk: a boundary type that rejects invalid input makes a bad-data DTO undeserializable, so the collect-all validator never runs and the caller gets a parse error instead of field-level messages.
  - Fix: `Soft{ValueObject}` is a plain record; validation is `solution-dto-property-validators`' job.
- Declare a cross-module `Soft{ValueObject}` in `Shared`, never once per module.
  - Risk: two modules with their own copy of the same value type cannot exchange it without a mapping, and the copies drift.
  - Fix: one declaration in `Shared/ValueObjects`.
- Never reference `{Module}.Domain` from `{Module}.Interfaces`.
  - Risk: the module's public contract project depends on its internals, and every consumer transitively pulls in the domain layer.
  - Fix: the strict `{ValueObject}` inherits `Soft{ValueObject}`, not the reverse; `Interfaces` stays leaf-level.

## SHOULD
- [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md#SHOULD|Soft{ValueObject}.cs]]
- Give a single-property `Soft{ValueObject}` implicit conversion operators to and from its primitive for ergonomic call sites.
- Give a multi-property `Soft{ValueObject}` a parameterless constructor so EF Core can materialize it later (if a strict `{ValueObject}` is added and persisted).

# Check list
- [ ] `Soft{ValueObject}` is a `record` in `{Module}.Interfaces/ValueObjects` (or `Shared/ValueObjects` if cross-module).
- [ ] It has no validation, no `Check()`, no throw.
- [ ] It has structural equality (record) and no identity.
- [ ] Single-property types expose implicit conversions; multi-property types have a parameterless constructor.
- [ ] `{Module}.Interfaces` does not reference `{Module}.Domain`.
- [ ] No `Soft{ValueObject}` is duplicated across modules.
