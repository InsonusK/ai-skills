---
description: Marker interface for commands carrying a client-generated Guid
project_name: Shared
name: IHasGuid.cs
element_kind: class
change_kind: create
tags:
  - solution/external-created-entity
  - element/ihasguid-cs
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
# Rule changes

## MUST
- Only create commands for externally-created entities implement `IHasGuid`
- Create commands for external-created entities implement both `ICommand<Result<Create{Entity}Result>>` and `IHasGuid`
- Never update, delete, or internally-created entity commands implement `IHasGuid`

## SHOULD
- Avoid `IHasGuid` on query objects — has no meaning for reads
- Avoid `IHasGuid` defined in BuildingBlocks — forces module Interfaces to reference BuildingBlocks

# Check list
- [ ] `IHasGuid` defined in `Shared/Guid/IHasGuid.cs`
- [ ] Only create commands for external-created entities implement it

# Unittest TestCases
- [ ] WHEN applied THEN Mark a create command as carrying a client-generated Guid
- [ ] WHEN applied THEN Opt the command into GuidResolvingBehavior — non-Guid commands are unaffected
- [ ] WHEN applied THEN Single property: Guid Guid { get; }
- [ ] WHEN applied THEN Implemented by create commands for externally-created entity types only
- [ ] WHEN applied THEN Not implemented by update, delete, or internal-create commands
- [ ] WHEN applied THEN Lives in Shared so {Module}.Interfaces can implement it without referencing BuildingBlocks
- [ ] WHEN verified THEN IHasGuid defined in Shared/Guid/IHasGuid.cs
- [ ] WHEN verified THEN Only create commands for external-created entities implement it
- [ ] WHEN naming 'Guid carrier marker' THEN pattern matches convention
