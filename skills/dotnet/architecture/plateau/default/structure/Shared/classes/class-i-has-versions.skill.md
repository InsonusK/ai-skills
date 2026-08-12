---
name: class-i-has-versions
description: Interface for update commands carrying client-supplied version information
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
  - stack/dotnet
  - concern/architecture

created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]]"
---

# Goal
- Provide a typed contract for update commands to carry client-supplied version information
- Enable `ConcurrencyBehavior` to activate selectively on commands that carry versions — not all commands

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create|IHasVersions.cs]]

# Core Principles
- Apply ONE plateau template per class
- Dictionary structure: entity name string → (entity Id → expected version)
- Supports multi-entity updates — a single command can carry versions for multiple entities
- Entity name keys are stable business strings — `"Task"`, `"TimeLog"` — never C# type names
- Declared in Shared — update commands in `{Module}.Interfaces` implement this without referencing BuildingBlocks

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create|IHasVersions.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Version carrier interface | `IHasVersions` | `IHasVersions` | `IHasVersions.cs` | `IHasVersions.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create|IHasVersions.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-i-has-versions
//Plateau: default
//Version: 20260628
```

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
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create|IHasVersions.cs]]

# Rules
MUST:
	- Keys are stable business string names — never C# type names or namespace-qualified names
	- Used only on update and patch commands — never on create or delete commands
MUST NOT:
	- Use C# `Type` as the dictionary key — breaks when entities are renamed

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create|IHasVersions.cs]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- `Versions` as a flat dictionary without entity name grouping — does not support multi-entity updates

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create|IHasVersions.cs]]

# Check list
- [ ] `IHasVersions` defined in `Shared/Concurrency/IHasVersions.cs`
- [ ] Dictionary keys are stable business strings
- [ ] Only update and patch commands implement this interface

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create|IHasVersions.cs]]

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
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill|solution-entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-concurrency-change.skill/Implementation/Shared.csproj.extend/IHasVersions.cs.create|IHasVersions.cs]]
