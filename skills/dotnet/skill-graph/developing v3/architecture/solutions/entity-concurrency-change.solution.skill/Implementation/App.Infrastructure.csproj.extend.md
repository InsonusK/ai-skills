---
description: Add EntityVersionResolver implementation
name: App.Infrastructure.csproj
element_kind: project
change_kind: extend
---

# Goals
- Own `EntityVersionResolver` — the mapping from stable string entity names to C# entity types
- Discover versioned entities automatically by scanning assemblies supplied by the composition root

# Core Principles
- Static readonly dictionary — populated at startup, no runtime modification
- Keys are stable business names agreed with the frontend — changing a key is a breaking API change
- Entity types are discovered from assemblies supplied by App.Host — typically module Domain assemblies
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
| /Concurrency/EntityVersionResolver.cs | Maps string entity names to C# types for ConcurrencyBehavior by scanning assemblies |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| None required beyond Shared reference | — | Uses reflection on `IVersioned` from Shared |

# Allowed Dependencies
- Shared
- BuildingBlocks
- {Module}.Domain (for entity types referenced in scanned assemblies)

# Rules

MUST:
- `EntityVersionResolver` scans supplied assemblies for `IVersioned` implementations
- Every mutable entity implements `IVersioned` so it is discovered
- Every mutable entity declares a public `VersionedEntityName` constant with its stable business name
- Keys are stable business string names — same strings used in `IHasVersions` commands and ETag encoding
- Constructor accepts `IEnumerable<Assembly>` from the composition root

MUST NOT:
- Keys be C# type names, namespaces, or assembly-qualified names as the public contract — breaks when entities are renamed
- Rely on a hardcoded dictionary without automatic discovery

# Anti-patterns
- `EntityVersionResolver` key using `nameof(TodoTask)` — fragile, breaks on class rename
- Hardcoded dictionary of entity types — duplicates the entity list and is easy to forget when adding new entities
- Scanning `AppDomain.CurrentDomain.GetAssemblies()` without an explicit allow-list — includes unrelated assemblies

# Check list
- [ ] `EntityVersionResolver` defined in `App.Infrastructure/Concurrency/EntityVersionResolver.cs`
- [ ] Constructor accepts `IEnumerable<Assembly>`
- [ ] Scans supplied assemblies for `IVersioned` implementations
- [ ] Every mutable entity implements `IVersioned`
- [ ] Keys are stable business strings

# Unittest TestCases
- [ ] WHEN applied THEN Own EntityVersionResolver — the mapping from stable string entity names to C# entity types
- [ ] WHEN applied THEN Discover versioned entities automatically by scanning assemblies supplied by the composition root
- [ ] WHEN verified THEN EntityVersionResolver defined in App.Infrastructure/Concurrency/EntityVersionResolver.cs
- [ ] WHEN verified THEN Constructor accepts IEnumerable<Assembly>
- [ ] WHEN verified THEN Scans supplied assemblies for IVersioned implementations
