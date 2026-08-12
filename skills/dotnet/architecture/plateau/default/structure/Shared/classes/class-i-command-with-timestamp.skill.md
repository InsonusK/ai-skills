---
name: class-i-command-with-timestamp
description: Command marker that carries the user-supplied action time
domain: skill
type: template
version: 20260630010447
plateau: default
tags:
  - skill/template/class
  - plateau/default
  - stack/dotnet
  - concern/architecture

created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]"
---

# Goal
- Mark a command as carrying the user timestamp that should be recorded for the affected entity.
- Enable validators and handlers to work with `ActionTimeStamp` consistently.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICommandWithTimestamp.cs.create|ICommandWithTimestamp.cs]]

# Core Principles
- Apply ONE plateau template per class
- Single property: `DateTimeOffset ActionTimeStamp { get; }`.
- Implemented by create and update commands for timestamped entity types.
- Not implemented by delete commands or commands targeting `Internal Immutable` entities.
- Lives in `Shared` so `{Module}.Interfaces` can implement it without referencing BuildingBlocks.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICommandWithTimestamp.cs.create|ICommandWithTimestamp.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Timestamp command marker | `ICommandWithTimestamp` | `ICommandWithTimestamp` | `ICommandWithTimestamp.cs` | `ICommandWithTimestamp.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICommandWithTimestamp.cs.create|ICommandWithTimestamp.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-i-command-with-timestamp
//Plateau: default
//Version: 20260630010447
```

```csharp
// Shared/Timestamps/ICommandWithTimestamp.cs
namespace Shared.Timestamps;

public interface ICommandWithTimestamp
{
    DateTimeOffset ActionTimeStamp { get; }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICommandWithTimestamp.cs.create|ICommandWithTimestamp.cs]]

# Rules
MUST:
	- Only commands that create or update a timestamped entity implement this interface.
	- `ActionTimeStamp` is typed as `DateTimeOffset`.
	- Defined in `Shared/Timestamps/ICommandWithTimestamp.cs`.
MUST NOT:
	- Add methods or other properties.
	- Be implemented by delete commands or query objects.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICommandWithTimestamp.cs.create|ICommandWithTimestamp.cs]]

# Anti-patterns
- `ICommandWithTimestamp` on query objects — has no meaning for reads.
- `ICommandWithTimestamp` defined in BuildingBlocks — forces module Interfaces to reference BuildingBlocks.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICommandWithTimestamp.cs.create|ICommandWithTimestamp.cs]]

# Check list
- [ ] `ICommandWithTimestamp` defined in `Shared/Timestamps/ICommandWithTimestamp.cs`.
- [ ] Only create and update commands for timestamped entities implement it.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICommandWithTimestamp.cs.create|ICommandWithTimestamp.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN expose `ActionTimeStamp` as `DateTimeOffset`.
- [ ] WHEN applied THEN be implemented by create/update commands for timestamped entities only.
- [ ] WHEN applied THEN live in `Shared.Timestamps`.
- [ ] WHEN naming THEN pattern matches convention.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICommandWithTimestamp.cs.create|ICommandWithTimestamp.cs]]
