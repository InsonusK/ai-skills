---
uid:
name: module-domain-csproj
description: Domain project for a bounded context module — stores entities, value objects, domain rules, domain events, and EF Core configurations. Pure business logic with no infrastructure dependencies.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/csproj
  - dotnet
  - domain
  - module
triggers:
  - create {Module}.Domain project
  - add domain layer
  - implement domain project
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill|01-module-boundary.solution.skill]]"
extended_by:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill|02-solution-layer-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-value-object.solution.skill|04-value-object.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill]]"
aliases:
  - "{Module}.Domain"
  - "{Module}.Domain (.csproj)"
---

# Goal
- Own the business logic, entities, value objects, rules, and domain events for this bounded context
- Store all Value Object types for this bounded context
- Store all domain rule types for this bounded context
- Store all entity types for this bounded context
- Store all EF Core entity type configuration classes for this bounded context
- Prevent invalid domain state by making types self-validating at construction time
- Define a domain entity as an object with stable identity where identity determines equality
- Keep domain entities free of EF attributes and infrastructure concerns

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Domain (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-value-object.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill#{Module}.Domain (.csproj) (extended)]]

# Core Principles
- Domain is the innermost layer — pure business logic, no infrastructure dependencies
- Domain has no knowledge of other modules
- Semantics belong to types, not primitives — if a primitive carries business meaning, it is a VO
- Value Object is immutable — no property can change after construction
- Value Object is self-validating — invalid state cannot exist, constructor throws on violation
- Equality is structural for VOs — two instances with same values are equal
- Rule defines business meaning — not transport behavior, not framework behavior
- Rule returns bool — the caller decides whether to throw
- Rules are stateless, deterministic, and side-effect free
- Entity has stable identity — int Id is always the system primary identity
- Entity has mutable state — unlike Value Objects, state changes over time
- Entity encapsulates behavior — state changes happen through methods, not direct property assignment
- Entity enforces invariants — invalid state must never be reachable
- Id is always internal set — only persistence layer assigns it
- One IEntityTypeConfiguration<T> per entity — no exceptions
- Configuration class owns all persistence concerns — entity owns all domain concerns
- Index and constraint names are public static string constants on the config class
- Domain entity must have zero EF attributes
- Configuration is the only place that knows about column names, table names, and constraints
- All configurations registered via ApplyConfigurationsFromAssembly — never manually

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Domain (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-value-object.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill#{Module}.Domain (.csproj) (extended)]]

# Structure

## Solution place
Defined in [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill]]
```
/src
  /Modules
    /{ModuleName}
      /{ModuleName}.Domain
```

## Project Structure
```
/{ModuleName}.Domain
  /Entities
  /ValueObjects
  /Rules
  /Events
  /Configurations
  {ModuleName}.Domain.csproj
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Domain (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-value-object.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill#{Module}.Domain (.csproj) (extended)]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Entities | All entity types for this module | [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Entity.class.skill\|Entity.class.skill]] |
| /ValueObjects | All Value Object types for this module | [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/ValueObject.class.skill\|ValueObject.class.skill]] |
| /Rules | All domain rule static classes for this module | [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Rules/PrimitiveRule.class.skill\|PrimitiveRule.class.skill]], [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Rules/ValueObjectRule.class.skill\|ValueObjectRule.class.skill]], [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Rules/ContextualRule.class.skill\|ContextualRule.class.skill]] |
| /Events | Domain events raised by this module | |
| /Configurations | One EF config class per entity | [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/EntityConfiguration.class.skill\|EntityConfiguration.class.skill]] |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Domain (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-value-object.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill#{Module}.Domain (.csproj) (extended)]]

## What Does NOT Belong Here
- Transport validation — belongs to module Application validators
- Infrastructure implementations — belongs to App.Infrastructure
- Pipeline behaviors — belongs to BuildingBlocks
- Command/Query handlers — belong to module Application
- Cross-module JOIN queries — belong to App.Queries
- Business logic beyond domain invariants — belongs to domain services within Domain

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Domain (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#App.Infrastructure (.csproj)]]

## Allowed Dependencies
- Shared
- Microsoft.EntityFrameworkCore (IEntityTypeConfiguration only)

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Domain (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Domain (.csproj) (extended)]]

# Rules

MUST:
- Domain depends only on Shared and EF Core (for IEntityTypeConfiguration only)
- All entities live in /{Module}.Domain/Entities
- All Value Objects live in /{Module}.Domain/ValueObjects
- All domain rules live in /{Module}.Domain/Rules
- All EF configuration classes live in /{Module}.Domain/Configurations
- All Value Objects declared as `sealed record`
- All Value Objects immutable — no public setters
- All Value Objects self-validating — throw `DomainException` on invalid construction
- Multi-property VO has `private` parameterless constructor for EF materialization
- Multi-property VO has `OwnsOne` EF configuration on owning entity
- All rules implemented as static extension methods
- Rules return `bool` — caller decides whether to throw
- Rules are stateless and deterministic
- Primitive rule is single source of truth — VO rules delegate to it
- Every entity has `int Id` with `internal set`
- Every entity type selected from the four-type matrix
- Mutable entities have `uint Version` with `internal set`
- External entities have `Guid Guid` with `internal set`
- All property setters are `internal` or `private`
- One config class per entity — no shared configs
- All configurations registered via `ApplyConfigurationsFromAssembly` in DbContext

SHOULD:
- Single-property VO has implicit conversion operators
- All VOs override `ToString()` when used in logs or UI
- Rules be synchronous
- Rules avoid allocations

MUST NOT:
- Domain reference any other module's project
- Domain use EF Core beyond `IEntityTypeConfiguration`
- Value Object depend on infrastructure, repositories, or application services
- Value Object expose public setters
- Value Object be used to carry identity
- Primitive used in place of VO when the primitive carries business meaning
- Rule throw exceptions internally
- Rule depend on EF Core, FluentValidation, ASP.NET, HttpContext, or any infrastructure
- Rule mutate any object
- Rule duplicate logic that already exists in another rule
- Rule be instantiated with `new`
- Use `public` setters on any entity property
- Use `Guid` as primary identity
- Place entities outside the Domain project
- Use `long` or `string` as primary key without explicit justification
- Place EF config classes outside /Configurations folder
- Use EF data annotations on domain entity classes
- Put mapping logic directly in `DbContext.OnModelCreating`
- Configure cross-module foreign keys in Domain config

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Domain (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-value-object.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill#{Module}.Domain (.csproj) (extended)]]

# Anti-patterns
- Injecting DbContext into a domain class — domain has no persistence dependency
- Referencing another module's Domain for shared entity types — each module owns its own entities
- Using EF Core attributes on domain entities — use configuration classes instead
- `string Email` on entity instead of `Email` VO — loses invariant enforcement
- VO with public setter — allows post-construction mutation
- VO with infrastructure dependency — couples domain to persistence layer
- Reusing same VO type across modules — each module defines its own VO types
- Rule throws `DomainException` itself — rule returns `bool`, caller throws
- `new CanDriveCarRule().IsSatisfied()` — rules are static, never instantiated
- VO rule reimplements primitive rule logic — always delegate to primitive overload
- Same business condition checked in controller, validator, entity, and service separately — define once as rule
- `public string Title { get; set; }` — public setter allows invalid state
- Using `Guid` as the primary key — internal `int Id` is always primary
- Skipping the type matrix — leads to missing Version or Guid
- Placing entity in Application or Interfaces project — entities belong in Domain only
- Annotating domain entity with `[Column]`, `[Index]`, `[ForeignKey]` — all mapping belongs in config class
- Registering configs manually one by one in DbContext — use `ApplyConfigurationsFromAssembly`
- Sharing one config class across multiple entity types
- Cross-module FK configured in Domain config — belongs in App.Infrastructure
- Mapping multi-property VO without `OwnsOne` — EF will fail to map or create a shadow table
- Hardcoded index name strings — breaks error handling that matches constraint names

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Domain (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-value-object.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill#{Module}.Domain (.csproj) (extended)]]

# Check list
- [ ] Domain.csproj references only Shared and EF Core
- [ ] No DbContext reference in any domain class
- [ ] No cross-module domain references
- [ ] /Entities folder exists
- [ ] /ValueObjects folder exists
- [ ] /Rules folder exists
- [ ] /Configurations folder exists
- [ ] Declared as `sealed record` for VOs
- [ ] All invariants validated in constructor for VOs
- [ ] `DomainException` thrown on VO violation
- [ ] No public setters on VOs
- [ ] Single-property VO has implicit conversion operators
- [ ] Multi-property VO has private parameterless constructor
- [ ] Multi-property VO has `OwnsOne` EF configuration
- [ ] Rule is a static class with static extension methods
- [ ] Rule returns `bool` — never throws
- [ ] Primitive rule exists as source of truth where applicable
- [ ] VO rule delegates to primitive rule
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present on entities
- [ ] All property setters are `internal` or `private`
- [ ] Mutable entity has `uint Version` with `internal set`
- [ ] External entity has `Guid Guid` with `internal set`
- [ ] One config class per entity in /Configurations
- [ ] All index names defined as `public static string` constants
- [ ] All unique indexes use `HasDatabaseName(ConstantName)`
- [ ] All intra-module relations configured
- [ ] `OwnsOne` configured for every multi-property VO
- [ ] No EF attributes on any domain entity
- [ ] Configurations registered via `ApplyConfigurationsFromAssembly`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/01-module-boundary.solution.skill#{Module}.Domain (.csproj)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-value-object.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-entity-base.solution.skill#{Module}.Domain (.csproj) (extended)]]
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/03-domain-configuration.solution.skill#{Module}.Domain (.csproj) (extended)]]
