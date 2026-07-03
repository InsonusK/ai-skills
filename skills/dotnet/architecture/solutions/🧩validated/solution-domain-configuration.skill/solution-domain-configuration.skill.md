---
name: solution-domain-configuration
description: Defines the EF Core entity type configuration pattern — one configuration class per entity that owns all persistence concerns, keeping domain entities free of infrastructure attributes
domain: skill
type: architecture
version: 20260611
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
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]"
---

# Goal
- Define one EF Core configuration class per entity that owns all persistence concerns — indexes, relations, concurrency tokens, and value object mappings
- Keep domain entities free of EF attributes and infrastructure concerns
- Ensure all index and constraint names are constants — never magic strings — so they can be referenced in tests and error handling
- Register all configurations via assembly scan — never manually per entity

# Capabilities
- Clean separation between domain entities and EF Core mapping concerns
- Centralized persistence configuration per entity
- Reusable named constants for tables, indexes, and constraints
- Automatic discovery of entity configurations via assembly scan
- Zero EF attributes on domain entities

# Core Principles
- One `IEntityTypeConfiguration<T>` per entity — no exceptions
- Configuration class owns all persistence concerns — entity owns all domain concerns
- `TableName`, index, and constraint names are `public const string` constants on the config class
- Domain entity must have zero EF attributes (`[Column]`, `[Index]`, `[ForeignKey]`, etc.)
- Configuration is the only place that knows about column names, table names, and constraints
- All configurations registered via `ApplyConfigurationsFromAssembly` — never manually
- Cross-module foreign key configurations live in App.Infrastructure — not in Domain config

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create|{Module}.Domain.csproj]] - hosts entity configurations and owns persistence mapping
    - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/{Module}.Domain.csproj.create/{Entity}.cs.create|{Entity}.cs]] - entity pattern extended with EF Core configuration
  - [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/Implementation/App.Infrastructure.csproj.create|App.Infrastructure.csproj]] - hosts cross-module foreign-key configurations

NUGET:
- `Microsoft.EntityFrameworkCore` {version} - provides `IEntityTypeConfiguration<T>`, `EntityTypeBuilder<T>`, `ApplyConfigurationsFromAssembly`

# Template Skill Mutations

PROJECT:
- [[./Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - Add Configurations folder and EF config pattern
	- [[./Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs]] - create - One EF config class per entity
	- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend|{Entity}.cs]] - extend - Zero EF attributes on domain entity
- [[./Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - extend - Add cross-module FK configuration support
	- [[./Implementation/App.Infrastructure.csproj.extend/{Module1}To{Module2}Config.cs.create.md|{Module1}To{Module2}Config.cs]] - create - Cross-module FK configuration class

# Rules

## MUST:
- [[./Implementation/App.Infrastructure.csproj.extend.md#MUST|App.Infrastructure.csproj.extend]]
	- [[./Implementation/App.Infrastructure.csproj.extend/{Module1}To{Module2}Config.cs.create.md#MUST|{Module1}To{Module2}Config.cs.create]]
- [[./Implementation/{Module}.Domain.csproj.extend.md#MUST|{Module}.Domain.csproj.extend]]
	- [[./Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md#MUST|{Entity}Config.cs.create]]
- `TableName` defined as `public const string`
- All index and constraint names defined as `public const string` constants
- `OwnsOne` configured for every multi-property VO property
- All configurations registered via `ApplyConfigurationsFromAssembly`
- Domain entities have zero EF attributes

## MUST NOT:
- [[./Implementation/App.Infrastructure.csproj.extend.md#MUST NOT|App.Infrastructure.csproj.extend]]
	- [[./Implementation/App.Infrastructure.csproj.extend/{Module1}To{Module2}Config.cs.create.md#MUST NOT|{Module1}To{Module2}Config.cs.create]]
- [[./Implementation/{Module}.Domain.csproj.extend.md#MUST NOT|{Module}.Domain.csproj.extend]]
	- [[./Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md#MUST NOT|{Entity}.cs.extend]]
	- [[./Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md#MUST NOT|{Entity}Config.cs.create]]
- Use EF data annotations on domain entities
- Define table, index, or constraint names as inline strings
- Use `static` instead of `const` for `TableName`, index, or constraint names
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
- [ ] `TableName` defined as `public const string`
- [ ] All index names defined as `public const string` constants
- [ ] All unique indexes use `HasDatabaseName(ConstantName)`
- [ ] All intra-module relations configured
- [ ] `OwnsOne` configured for every multi-property VO
- [ ] No EF attributes on any domain entity
- [ ] Configurations registered via `ApplyConfigurationsFromAssembly`
- [ ] Cross-module FK configs live in App.Infrastructure/Persistence/Configurations
- [ ] DbContext uses `ApplyConfigurationsFromAssembly` on all module Domain assemblies
