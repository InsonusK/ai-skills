---
name: class-i-creation-info-model
description: Mutable creation timestamp contract implemented by entities
domain: skill
type: template
version: 20260630010447
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]"
---

# Goal
- Allow handlers and `AppDbContext` to assign creation timestamps while keeping the entity's own setters internal.

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModel.cs.create|ICreationInfoModel.cs]]

# Core Principles
- Apply ONE plateau template per class
- Inherits the read-only contract so entities automatically satisfy `ICreationInfoModelReadOnly`.
- Re-declares properties with setters to form a mutable contract.
- Entities implement this interface explicitly for the setter so the class-level property setter can remain `internal`.

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModel.cs.create|ICreationInfoModel.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Mutable creation timestamps | `ICreationInfoModel` | `ICreationInfoModel` | `ICreationInfoModel.cs` | `ICreationInfoModel.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModel.cs.create|ICreationInfoModel.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-i-creation-info-model
//Plateau: default
//Version: 20260630010447
```

```csharp
// Shared/Timestamps/ICreationInfoModel.cs
namespace Shared.Timestamps;

public interface ICreationInfoModel : ICreationInfoModelReadOnly
{
    new DateTimeOffset ServerCreatedDateTime { get; set; }
    new DateTimeOffset UserCreatedDateTime { get; set; }
}
```

Entities implement the setter explicitly to keep class-level setters internal:

```csharp
// {Module}.Domain/Entities/{EntityName}.cs
public class {EntityName} : ICreationInfoModel
{
    public int Id { get; internal set; }

    public DateTimeOffset ServerCreatedDateTime { get; internal set; }
    public DateTimeOffset UserCreatedDateTime { get; internal set; }

    DateTimeOffset ICreationInfoModel.ServerCreatedDateTime
    {
        get => ServerCreatedDateTime;
        set => ServerCreatedDateTime = value;
    }

    DateTimeOffset ICreationInfoModel.UserCreatedDateTime
    {
        get => UserCreatedDateTime;
        set => UserCreatedDateTime = value;
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModel.cs.create|ICreationInfoModel.cs]]

# Rules
MUST:
	- Inherit `ICreationInfoModelReadOnly`.
	- Re-declare both properties with `get; set;`.
	- Be implemented by `External Immutable`, `Internal Mutable`, and `External Mutable` entities.
MUST NOT:
	- Be implemented by `Internal Immutable` entities.
	- Add behavior methods.

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModel.cs.create|ICreationInfoModel.cs]]

# Anti-patterns
- Implementing this interface implicitly with public setters — breaks the "no public setters on entities" rule.
- Forgetting to implement the setter explicitly when the class-level setter is `internal`.

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModel.cs.create|ICreationInfoModel.cs]]

# Check list
- [ ] Interface defined in `Shared/Timestamps/ICreationInfoModel.cs`.
- [ ] Inherits `ICreationInfoModelReadOnly`.
- [ ] Both properties have `get` and `set`.
- [ ] Entity class-level timestamp properties have `internal set`.
- [ ] Entity implements the interface setter explicitly.

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModel.cs.create|ICreationInfoModel.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN allow assignment through `ICreationInfoModel`.
- [ ] WHEN applied THEN keep entity class-level setter internal.
- [ ] WHEN applied THEN satisfy `ICreationInfoModelReadOnly` through inheritance.
- [ ] WHEN naming THEN pattern matches convention.

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/ICreationInfoModel.cs.create|ICreationInfoModel.cs]]
