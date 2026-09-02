---
name: plateau-offline-sync-service--csproj-module-domain
description: Project {Module}.Domain in the plateau-offline-sync-service plateau — the module's domain layer (guarded entities, strict Value Objects, domain services, EF entity configurations)
whenToUse: when adding or editing an entity, a strict Value Object, a domain service, or an EF entity configuration in {Module}.Domain, or deciding whether logic belongs in the domain layer
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/offline-sync-service
created_by:
  - "[[../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
  - "[[../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]]"
  - "[[../../../../solutions/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]]"
---

# Goal
- Own the entities, strict Value Objects, and domain services for one bounded context — the only layer that contains entity definitions.
- Own each entity's EF Core `IEntityTypeConfiguration<T>` (persistence mapping), keeping the entity itself free of EF attributes.
- Exist only for a module that has a domain layer (VP1); a module without one has just `Interfaces` + `Application`.

__Applied solutions:__
- [[../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[../../../../solutions/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[../../../../solutions/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj.extend]]

# Core Principles
- Entities in `/Entities`, strict Value Objects in `/ValueObjects`, domain services in `/Services`, EF configs in `/Configurations` — one type per file.
- An entity has no public setter for guarded state; every mutating method validates via a locally-owned condition and throws `DomainException`.
- A strict `{ValueObject}` is `sealed record {ValueObject} : Soft{ValueObject}` — reuses the Soft shape, validates in its constructor.
- A domain service is a `static class` of extension methods on the entity; it validates locally and mutates only through the entity's guarded methods.
- References only `Shared` and `{Module}.Interfaces`, plus `Microsoft.EntityFrameworkCore` scoped to `IEntityTypeConfiguration` — never `DbContext`, never a repository.

__Applied solutions:__
- [[../../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] - [[../../../../solutions/solution-value-objects.skill/Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.create.md|{ValueObject}.cs.create]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Domain
```

## Project Structure
- /{ModuleName}.Domain
  - /Entities/[{Entity}.cs](./classes/plateau-offline-sync-service--class-entity.skill.md)
  - /ValueObjects/[{ValueObject}.cs](./classes/plateau-offline-sync-service--class-value-object.skill.md)
  - /Services/[{Behavior}Service.cs](./classes/plateau-offline-sync-service--class-behavior-service.skill.md)
  - /Configurations/[{Entity}Config.cs](./classes/plateau-offline-sync-service--class-entity-config.skill.md)
  - {ModuleName}.Domain.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Entities/{Entity}.cs | Entity with guarded state transitions | [[./classes/plateau-offline-sync-service--class-entity.skill.md\|class-entity]] |
| /ValueObjects/{ValueObject}.cs | Strict, self-validating value record | [[./classes/plateau-offline-sync-service--class-value-object.skill.md\|class-value-object]] |
| /Services/{Behavior}Service.cs | Static extension methods for bulky behavior | [[./classes/plateau-offline-sync-service--class-behavior-service.skill.md\|class-behavior-service]] |
| /Configurations/{Entity}Config.cs | EF Core persistence mapping for one entity | [[./classes/plateau-offline-sync-service--class-entity-config.skill.md\|class-entity-config]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| Microsoft.EntityFrameworkCore | central | `IEntityTypeConfiguration<T>`, `EntityTypeBuilder<T>` only |

## What Does NOT Belong Here
- Orchestration, handlers, validators — belong to [[../{Module}.Application/plateau-offline-sync-service--csproj-module-application.skill.md|{Module}.Application]].
- `DbContext`, repositories, `SaveChangesAsync` — belong to [[../App.Infrastructure/plateau-offline-sync-service--csproj-app-infrastructure.skill.md|App.Infrastructure]].
- Cross-module foreign-key configurations — belong to `App.Infrastructure/Persistence/Configurations`.
- Public DTOs, commands, Soft Value Objects — belong to `{Module}.Interfaces`.

## Allowed Dependencies
- `Shared`, `{Module}.Interfaces`
- NuGet: `Microsoft.EntityFrameworkCore` (`IEntityTypeConfiguration` only)

# Rules
MUST:
- Reference only `Shared` and `{Module}.Interfaces`; the only permitted NuGet is EF Core, used only for `IEntityTypeConfiguration`.
- Place every entity in `/Entities`, strict VO in `/ValueObjects`, domain service in `/Services`, config in `/Configurations`.
- Keep every entity free of EF attributes (`[Table]`, `[Column]`, `[Key]`, `[Index]`, `[ForeignKey]`, `[ConcurrencyCheck]`) — all mapping in the config class.
- Give each config a `public const string TableName` and `public const string` index/constraint names; register configs via `ApplyConfigurationsFromAssembly`, never by hand.
- Never reference another module's `Domain` or `Application`; never add a `DbContext`/repository reference; never configure a cross-module FK here.

__Applied solutions:__
- [[../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create.md|{Module}.Domain.csproj.create]]
- [[../../../../solutions/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[../../../../solutions/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs.create]]

# Check list
- [ ] `{Module}.Domain.csproj` references only `Shared` + `{Module}.Interfaces` (+ EF Core for configs).
- [ ] `/Entities`, `/ValueObjects`, `/Services`, `/Configurations` folders present.
- [ ] No EF attributes on any entity; one config per entity with `const` names.
- [ ] No `DbContext`/repository reference; no cross-module `Domain`/`Application` reference.
