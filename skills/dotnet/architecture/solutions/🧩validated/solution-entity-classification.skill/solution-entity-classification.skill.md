---
uid: 7f6e5d4c-3b2a-1f0e-9d8c-7b6a5f4e3d2c
name: solution-entity-classification
description: Defines the four-type entity classification taxonomy for the domain model — Internal Immutable, External Immutable, Internal Mutable, and External Mutable — and maps each type to the exact combination of solution-entity-concurrency-change.skill and solution-external-created-entity.skill that must be applied.
domain: skill
type: architecture
version: 20260616
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - entity
  - classification
  - concurrency
  - guid
  - idempotency
triggers:
  - classify entity
  - entity type
  - internal entity
  - external entity
  - immutable entity
  - mutable entity
  - entity ownership
  - entity mutability
  - which entity solutions to apply
creates:
extends:
  - "{Module}.Domain.Entities.{EntityName}.cs"
  - "{Module}.Domain.Configurations.{EntityName}Config.cs"
  - "{Module}.Interfaces.csproj"
  - "{Module}.Application.csproj"
  - "{Module}.Api.csproj"
depends_on:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity.skill]]"
---

# Goal

- Provide a single, unambiguous decision framework for classifying every domain entity into one of four orthogonal types.
- Map each entity type to the exact subset of `solution-entity-concurrency-change.skill` and `solution-external-created-entity.skill` that must be implemented.
- Prevent over-engineering by forbidding concurrency control on immutable entities and forbidding external-created infrastructure on internal entities.
- Ensure that mutable and/or externally-created entities receive all required infrastructure consistently across every module.
- Make the classification decision explicit and reviewable for every entity before implementation begins.

# Core Principles

- Entity type is determined by two independent axes: **ownership** and **mutability**.
  - **Ownership**: who generates the entity identity at creation.
    - **Internal**: the backend generates the identity (`int Id`).
    - **External**: a client or external system supplies a correlation `Guid` at creation.
  - **Mutability**: whether the entity changes state after creation.
    - **Immutable**: the entity is created once and never updated.
    - **Mutable**: the entity can be updated after creation.
- The four combinations form the complete taxonomy and cover every entity in the domain model.
- Classification is made **per entity**, not per aggregate, module, or bounded context.
- Each classification maps to a deterministic dependency matrix — there are no optional or partial applications.
- Internal identity (`int Id`) is always the primary domain identity, regardless of whether an external `Guid` is also present.
- Concurrency control (`Version`, `IVersioned`, `ConcurrencyBehavior`) is only meaningful for entities that can change state.
- External-created infrastructure (`Guid`, `IHasGuid`, `GuidResolvingBehavior`) is only meaningful for entities whose creation is initiated outside the system.
- The classification decision must be documented and treated as an architecture decision for every entity.

# Entity Type Matrix

| Type | Ownership | Mutability | `entity-concurrency-change` | `external-created-entity` |
|---|---|---|---|---|
| **Internal Immutable** | Internal | Immutable | DO NOT implement | DO NOT implement |
| **External Immutable** | External | Immutable | DO NOT implement | Implement |
| **Internal Mutable** | Internal | Mutable | Implement | DO NOT implement |
| **External Mutable** | External | Mutable | Implement | Implement |

# Requirements

SOLUTION:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change.skill]]
  - Applied only to **Internal Mutable** and **External Mutable** entities.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - provides `Version`, `IVersioned`, and EF `xmin` concurrency token mapping.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Api.csproj.extend.md|{Module}.Api.csproj]] - provides `ETag` and `If-Match` handling for mutable entities.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj]] - provides `IHasVersions` for update/patch commands of mutable entities.
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity.skill]]
  - Applied only to **External Immutable** and **External Mutable** entities.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - provides `Guid` property and unique index configuration.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - provides `{Entity}ByGuidSpec` and `Create{Entity}GuidResolver`.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend.md|{Module}.Interfaces.csproj]] - provides `IHasGuid` for create commands of external entities.

# Template Skill Mutations

This solution classifies each entity and provides the concrete entity class and EF configuration templates for each classification. The actual cross-cutting infrastructure (pipeline behaviors, resolvers, encoders) is reused from the dependency solutions.

PROJECT:
- [[./Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - apply the correct entity class and configuration variant for the selected classification
  - [[./Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md|{EntityName}.cs]] - extend - add `Guid`, `Version`, `IVersioned`, or none based on classification
  - [[./Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs]] - extend - add unique `Guid` index, `xmin` concurrency token, or none based on classification

Apply mutations from dependency solutions for the selected classification:
- `solution-entity-concurrency-change.skill` for **Internal Mutable** and **External Mutable** entities.
- `solution-external-created-entity.skill` for **External Immutable** and **External Mutable** entities.

# Rules

MUST:
- Classify every domain entity into exactly one of the four types before writing its code, configuration, or API contract.
- **Internal Immutable** entities:
  - Have only the internal `int Id` identity.
  - Do not have a `Version` property or implement `IVersioned`.
  - Do not have a `Guid` property or implement `IHasGuid`.
  - Do not apply `solution-entity-concurrency-change.skill` or `solution-external-created-entity.skill`.
- **External Immutable** entities:
  - Have `public Guid Guid { get; internal set; }` set once in the factory method.
  - Have a unique database index on `Guid`.
  - Implement `solution-external-created-entity.skill` fully.
  - Do not have a `Version` property or implement `IVersioned`.
  - Do not apply `solution-entity-concurrency-change.skill`.
- **Internal Mutable** entities:
  - Have `public uint Version { get; internal set; }` mapped to PostgreSQL `xmin`.
  - Implement `IVersioned`.
  - Update and patch commands implement `IHasVersions`.
  - Implement `solution-entity-concurrency-change.skill` fully.
  - Do not have a `Guid` property or implement `IHasGuid`.
  - Do not apply `solution-external-created-entity.skill`.
- **External Mutable** entities:
  - Have both `public Guid Guid { get; internal set; }` and `public uint Version { get; internal set; }`.
  - Implement both `IVersioned` and `IHasGuid` where applicable.
  - Apply both `solution-entity-concurrency-change.skill` and `solution-external-created-entity.skill` fully.
- Document the classification decision for every entity in a discoverable location (e.g., entity config XML comment, module ADR, or team wiki).
- Re-evaluate classification when the entity's ownership or mutability requirements change.

MUST NOT:
- Apply `solution-entity-concurrency-change.skill` to immutable entities.
- Apply `solution-external-created-entity.skill` to internal entities.
- Apply a dependency solution partially or omit required parts for a classified type.
- Use `Guid` as the primary domain identity or foreign key for internal entities.
- Use `Version` / `IVersioned` for entities that never change after creation.
- Leave an entity unclassified or classify it at aggregate/module level instead of entity level.
- Change classification without updating the implemented infrastructure accordingly.

SHOULD:
- Name the classification in entity configuration comments or a dedicated `ENTITY_CLASSIFICATION.md` per module.
- Review classifications during domain model refactoring or story planning.
- Treat external `Guid` as a correlation handle and internal `Id` as the domain identity, even when both are present.

# Anti-patterns
- Adding `Version` to an entity that is never updated — adds complexity and false concurrency semantics.
- Adding `Guid` and unique-index infrastructure to an entity whose creation is fully backend-driven — leaks external identity concepts into internal flows.
- Applying `GuidResolvingBehavior` without `IHasGuid` on commands, or vice versa.
- Applying `ConcurrencyBehavior` to create/delete commands or to commands that target immutable entities.
- Classifying an aggregate root as mutable while its child entities are immutable, then applying mutable-entity infrastructure to the children.
- Using the external `Guid` in routing, relationships, or domain logic after creation.
- Documenting classification only in transient places (chat, PR comments) instead of alongside the entity definition.

# Check list
- [ ] Every domain entity has a documented classification: Internal Immutable, External Immutable, Internal Mutable, or External Mutable.
- [ ] Internal Immutable entities have no `Version` and no `Guid`.
- [ ] External Immutable entities have `Guid` and unique index, and implement `solution-external-created-entity.skill` only.
- [ ] Internal Mutable entities have `Version`, `IVersioned`, and implement `solution-entity-concurrency-change.skill` only.
- [ ] External Mutable entities have both `Version` and `Guid`, and implement both dependency solutions.
- [ ] Create commands for external entities implement `IHasGuid`.
- [ ] Update/patch commands for mutable entities implement `IHasVersions`.
- [ ] Classification is stored in a discoverable location for each entity.
- [ ] Classification was reviewed when entity ownership or mutability changed.

# Unittest TestCases
- [ ] When entity is classified as Internal Immutable Then it has no `Version` property and no `Guid` property.
- [ ] When entity is classified as External Immutable Then it has `Guid` property, unique index, and `IHasGuid` on create command, but no `Version` property.
- [ ] When entity is classified as Internal Mutable Then it has `Version` property, implements `IVersioned`, and update command implements `IHasVersions`, but no `Guid` property.
- [ ] When entity is classified as External Mutable Then it has both `Version` and `Guid` properties, and both create and update commands carry the required markers.
- [ ] When an immutable entity is inspected Then `ConcurrencyBehavior` is not registered for its create command.
- [ ] When an internal entity create command is inspected Then `GuidResolvingBehavior` does not constrain it.
