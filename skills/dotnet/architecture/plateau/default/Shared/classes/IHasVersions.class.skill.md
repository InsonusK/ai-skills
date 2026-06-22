---
uid: d347a5b6-d388-49a4-99ed-69c60ba5605e
name: ihasversions-class
description: Interface for update commands carrying client-supplied version information
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change.solution.skill]]"
---

# Goal
- Provide a typed contract for update commands to carry client-supplied version information
- Enable `ConcurrencyBehavior` to activate selectively on commands that carry versions — not all commands

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create.md|IHasVersions.cs.create]]

# Core Principals
- Dictionary structure: entity name string → (entity Id → expected version)
- Supports multi-entity updates — a single command can carry versions for multiple entities
- Entity name keys are stable business strings — `"Task"`, `"TimeLog"` — never C# type names
- Declared in Shared — update commands in `{Module}.Interfaces` implement this without referencing BuildingBlocks

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create.md|IHasVersions.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Version carrier interface | `IHasVersions` | `IHasVersions` | `IHasVersions.cs` | `IHasVersions.cs` |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create.md|IHasVersions.cs.create]]

# Implementation
```csharp
// Shared/Concurrency/IHasVersions.cs
namespace Shared.Concurrency;

public interface IHasVersions
{
    // entity name → (entity id → expected row version)
    // e.g. {"Task": {"2": 3}, "TimeLog": {"1": 19}}
    IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions { get; }
}
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create.md|IHasVersions.cs.create]]

# Rules
MUST:
	- Keys are stable business string names — never C# type names or namespace-qualified names
	- Used only on update and patch commands — never on create or delete commands
MUST NOT:
	- Use C# `Type` as the dictionary key — breaks when entities are renamed

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create.md|IHasVersions.cs.create]]

# Anti-patterns
- `Versions` as a flat dictionary without entity name grouping — does not support multi-entity updates

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create.md|IHasVersions.cs.create]]

# Check list
- [ ] `IHasVersions` defined in `Shared/Concurrency/IHasVersions.cs`
- [ ] Dictionary keys are stable business strings
- [ ] Only update and patch commands implement this interface

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create.md|IHasVersions.cs.create]]

# Unittest TestCases
- [ ] WHEN component is requested THEN it provide a typed contract for update commands to carry client-supplied version information
- [ ] WHEN applied THEN Enable ConcurrencyBehavior to activate selectively on commands that carry versions — not all commands
- [ ] WHEN applied THEN Dictionary structure: entity name string → (entity Id → expected version)
- [ ] WHEN applied THEN Supports multi-entity updates — a single command can carry versions for multiple entities
- [ ] WHEN applied THEN Entity name keys are stable business strings — "Task", "TimeLog" — never C# type names
- [ ] WHEN applied THEN Declared in Shared — update commands in {Module}.Interfaces implement this without referencing BuildingBlocks
- [ ] WHEN verified THEN IHasVersions defined in Shared/Concurrency/IHasVersions.cs
- [ ] WHEN verified THEN Dictionary keys are stable business strings
- [ ] WHEN verified THEN Only update and patch commands implement this interface
- [ ] WHEN naming 'Version carrier interface' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create.md|IHasVersions.cs.create]]
