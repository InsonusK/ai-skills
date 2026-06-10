---
uid: f5aeeabe-b8cc-4378-aeac-cbaa44d06b6b
name: domain-configuration
description: Defines the EF Core entity type configuration pattern — one configuration class per entity that owns all persistence concerns, keeping domain entities free of infrastructure attributes
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - ddd
  - ef-core
  - configuration
triggers:
  - create ef configuration
  - configure entity mapping
  - define database schema
  - configure index
  - configure relation
creates:
  - "{Module}.Domain.Configurations.{Entity}Config.cs"
  - App.Infrastructure.Persistence.Configurations.{Module1}To{Module2}Config.cs
extends:
  - "{Module}.Domain.csproj"
  - "{Module}.Domain.Entities.Entity.cs"
  - App.Infrastructure.csproj
depends_on:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/solution-structure.solution.skill.md|solution-structure.solution.skill]]"
---

# Goal
- Define one EF Core configuration class per entity that owns all persistence concerns — indexes, relations, concurrency tokens, and value object mappings
- Keep domain entities free of EF attributes and infrastructure concerns
- Ensure all index and constraint names are constants — never magic strings — so they can be referenced in tests and error handling
- Register all configurations via assembly scan — never manually per entity

# Core Principals
- One `IEntityTypeConfiguration<T>` per entity — no exceptions
- Configuration class owns all persistence concerns — entity owns all domain concerns
- Index and constraint names are `public static string` constants on the config class
- Domain entity must have zero EF attributes (`[Column]`, `[Index]`, `[ForeignKey]`, etc.)
- Configuration is the only place that knows about column names, table names, and constraints
- All configurations registered via `ApplyConfigurationsFromAssembly` — never manually
- Cross-module foreign key configurations live in App.Infrastructure — not in Domain config

# Requirements
- definition of `Module project` — [[solution-structure.solution.skill]] defines the module projects that this solution extends
- definition of `App.Infrastructure project` — [[solution-structure.solution.skill]] defines App.Infrastructure where cross-module FK configs live
- definition of `Entity` — [[skills/dotnet/skill-graph/developing v3/architecture/solutions/solution-structure.solution.skill/solution-structure.solution.skill|solution-structure.solution.skill]] defines the entity pattern this configuration extends

# Template Skill Mutations

PROJECT:
- [[./Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - Add Configurations folder and EF config pattern
	- [[./Implementation/{Entity}Config.cs.create.md|{Entity}Config.cs]] - create - One EF config class per entity
	- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/domain-configuration.solution.skill/Implementation/{Entity}.cs.extend|{Entity}.cs]] - extend - Zero EF attributes on domain entity
- [[./Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - extend - Add cross-module FK configuration support
	- [[./Implementation/{Module1}To{Module2}Config.cs.create.md|{Module1}To{Module2}Config.cs]] - create - Cross-module FK configuration class

# Rules

MUST:
- One `IEntityTypeConfiguration<T>` per entity
- All index and constraint names defined as `public static string` constants
- `OwnsOne` configured for every multi-property VO property
- All configurations registered via `ApplyConfigurationsFromAssembly`
- Domain entities have zero EF attributes

MUST NOT:
- Use EF data annotations on domain entities
- Define constraint names as inline strings
- Put mapping logic in `DbContext.OnModelCreating` directly
- Configure cross-module foreign keys in Domain config

# Anti-patterns
- Mapping multi-property VO without `OwnsOne` — EF will fail to map or create a shadow table
- Hardcoded index name strings — breaks error handling that matches constraint names
- Using `[ConcurrencyCheck]` attribute on entity instead of fluent config
- Registering configs manually in DbContext — use `ApplyConfigurationsFromAssembly`
- Annotating domain entity with `[Column]`, `[Index]`, `[ForeignKey]` — all mapping belongs in config class
- Single config class shared across multiple entities — one config per entity, no exceptions
- Cross-module FK configured in Domain config — belongs in App.Infrastructure

# Check list
- [ ] One config class per entity in /{Module}.Domain/Configurations
- [ ] All index names defined as `public static string` constants
- [ ] All unique indexes use `HasDatabaseName(ConstantName)`
- [ ] All intra-module relations configured
- [ ] `OwnsOne` configured for every multi-property VO
- [ ] No EF attributes on any domain entity
- [ ] Configurations registered via `ApplyConfigurationsFromAssembly`
- [ ] Cross-module FK configs live in App.Infrastructure/Persistence/Configurations
- [ ] DbContext uses `ApplyConfigurationsFromAssembly` on all module Domain assemblies

# Unittest TestCases
- [ ] When insert entity with duplicate unique-indexed field Then DbUpdateException thrown with constraint name matching constant
- [ ] When insert entity with multi-property VO Then VO columns persisted flat on entity table
- [ ] When entity with relation loaded Then navigation property returns correct related entities
- [ ] When insert entity with duplicate unique-indexed field Then throws DbUpdateException with correct constraint name matching the constant
- [ ] When insert entity with multi-property VO Then all VO columns are persisted flat on entity table
- [ ] When entity relation configured Then navigating the relation returns correct related entities
