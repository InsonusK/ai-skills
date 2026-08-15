---
description: Add EntityVersionResolverFactory factory implementation
name: App.Infrastructure.csproj
element_kind: project
change_kind: extend
tags:
  - solution/entity-concurrency-change
  - element/app-infrastructure-csproj
---

# Goals
- Own `EntityVersionResolverFactory` — the factory that maps stable string entity names to Application-layer `IEntityVersionResolver` implementations
- Discover versioned entities from Domain config classes and resolver implementations from Application assemblies

# Core Principles
- Read-only map — populated once (static/lazy) at first use, no runtime modification
- Keys are stable business names declared in `{Entity}Config.VersionedEntityName` and `{Entity}VersionResolver.VersionedEntityName`
- Domain assemblies supply the list of valid versioned entities
- Application assemblies supply the resolver implementations
- Returns `null` for unknown names — `ConcurrencyBehavior` returns `Result.Error` on null

# Structure

## Project Structure
```
/App.Infrastructure
  /Concurrency
    EntityVersionResolverFactory.cs
```

## Directory and class skills
| Directory \ file | Description |
| ----------------- | ----------- |
| /Concurrency/EntityVersionResolverFactory.cs | Maps string entity names to Application-layer IEntityVersionResolver implementations |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides `IEntityTypeConfiguration<>` used to discover config classes |

# Allowed Dependencies
- Shared
- {Module}.Domain (for config classes)

# Rules

## MUST
- `EntityVersionResolverFactory` scans Domain assemblies for `IEntityTypeConfiguration<T>` configs where `T` implements `IVersioned`
- `EntityVersionResolverFactory` scans Application assemblies for concrete `IEntityVersionResolver` implementations
- Every mutable entity implements `IVersioned`
- Every mutable entity config class declares `public const string VersionedEntityName`
- Every `{Entity}VersionResolver` declares `public const string VersionedEntityName` matching its config
- Keys are stable business string names — same strings used in `IHasVersions` commands and ETag encoding
- Constructor accepts `IServiceProvider`, Domain assemblies, and Application assemblies
- Build the resolver-type map only once (static, lazy, thread-safe)

## MUST NOT
- Keys be C# type names, namespaces, or assembly-qualified names as the public contract — breaks when entities are renamed
- Rely on a hardcoded dictionary of resolver types
- `IEntityVersionResolver` implementations live in App.Infrastructure or BuildingBlocks

# Anti-patterns
- `EntityVersionResolverFactory` key using `nameof(TodoTask)` — fragile, breaks on class rename
- Hardcoded dictionary of resolver types — duplicates the entity list and is easy to forget when adding new entities
- Scanning `AppDomain.CurrentDomain.GetAssemblies()` without an explicit allow-list — includes unrelated assemblies
- Putting `VersionedEntityName` on the entity class instead of the config — spreads configuration across the domain

# Check list
- [ ] `EntityVersionResolverFactory` defined in `App.Infrastructure/Concurrency/EntityVersionResolverFactory.cs`
- [ ] Constructor accepts `IServiceProvider`, Domain assemblies, and Application assemblies
- [ ] Scans Domain assemblies for `IEntityTypeConfiguration<T>` configs where `T` implements `IVersioned`
- [ ] Scans Application assemblies for `IEntityVersionResolver` implementations
- [ ] Every mutable entity config class declares `VersionedEntityName`
- [ ] Every `{Entity}VersionResolver` declares matching `VersionedEntityName`
- [ ] Keys are stable business strings
- [ ] Resolver-type map is built only once and thread-safe

# Unittest TestCases
- [ ] WHEN applied THEN Own EntityVersionResolverFactory — the factory that maps stable entity names to Application-layer IEntityVersionResolver implementations
- [ ] WHEN applied THEN Discover versioned entities from Domain config classes and resolver implementations from Application assemblies
- [ ] WHEN verified THEN EntityVersionResolverFactory defined in App.Infrastructure/Concurrency/EntityVersionResolverFactory.cs
- [ ] WHEN verified THEN Constructor accepts IServiceProvider, domainAssemblies, and applicationAssemblies
- [ ] WHEN verified THEN Scans Domain assemblies for IEntityTypeConfiguration<T> configs where T implements IVersioned
- [ ] WHEN verified THEN Scans Application assemblies for IEntityVersionResolver implementations
