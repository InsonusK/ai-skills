---
name: class-i-update-info-model-read-only
description: Read-only update timestamp contract
domain: skill
type: template
version: 20260630010447
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]"
---

# Goal
- Expose update timestamps for read models, DTOs, and other consumers that must not modify them.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModelReadOnly.cs.create|IUpdateInfoModelReadOnly.cs]]

# Core Principles
- Apply ONE plateau template per class
- Read-only contract — no setters.
- Implemented by read models and by `IUpdateInfoModel` through inheritance.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModelReadOnly.cs.create|IUpdateInfoModelReadOnly.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Read-only update timestamps | `IUpdateInfoModelReadOnly` | `IUpdateInfoModelReadOnly` | `IUpdateInfoModelReadOnly.cs` | `IUpdateInfoModelReadOnly.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModelReadOnly.cs.create|IUpdateInfoModelReadOnly.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-i-update-info-model-read-only
//Plateau: default
//Version: 20260630010447
```

```csharp
// Shared/Timestamps/IUpdateInfoModelReadOnly.cs
namespace Shared.Timestamps;

public interface IUpdateInfoModelReadOnly
{
    DateTimeOffset ServerUpdatedDateTime { get; }
    DateTimeOffset UserUpdatedDateTime { get; }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModelReadOnly.cs.create|IUpdateInfoModelReadOnly.cs]]

# Rules
MUST:
	- Declare only getters.
	- Use `DateTimeOffset` for both properties.
MUST NOT:
	- Add setters or methods.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModelReadOnly.cs.create|IUpdateInfoModelReadOnly.cs]]

# Anti-patterns
- Using this interface where mutation is required — use `IUpdateInfoModel` instead.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModelReadOnly.cs.create|IUpdateInfoModelReadOnly.cs]]

# Check list
- [ ] Interface defined in `Shared/Timestamps/IUpdateInfoModelReadOnly.cs`.
- [ ] Both properties are `DateTimeOffset` with getters only.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModelReadOnly.cs.create|IUpdateInfoModelReadOnly.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN expose `ServerUpdatedDateTime` as read-only.
- [ ] WHEN applied THEN expose `UserUpdatedDateTime` as read-only.
- [ ] WHEN naming THEN pattern matches convention.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModelReadOnly.cs.create|IUpdateInfoModelReadOnly.cs]]
