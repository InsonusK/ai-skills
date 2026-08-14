---
description: Command marker that carries the user-supplied action time
project_name: Shared
name: ICommandWithTimestamp.cs
element_kind: class
change_kind: create
---

# Goals
- Mark a command as carrying the user timestamp that should be recorded for the affected entity.
- Enable validators and handlers to work with `ActionTimeStamp` consistently.

# Core Principles
- Single property: `DateTimeOffset ActionTimeStamp { get; }`.
- Implemented by create and update commands for timestamped entity types.
- Not implemented by delete commands or commands targeting `Internal Immutable` entities.
- Lives in `Shared` so `{Module}.Interfaces` can implement it without referencing BuildingBlocks.

# Structure

## Project Structure
```
/Shared
  /Timestamps
    ICommandWithTimestamp.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Timestamp command marker | `ICommandWithTimestamp` | `ICommandWithTimestamp` | `ICommandWithTimestamp.cs` | `ICommandWithTimestamp.cs` |

# Implementation changes

```csharp
// Shared/Timestamps/ICommandWithTimestamp.cs
namespace Shared.Timestamps;

public interface ICommandWithTimestamp
{
    DateTimeOffset ActionTimeStamp { get; }
}
```
# Rule changes

## MUST
- `ActionTimeStamp` is typed as `DateTimeOffset`.
- `ICreationInfoModelReadOnly`, `ICreationInfoModel`, `IUpdateInfoModelReadOnly`, `IUpdateInfoModel`, and `ICommandWithTimestamp` are defined in `Shared/Timestamps`.
- Commands that create or update a timestamped entity implement `ICommandWithTimestamp` alongside `ICommand<Result<T>>`.
- `ActionTimeStamp` is the first property on commands that implement `ICommandWithTimestamp`.

## MUST NOT
- Add methods or other properties.
- Be implemented by delete commands or query objects.

# Anti-patterns
- `ICommandWithTimestamp` on query objects — has no meaning for reads.
- `ICommandWithTimestamp` defined in BuildingBlocks — forces module Interfaces to reference BuildingBlocks.

# Check list
- [ ] `ICommandWithTimestamp` defined in `Shared/Timestamps/ICommandWithTimestamp.cs`.
- [ ] Only create and update commands for timestamped entities implement it.

# Unittest TestCases
- [ ] WHEN applied THEN expose `ActionTimeStamp` as `DateTimeOffset`.
- [ ] WHEN applied THEN be implemented by create/update commands for timestamped entities only.
- [ ] WHEN applied THEN live in `Shared.Timestamps`.
- [ ] WHEN naming THEN pattern matches convention.
