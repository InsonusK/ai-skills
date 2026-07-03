---
name: class-i-has-guid
description: Marker interface for commands carrying a client-generated Guid
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]]"
---

# Goal
- Mark a create command as carrying a client-generated Guid
- Opt the command into `GuidResolvingBehavior` — non-Guid commands are unaffected

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IHasGuid.cs.create|IHasGuid.cs]]

# Core Principles
- Apply ONE plateau template per class
- Single property: `Guid Guid { get; }`
- Implemented by create commands for externally-created entity types only
- Not implemented by update, delete, or internal-create commands
- Lives in Shared so `{Module}.Interfaces` can implement it without referencing BuildingBlocks

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IHasGuid.cs.create|IHasGuid.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Guid carrier marker | `IHasGuid` | `IHasGuid` | `IHasGuid.cs` | `IHasGuid.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IHasGuid.cs.create|IHasGuid.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-i-has-guid
//Plateau: default
//Version: 20260628
```

```csharp
// Shared/Guid/IHasGuid.cs
public interface IHasGuid
{
    Guid Guid { get; }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IHasGuid.cs.create|IHasGuid.cs]]

# Rules
MUST:
	- Only create commands for externally-created entities implement `IHasGuid`
MUST NOT:
	- Update, delete, or internally-created entity commands implement `IHasGuid`

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IHasGuid.cs.create|IHasGuid.cs]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- `IHasGuid` on query objects — has no meaning for reads
- `IHasGuid` defined in BuildingBlocks — forces module Interfaces to reference BuildingBlocks

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IHasGuid.cs.create|IHasGuid.cs]]

# Check list
- [ ] `IHasGuid` defined in `Shared/Guid/IHasGuid.cs`
- [ ] Only create commands for external-created entities implement it

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IHasGuid.cs.create|IHasGuid.cs]]

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

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-external-created-entity.skill/Implementation/Shared.csproj.extend/IHasGuid.cs.create|IHasGuid.cs]]
