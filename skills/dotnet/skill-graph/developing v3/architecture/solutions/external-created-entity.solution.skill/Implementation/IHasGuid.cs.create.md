---
description: Marker interface for commands carrying a client-generated Guid
name: IHasGuid.cs
change_kind: create
---

# Goals
- Mark a create command as carrying a client-generated Guid
- Opt the command into `GuidResolvingBehavior` — non-Guid commands are unaffected

# Core Principles
- Single property: `Guid Guid { get; }`
- Implemented by create commands for externally-created entity types only
- Not implemented by update, delete, or internal-create commands
- Lives in Shared so `{Module}.Interfaces` can implement it without referencing BuildingBlocks

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid carrier marker | `IHasGuid` | `IHasGuid` | `IHasGuid.cs` | `IHasGuid.cs` |

# Implementation changes

```csharp
// Shared/Guid/IHasGuid.cs
public interface IHasGuid
{
    Guid Guid { get; }
}
```

# Rules

MUST:
- Only create commands for externally-created entities implement `IHasGuid`

MUST NOT:
- Update, delete, or internally-created entity commands implement `IHasGuid`

# Anti-patterns
- `IHasGuid` on query objects — has no meaning for reads
- `IHasGuid` defined in BuildingBlocks — forces module Interfaces to reference BuildingBlocks

# Check list
- [ ] `IHasGuid` defined in `Shared/Guid/IHasGuid.cs`
- [ ] Only create commands for external-created entities implement it
