---
description: Add EntityVersionResolver implementation
name: App.Infrastructure.csproj
element_kind: project
change_kind: extend
---

# Goals
- Own `EntityVersionResolver` — the mapping from stable string entity names to C# entity types
- Discover versioned entities automatically by scanning config classes in supplied assemblies

# Core Principles
- Static readonly dictionary — populated at startup, no runtime modification
- Keys are stable business names declared in `{Entity}Config.VersionedEntityName` — changing a key is a breaking API change
- Entity types are discovered from `IEntityTypeConfiguration<T>` config classes in assemblies supplied during registration — typically module Domain assemblies
- Returns `null` for unknown names — `ConcurrencyBehavior` returns `Result.Error` on null

# Structure

## Project Structure
```
/App.Infrastructure
  /Concurrency
    EntityVersionResolver.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Concurrency/EntityVersionResolver.cs | Maps string entity names to C# types for ConcurrencyBehavior by scanning config classes |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides `IEntityTypeConfiguration<>` used to discover config classes |

# Allowed Dependencies
- Shared
- BuildingBlocks
- {Module}.Domain (for config classes)

# Rules

MUST:
- `EntityVersionResolver` scans supplied assemblies for `IEntityTypeConfiguration<T>` configs where `T` implements `IVersioned`
- Every mutable entity implements `IVersioned`
- Every mutable entity config class declares `public const string VersionedEntityName`
- Keys are stable business string names — same strings used in `IHasVersions` commands and ETag encoding
- Constructor accepts `IEnumerable<Assembly>` from the composition root

MUST NOT:
- Keys be C# type names, namespaces, or assembly-qualified names as the public contract — breaks when entities are renamed
- Rely on a hardcoded dictionary of entity types

# Anti-patterns
- `EntityVersionResolver` key using `nameof(TodoTask)` — fragile, breaks on class rename
- Hardcoded dictionary of entity types — duplicates the entity list and is easy to forget when adding new entities
- Scanning `AppDomain.CurrentDomain.GetAssemblies()` without an explicit allow-list — includes unrelated assemblies
- Putting `VersionedEntityName` on the entity class instead of the config — spreads configuration across the domain

# Check list
- [ ] `EntityVersionResolver` defined in `App.Infrastructure/Concurrency/EntityVersionResolver.cs`
- [ ] Constructor accepts `IEnumerable<Assembly>`
- [ ] Scans supplied assemblies for `IEntityTypeConfiguration<T>` configs where `T` implements `IVersioned`
- [ ] Every mutable entity implements `IVersioned`
- [ ] Every mutable entity config class declares `VersionedEntityName`
- [ ] Keys are stable business strings

# Unittest TestCases
- [ ] WHEN applied THEN Own EntityVersionResolver — the mapping from stable string entity names to C# entity types
- [ ] WHEN applied THEN Discover versioned entities automatically by scanning config classes in supplied assemblies
- [ ] WHEN verified THEN EntityVersionResolver defined in App.Infrastructure/Concurrency/EntityVersionResolver.cs
- [ ] WHEN verified THEN Constructor accepts IEnumerable<Assembly>
- [ ] WHEN verified THEN Scans supplied assemblies for IEntityTypeConfiguration<T> configs where T implements IVersioned
