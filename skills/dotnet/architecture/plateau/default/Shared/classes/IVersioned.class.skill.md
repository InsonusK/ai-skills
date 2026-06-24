---
uid: c24b69f7-107f-46e9-912c-149da36274d5
name: iversioned-class
description: Marker interface for mutable entities that expose a database-generated concurrency version
domain: skill
type: template
version: 20260622
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change-solution.skill]]"
---

# Goal
- Let the concurrency infrastructure recognize mutable entities without using reflection
- Let `EntityVersionResolverFactory` discover versioned entity types automatically by scanning assemblies

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/Shared.csproj.extend/IVersioned.cs.create.md|IVersioned.cs.create]]

# Core Principals
- Single read-only `Version` property
- Implemented by every mutable entity in module Domain projects
- Declared in Shared so Domain can implement it without referencing BuildingBlocks or App.Infrastructure

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/Shared.csproj.extend/IVersioned.cs.create.md|IVersioned.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Versioned entity marker | `IVersioned` | `IVersioned` | `IVersioned.cs` | `IVersioned.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/Shared.csproj.extend/IVersioned.cs.create.md|IVersioned.cs.create]]

# Implementation
```csharp
// Shared/Concurrency/IVersioned.cs
namespace Shared.Concurrency;

public interface IVersioned
{
    uint Version { get; }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/Shared.csproj.extend/IVersioned.cs.create.md|IVersioned.cs.create]]

# Rules
MUST:
	- `Version` is `uint` and read-only at the interface level
	- Implemented by every mutable entity that participates in optimistic concurrency checks
	- Defined in `Shared/Concurrency/IVersioned.cs`
MUST NOT:
	- Add methods or setters to the interface
	- Be implemented by immutable entities or value objects

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/Shared.csproj.extend/IVersioned.cs.create.md|IVersioned.cs.create]]

# Anti-patterns
- Using reflection on `Version` property instead of casting to `IVersioned`
- Implementing `IVersioned` on DTOs or commands — belongs on domain entities only

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/Shared.csproj.extend/IVersioned.cs.create.md|IVersioned.cs.create]]

# Check list
- [ ] `IVersioned` defined in `Shared/Concurrency/IVersioned.cs`
- [ ] Every mutable entity implements `IVersioned`
- [ ] `Version` is read-only on the interface

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/Shared.csproj.extend/IVersioned.cs.create.md|IVersioned.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Let the concurrency infrastructure recognize mutable entities without using reflection
- [ ] WHEN applied THEN Let `EntityVersionResolverFactory` discover versioned entity types automatically by scanning assemblies
- [ ] WHEN applied THEN Version is uint and read-only at the interface level
- [ ] WHEN applied THEN Implemented by every mutable entity in module Domain projects
- [ ] WHEN applied THEN Declared in Shared so Domain can implement it without referencing BuildingBlocks or App.Infrastructure
- [ ] WHEN verified THEN IVersioned defined in Shared/Concurrency/IVersioned.cs
- [ ] WHEN verified THEN Every mutable entity implements IVersioned
- [ ] WHEN naming 'Versioned entity marker' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/entity-concurrency-change-solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change-solution.skill/Implementation/Shared.csproj.extend/IVersioned.cs.create.md|IVersioned.cs.create]]
