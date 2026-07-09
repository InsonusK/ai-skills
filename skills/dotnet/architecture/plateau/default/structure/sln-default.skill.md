---
name: sln-default
description: Default plateau — full solution architecture composed from all validated v3 architecture solutions
domain: skill
type: template
version: 20260629223200
plateau: default
tags:
  - skill/template/sln
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]"
---

# Structure

## Repository Structure
- /src
  - /Modules
    - /{ModuleName}
      - /[{ModuleName}.Api](skills/dotnet/architecture/plateau/default/structure/{Module}.Api/csproj-module-api.skill.md)
      - /[{ModuleName}.Application](skills/dotnet/architecture/plateau/default/structure/{Module}.Application/csproj-module-application.skill.md)
      - /[{ModuleName}.Domain](skills/dotnet/architecture/plateau/default/structure/{Module}.Domain/csproj-module-domain.skill.md)
      - /[{ModuleName}.Interfaces](skills/dotnet/architecture/plateau/default/structure/{Module}.Interfaces/csproj-module-interfaces.skill.md)
      - /{ModuleName}.Api.Tests
      - /{ModuleName}.Application.Tests
      - /{ModuleName}.Domain.Tests
      - /{ModuleName}.Integration.Tests
  - /App
    - /[App.Host](skills/dotnet/architecture/plateau/default/structure/App.Host/csproj-app-host.skill.md)
    - /[App.Infrastructure](skills/dotnet/architecture/plateau/default/structure/App.Infrastructure/csproj-app-infrastructure.skill.md)
    - /[App.Infrastructure.Migrations](skills/dotnet/architecture/plateau/default/structure/App.Infrastructure.Migrations/csproj-app-infrastructure-migrations.skill.md)
    - /[App.Queries](skills/dotnet/architecture/plateau/default/structure/App.Queries/csproj-app-queries.skill.md)
  - /[Shared](skills/dotnet/architecture/plateau/default/structure/Shared/csproj-shared.skill.md)
  - /[BuildingBlocks](skills/dotnet/architecture/plateau/default/structure/BuildingBlocks/csproj-building-blocks.skill.md)

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Repository.create|Repository]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]

## Directory and class skills
| `Directory\|file`              | template link                                                                                                                                                                                 | Description                                                    |
| ------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| /Shared                        | [[skills/dotnet/architecture/plateau/default/structure/Shared/csproj-shared.skill\|csproj-Shared.skill]]                                                                      | Cross-cutting primitives — Result, Exceptions, base interfaces |
| /BuildingBlocks                | [[skills/dotnet/architecture/plateau/default/structure/BuildingBlocks/csproj-building-blocks.skill\|csproj-BuildingBlocks.skill]]                                              | Reusable framework patterns — pipeline behaviors, ETag encoder |
| /App.Host                      | [[skills/dotnet/architecture/plateau/default/structure/App.Host/csproj-app-host.skill\|csproj-App.Host.skill]]                                                                | Composition root — DI, pipeline, module wiring                 |
| /App.Infrastructure            | [[skills/dotnet/architecture/plateau/default/structure/App.Infrastructure/csproj-app-infrastructure.skill\|csproj-App.Infrastructure.skill]]                                  | Persistence — DbContext, repos, outbox, version resolver factory |
| /App.Infrastructure.Migrations | [[skills/dotnet/architecture/plateau/default/structure/App.Infrastructure.Migrations/csproj-app-infrastructure-migrations.skill\|csproj-App.Infrastructure.Migrations.skill]] | EF Core migrations only                                        |
| /App.Queries                   | [[skills/dotnet/architecture/plateau/default/structure/App.Queries/csproj-app-queries.skill\|csproj-App.Queries.skill]]                                                       | Cross-module read models and JOIN queries                      |
| /{Module}.Interfaces           | [[skills/dotnet/architecture/plateau/default/structure/{Module}.Interfaces/csproj-module-interfaces.skill\|csproj-{Module}.Interfaces.skill]]                               | Public contracts — commands, queries, DTOs, events, soft VOs   |
| /{Module}.Domain               | [[skills/dotnet/architecture/plateau/default/structure/{Module}.Domain/csproj-module-domain.skill\|csproj-{Module}.Domain.skill]]                                           | Business logic — entities, VOs (inherit from soft VOs), rules, events |
| /{Module}.Application          | [[skills/dotnet/architecture/plateau/default/structure/{Module}.Application/csproj-module-application.skill\|csproj-{Module}.Application.skill]]                            | Orchestration — handlers, validators, specs, version resolvers |
| /{Module}.Api                  | [[skills/dotnet/architecture/plateau/default/structure/{Module}.Api/csproj-module-api.skill\|csproj-{Module}.Api.skill]]                                                    | HTTP endpoints, MediatR dispatch, ETag/If-Match handling       |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/Repository.create|Repository]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]

# Goal

- Provide a single, unambiguous decision framework for classifying every domain entity into one of four orthogonal types.
- Map each entity type to the exact subset of `solution-entity-concurrency-change.skill` and `solution-external-created-entity.skill` that must be implemented.
- Prevent over-engineering by forbidding concurrency control on immutable entities and forbidding external-created infrastructure on internal entities.
- Ensure that mutable and/or externally-created entities receive all required infrastructure consistently across every module.
- Ensure timestamped entities receive the correct creation and/or update timestamp contracts based on classification.
- Make the classification decision explicit and reviewable for every entity before implementation begins.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]

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
- Timestamp infrastructure (`ICreationInfoModel`, `IUpdateInfoModel`, `ICommandWithTimestamp`) is applied to user-initiated entities based on mutability: creation-only for immutable entities, creation and update for mutable entities.
- Internal Immutable entities have no timestamp interfaces because they are not user-initiated creation points in this architecture.
- The classification decision must be documented and treated as an architecture decision for every entity.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]

# Entity Type Matrix

| Type | Ownership | Mutability | `entity-concurrency-change` | `external-created-entity` | `entity-edit-timestamp` |
|---|---|---|---|---|---|
| **Internal Immutable** | Internal | Immutable | DO NOT implement | DO NOT implement | DO NOT implement |
| **External Immutable** | External | Immutable | DO NOT implement | Implement | Implement creation only |
| **Internal Mutable** | Internal | Mutable | Implement | DO NOT implement | Implement creation + update |
| **External Mutable** | External | Mutable | Implement | Implement | Implement creation + update |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]

# Requirements

SOLUTION:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]]
  - Applied only to **Internal Mutable** and **External Mutable** entities.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - provides `IVersioned`, `IHasVersions`, `IEntityVersionResolverFactory`, and `IEntityVersionResolver` concurrency contracts.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/BuildingBlocks.csproj.extend|BuildingBlocks.csproj]] - provides `ETagEncoder` and `ConcurrencyBehavior`.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend|App.Infrastructure.csproj]] - provides `EntityVersionResolverFactory` that maps stable business entity names to Application-layer resolvers.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - registers `IEntityVersionResolverFactory` and all module `IEntityVersionResolver` implementations.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]] - provides `Version`, `IVersioned`, and EF `xmin` concurrency token mapping.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Application.csproj.extend|{Module}.Application.csproj]] - provides `{Entity}VersionResolver` for each mutable entity.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Interfaces.csproj.extend|{Module}.Interfaces.csproj]] - provides `IHasVersions` for update/patch commands of mutable entities.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/{Module}.Api.csproj.extend|{Module}.Api.csproj]] - provides `ETag` and `If-Match` handling for mutable entities.
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]]
  - Applied only to **External Immutable** and **External Mutable** entities.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]] - provides `Guid` property and unique index configuration.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend|{Module}.Application.csproj]] - provides `{Entity}ByGuidSpec` and `Create{Entity}GuidResolver`.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Interfaces.csproj.extend|{Module}.Interfaces.csproj]] - provides `IHasGuid` for create commands of external entities.
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]
  - Applied to **External Immutable**, **Internal Mutable**, and **External Mutable** entities.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend|Shared.csproj]] - provides `ICreationInfoModel`, `IUpdateInfoModel`, and `ICommandWithTimestamp` contracts.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend|{Module}.Domain.csproj]] - adds timestamp properties and explicit interface implementation.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Interfaces.csproj.extend|{Module}.Interfaces.csproj]] - adds `ICommandWithTimestamp` to create/update commands.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/{Module}.Application.csproj.extend|{Module}.Application.csproj]] - validates `ActionTimeStamp` and assigns user timestamps in handlers.
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend|App.Infrastructure.csproj]] - assigns server timestamps in `AppDbContext.OnBeforeSaving`.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]

# Rules

MUST:
- Classify every domain entity into exactly one of the four types before writing its code, configuration, or API contract.
- **Internal Immutable** entities:
  - Have only the internal `int Id` identity.
  - Do not have a `Version` property or implement `IVersioned`.
  - Do not have a `Guid` property or implement `IHasGuid`.
  - Do not implement timestamp interfaces and do not map timestamp columns.
  - Do not apply `solution-entity-concurrency-change.skill`, `solution-external-created-entity.skill`, or `solution-entity-edit-timestamp.skill`.
- **External Immutable** entities:
  - Have `public Guid Guid { get; internal set; }` set once in the factory method.
  - Have a unique database index on `Guid`.
  - Implement `solution-external-created-entity.skill` fully.
  - Implement `ICreationInfoModel`; map `ServerCreatedDateTime` and `UserCreatedDateTime` as required columns.
  - Do not have a `Version` property or implement `IVersioned`.
  - Do not apply `solution-entity-concurrency-change.skill`.
  - Do not map update timestamps.
- **Internal Mutable** entities:
  - Have `public uint Version { get; internal set; }` mapped to PostgreSQL `xmin`.
  - Implement `IVersioned`.
  - Update and patch commands implement `IHasVersions`.
  - Implement `solution-entity-concurrency-change.skill` fully.
  - Implement `ICreationInfoModel` and `IUpdateInfoModel`; map all four timestamp columns as required.
  - Create and update commands implement `ICommandWithTimestamp`.
  - Do not have a `Guid` property or implement `IHasGuid`.
  - Do not apply `solution-external-created-entity.skill`.
- **External Mutable** entities:
  - Have both `public Guid Guid { get; internal set; }` and `public uint Version { get; internal set; }`.
  - Implement both `IVersioned` and `IHasGuid` where applicable.
  - Apply both `solution-entity-concurrency-change.skill` and `solution-external-created-entity.skill` fully.
  - Implement `ICreationInfoModel` and `IUpdateInfoModel`; map all four timestamp columns as required.
  - Create and update commands implement `ICommandWithTimestamp`.
- Document the classification decision for every entity in a discoverable location (e.g., entity config XML comment, module ADR, or team wiki).
- Re-evaluate classification when the entity's ownership or mutability requirements change.

MUST NOT:
- Apply `solution-entity-concurrency-change.skill` to immutable entities.
- Apply `solution-external-created-entity.skill` to internal entities.
- Apply `solution-entity-edit-timestamp.skill` to `Internal Immutable` entities or omit update timestamps from mutable entities.
- Apply a dependency solution partially or omit required parts for a classified type.
- Use `Guid` as the primary domain identity or foreign key for internal entities.
- Use `Version` / `IVersioned` for entities that never change after creation.
- Leave an entity unclassified or classify it at aggregate/module level instead of entity level.
- Change classification without updating the implemented infrastructure accordingly.

SHOULD:
- Name the classification in entity configuration comments or a dedicated `ENTITY_CLASSIFICATION.md` per module.
- Review classifications during domain model refactoring or story planning.
- Treat external `Guid` as a correlation handle and internal `Id` as the domain identity, even when both are present.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]

# Anti-patterns

- Adding `Version` to an entity that is never updated — adds complexity and false concurrency semantics.
- Adding `Guid` and unique-index infrastructure to an entity whose creation is fully backend-driven — leaks external identity concepts into internal flows.
- Applying `GuidResolvingBehavior` without `IHasGuid` on commands, or vice versa.
- Applying `ConcurrencyBehavior` to create/delete commands or to commands that target immutable entities.
- Mapping timestamp columns on `Internal Immutable` entities.
- Mapping update timestamps on `External Immutable` entities.
- Classifying an aggregate root as mutable while its child entities are immutable, then applying mutable-entity infrastructure to the children.
- Using the external `Guid` in routing, relationships, or domain logic after creation.
- Documenting classification only in transient places (chat, PR comments) instead of alongside the entity definition.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]

# Check list

- [ ] Every domain entity has a documented classification: Internal Immutable, External Immutable, Internal Mutable, or External Mutable.
- [ ] Internal Immutable entities have no `Version` and no `Guid`.
- [ ] External Immutable entities have `Guid` and unique index, and implement `solution-external-created-entity.skill` only.
- [ ] Internal Mutable entities have `Version`, `IVersioned`, and implement `solution-entity-concurrency-change.skill` only.
- [ ] External Mutable entities have both `Version` and `Guid`, and implement both dependency solutions.
- [ ] External Immutable entities implement `ICreationInfoModel` and map creation timestamps only.
- [ ] Internal Mutable and External Mutable entities implement `ICreationInfoModel` and `IUpdateInfoModel`, map all four timestamps, and commands implement `ICommandWithTimestamp`.
- [ ] Internal Immutable entities have no timestamp interfaces or columns.
- [ ] Create commands for external entities implement `IHasGuid`.
- [ ] Update/patch commands for mutable entities implement `IHasVersions`.
- [ ] Classification is stored in a discoverable location for each entity.
- [ ] Classification was reviewed when entity ownership or mutability changed.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]

# Unittest TestCases

- [ ] When entity is classified as Internal Immutable Then it has no `Version` property and no `Guid` property.
- [ ] When entity is classified as External Immutable Then it has `Guid` property, unique index, and `IHasGuid` on create command, but no `Version` property.
- [ ] When entity is classified as Internal Mutable Then it has `Version` property, implements `IVersioned`, and update command implements `IHasVersions`, but no `Guid` property.
- [ ] When entity is classified as External Mutable Then it has both `Version` and `Guid` properties, and both create and update commands carry the required markers.
- [ ] When entity is classified as Internal Immutable Then it has no timestamp interfaces or columns.
- [ ] When entity is classified as External Immutable Then it implements `ICreationInfoModel` and maps creation timestamps only.
- [ ] When entity is classified as Internal Mutable or External Mutable Then it implements both timestamp interfaces and commands carry `ICommandWithTimestamp`.
- [ ] When an immutable entity is inspected Then `ConcurrencyBehavior` is not registered for its create command.
- [ ] When an internal entity create command is inspected Then `GuidResolvingBehavior` does not constrain it.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/solution-entity-classification.skill|solution-entity-classification]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]

