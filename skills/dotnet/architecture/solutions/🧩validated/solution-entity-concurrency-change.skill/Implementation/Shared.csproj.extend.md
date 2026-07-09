---
description: Add IHasVersions, IEntityVersionResolverFactory, IEntityVersionResolver, and IVersioned concurrency contracts to Shared
name: Shared.csproj
element_kind: project
change_kind: extend
---

# Goals
- Own the common concurrency contracts that every layer can safely depend on without coupling to BuildingBlocks
- Provide `IVersioned` so domain entities can declare themselves as versioned and be discovered by infrastructure
- Provide `IEntityVersionResolverFactory` and `IEntityVersionResolver` so the pipeline behavior can check versions without knowing entity types

# Core Principles
- Shared defines only interfaces and markers — no implementations
- `IHasVersions` and `IEntityVersionResolverFactory` are cross-cutting contracts referenced by both Application and Api layers
- `IVersioned` is implemented by mutable entities in module Domain projects
- `IEntityVersionResolver` is implemented by module Application projects

# Structure

## Project Structure
```
/Shared
  /Concurrency
    IHasVersions.cs
    IEntityVersionResolverFactory.cs
    IEntityVersionResolver.cs
    IVersioned.cs
```

## Directory and class skills
| Directory \ file | Description |
| ----------------- | ----------- |
| /Concurrency/IHasVersions.cs | Interface carried by all update commands |
| /Concurrency/IEntityVersionResolverFactory.cs | Factory that resolves an entity name to an `IEntityVersionResolver` |
| /Concurrency/IEntityVersionResolver.cs | Reads the current version for one versioned entity |
| /Concurrency/IVersioned.cs | Marker interface for versioned domain entities |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| None | — | Interfaces only — no external dependencies |

# Allowed Dependencies
- None — Shared has no project dependencies

# Rules

## MUST
- `IHasVersions`, `IEntityVersionResolverFactory`, `IEntityVersionResolver`, and `IVersioned` defined in Shared
- All four types are interfaces or markers only — no implementation code

## MUST NOT
- Add MediatR, EF Core, or JSON serialization dependencies to Shared for these contracts
- Place implementations in Shared

# Anti-patterns
- Defining `IHasVersions`, `IEntityVersionResolverFactory`, or `IEntityVersionResolver` in BuildingBlocks — forces modules to reference BuildingBlocks for contracts

# Check list
- [ ] `IHasVersions` defined in `Shared/Concurrency/IHasVersions.cs`
- [ ] `IEntityVersionResolverFactory` defined in `Shared/Concurrency/IEntityVersionResolverFactory.cs`
- [ ] `IEntityVersionResolver` defined in `Shared/Concurrency/IEntityVersionResolver.cs`
- [ ] `IVersioned` defined in `Shared/Concurrency/IVersioned.cs`
- [ ] Shared.csproj has no project references and no new NuGet packages for these contracts

# Unittest TestCases
- [ ] WHEN applied THEN Own the common concurrency contracts that every layer can safely depend on without coupling to BuildingBlocks
- [ ] WHEN applied THEN Provide IVersioned so domain entities can declare themselves as versioned and be discovered by infrastructure
- [ ] WHEN verified THEN IHasVersions defined in Shared/Concurrency/IHasVersions.cs
- [ ] WHEN verified THEN IEntityVersionResolverFactory defined in Shared/Concurrency/IEntityVersionResolverFactory.cs
- [ ] WHEN verified THEN IEntityVersionResolver defined in Shared/Concurrency/IEntityVersionResolver.cs
- [ ] WHEN verified THEN IVersioned defined in Shared/Concurrency/IVersioned.cs
