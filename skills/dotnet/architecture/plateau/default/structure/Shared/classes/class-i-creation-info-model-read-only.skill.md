---
name: class-i-creation-info-model-read-only
description: Read-only creation timestamp contract
domain: skill
type: template
version: 20260630010447
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp.skill]]"
---

# Goal
- Expose creation timestamps for read models, DTOs, and other consumers that must not modify them.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModelReadOnly.cs.create.md|ICreationInfoModelReadOnly.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- Read-only contract — no setters.
- Implemented by read models and by `ICreationInfoModel` through inheritance.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModelReadOnly.cs.create.md|ICreationInfoModelReadOnly.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Read-only creation timestamps | `ICreationInfoModelReadOnly` | `ICreationInfoModelReadOnly` | `ICreationInfoModelReadOnly.cs` | `ICreationInfoModelReadOnly.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModelReadOnly.cs.create.md|ICreationInfoModelReadOnly.cs.create]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-i-creation-info-model-read-only
//Plateau: default
//Version: 20260630010447
```

```csharp
// Shared/Timestamps/ICreationInfoModelReadOnly.cs
namespace Shared.Timestamps;

public interface ICreationInfoModelReadOnly
{
    DateTimeOffset ServerCreatedDateTime { get; }
    DateTimeOffset UserCreatedDateTime { get; }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModelReadOnly.cs.create.md|ICreationInfoModelReadOnly.cs.create]]

# Rules
MUST:
	- Declare only getters.
	- Use `DateTimeOffset` for both properties.
MUST NOT:
	- Add setters or methods.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModelReadOnly.cs.create.md|ICreationInfoModelReadOnly.cs.create]]

# Anti-patterns
- Using this interface where mutation is required — use `ICreationInfoModel` instead.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModelReadOnly.cs.create.md|ICreationInfoModelReadOnly.cs.create]]

# Check list
- [ ] Interface defined in `Shared/Timestamps/ICreationInfoModelReadOnly.cs`.
- [ ] Both properties are `DateTimeOffset` with getters only.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModelReadOnly.cs.create.md|ICreationInfoModelReadOnly.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN expose `ServerCreatedDateTime` as read-only.
- [ ] WHEN applied THEN expose `UserCreatedDateTime` as read-only.
- [ ] WHEN naming THEN pattern matches convention.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModelReadOnly.cs.create.md|ICreationInfoModelReadOnly.cs.create]]
