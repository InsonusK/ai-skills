---
name: solution-domain-configuration
description: Defines the EF Core entity type configuration pattern — one configuration class per entity that owns all persistence concerns, keeping domain entities free of infrastructure attributes
whenToUse: when creating or changing an EF Core entity type configuration — mapping a table, an index, or a relation
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - stack/dotnet
  - domain
  - ddd
  - ef-core
  - framework/ef-core
  - configuration
  - concern/architecture
  - solution/domain-configuration

creates:
  - "{Module}.Domain.Configurations.{Entity}Config.cs"
  - App.Infrastructure.Persistence.Configurations.{Module1}To{Module2}Config.cs
extends:
  - "{Module}.Domain.csproj"
  - "{Module}.Domain.Entities.Entity.cs"
  - App.Infrastructure.csproj
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]"
adr:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/adr/entity-configuration-lives-in-domain.md|entity-configuration-lives-in-domain]]"
built_on_plateau:
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

# Adr
- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/adr/entity-configuration-lives-in-domain.md|entity-configuration-lives-in-domain]] — the config classes live in `{Module}.Domain/Configurations`, so `{Module}.Domain` takes a `Microsoft.EntityFrameworkCore` reference for the `IEntityTypeConfiguration<T>` abstractions only (no provider, no `DbContext`). This narrows `solution-domain-behaviour`'s "no EF Core in Domain" rule. Rejected: configs in `App.Infrastructure` (splits the entity from its mapping) and a dedicated `{Module}.Infrastructure` project per module (against the 4→2 base-project-set direction).

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj]] - hosts cross-module foreign-key configurations

NUGET:
- `Microsoft.EntityFrameworkCore` {version} - provides `IEntityTypeConfiguration<T>`, `EntityTypeBuilder<T>`, `ApplyConfigurationsFromAssembly`

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - extend - Add Configurations folder and EF config pattern
	- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs]] - create - One EF config class per entity
	- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md|{Entity}.cs]] - extend - Zero EF attributes on domain entity
- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - extend - Add cross-module FK configuration support
	- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend/{Module1}To{Module2}Config.cs.create.md|{Module1}To{Module2}Config.cs]] - create - Cross-module FK configuration class

# Rules

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend.md#MUST|App.Infrastructure.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend/{Module1}To{Module2}Config.cs.create.md#MUST|{Module1}To{Module2}Config.cs]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend.md#MUST|{Module}.Domain.csproj]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md#MUST|{Entity}Config.cs]]
	- [[skills/dotnet/architecture/v3.1/solutions/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}.cs.extend.md#MUST|{Entity}.cs]]

## SHOULD
- Avoid mapping multi-property VO without `OwnsOne` — EF will fail to map or create a shadow table
- Avoid hardcoded index name strings — breaks error handling that matches constraint names
- Avoid using `[ConcurrencyCheck]` attribute on entity instead of fluent config
- Avoid registering configs manually in DbContext — use `ApplyConfigurationsFromAssembly`
- Avoid annotating domain entity with `[Column]`, `[Index]`, `[ForeignKey]` — all mapping belongs in config class
- Avoid single config class shared across multiple entities — one config per entity, no exceptions
- Avoid cross-module FK configured in Domain config — belongs in App.Infrastructure

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
