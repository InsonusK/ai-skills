---
description: Add EntityVersionResolver implementation
name: App.Infrastructure.csproj
change_kind: extend
---

# Goals
- Own `EntityVersionResolver` — the mapping from stable string entity names to C# entity types

# Core Principles
- Static readonly dictionary — populated at startup, no runtime modification
- Keys are stable business names agreed with the frontend — changing a key is a breaking API change
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
| /Concurrency/EntityVersionResolver.cs | Maps string entity names to C# types for ConcurrencyBehavior |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides entity types referenced in the resolver map |

# Allowed Dependencies
- Shared
- BuildingBlocks
- {Module}.Domain (for entity types)

# Rules

MUST:
- Every mutable entity type registered in `EntityVersionResolver`
- Keys are stable business string names — same strings used in `IHasVersions` commands and ETag encoding

MUST NOT:
- Keys be C# type names, namespaces, or assembly-qualified names — breaks when entities are renamed

# Anti-patterns
- `EntityVersionResolver` key using `nameof(TodoTask)` — fragile, breaks on class rename

# Check list
- [ ] `EntityVersionResolver` defined in `App.Infrastructure/Concurrency/EntityVersionResolver.cs`
- [ ] Every mutable entity registered in the resolver map
- [ ] Keys are stable business strings
