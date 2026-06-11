---
description: Interface for update commands carrying client-supplied version information
name: IHasVersions.cs
change_kind: create
---

# Goals
- Provide a typed contract for update commands to carry client-supplied version information
- Enable `ConcurrencyBehavior` to activate selectively on commands that carry versions — not all commands

# Core Principles
- Dictionary structure: entity name string → (entity Id → expected version)
- Supports multi-entity updates — a single command can carry versions for multiple entities
- Entity name keys are stable business strings — `"Task"`, `"TimeLog"` — never C# type names
- Declared in BuildingBlocks — update commands in `{Module}.Interfaces` implement this

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Version carrier interface | `IHasVersions` | `IHasVersions` | `IHasVersions.cs` | `IHasVersions.cs` |

# Implementation changes

```csharp
// BuildingBlocks/Concurrency/IHasVersions.cs
public interface IHasVersions
{
    // entity name → (entity id → expected row version)
    // e.g. {"Task": {"2": 3}, "TimeLog": {"1": 19}}
    IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions { get; }
}
```

# Rules

MUST:
- Keys are stable business string names — never C# type names or namespace-qualified names
- Used only on update and patch commands — never on create or delete commands

MUST NOT:
- Use C# `Type` as the dictionary key — breaks when entities are renamed

# Anti-patterns
- `Versions` as a flat dictionary without entity name grouping — does not support multi-entity updates

# Check list
- [ ] `IHasVersions` defined in `BuildingBlocks/Concurrency/IHasVersions.cs`
- [ ] Dictionary keys are stable business strings
- [ ] Only update and patch commands implement this interface
