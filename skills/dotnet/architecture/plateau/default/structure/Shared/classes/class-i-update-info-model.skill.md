---
name: class-i-update-info-model
description: Mutable update timestamp contract implemented by entities
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
- Allow handlers and `AppDbContext` to assign update timestamps while keeping the entity's own setters internal.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModel.cs.create|IUpdateInfoModel.cs]]

# Core Principles
- Apply ONE plateau template per class
- Inherits the read-only contract so entities automatically satisfy `IUpdateInfoModelReadOnly`.
- Re-declares properties with setters to form a mutable contract.
- Entities implement this interface explicitly for the setter so the class-level property setter can remain `internal`.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModel.cs.create|IUpdateInfoModel.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Mutable update timestamps | `IUpdateInfoModel` | `IUpdateInfoModel` | `IUpdateInfoModel.cs` | `IUpdateInfoModel.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModel.cs.create|IUpdateInfoModel.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-i-update-info-model
//Plateau: default
//Version: 20260630010447
```

```csharp
// Shared/Timestamps/IUpdateInfoModel.cs
namespace Shared.Timestamps;

public interface IUpdateInfoModel : IUpdateInfoModelReadOnly
{
    new DateTimeOffset ServerUpdatedDateTime { get; set; }
    new DateTimeOffset UserUpdatedDateTime { get; set; }
}
```

Entities implement the setter explicitly to keep class-level setters internal:

```csharp
// {Module}.Domain/Entities/{EntityName}.cs
public class {EntityName} : ICreationInfoModel, IUpdateInfoModel
{
    public int Id { get; internal set; }
    public uint Version { get; internal set; }

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

    public DateTimeOffset ServerUpdatedDateTime { get; internal set; }
    public DateTimeOffset UserUpdatedDateTime { get; internal set; }

    DateTimeOffset IUpdateInfoModel.ServerUpdatedDateTime
    {
        get => ServerUpdatedDateTime;
        set => ServerUpdatedDateTime = value;
    }

    DateTimeOffset IUpdateInfoModel.UserUpdatedDateTime
    {
        get => UserUpdatedDateTime;
        set => UserUpdatedDateTime = value;
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModel.cs.create|IUpdateInfoModel.cs]]

# Rules
MUST:
	- Inherit `IUpdateInfoModelReadOnly`.
	- Re-declare both properties with `get; set;`.
	- Be implemented by `Internal Mutable` and `External Mutable` entities.
MUST NOT:
	- Be implemented by `External Immutable` or `Internal Immutable` entities.
	- Add behavior methods.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModel.cs.create|IUpdateInfoModel.cs]]

# Anti-patterns
- Implementing this interface implicitly with public setters — breaks the "no public setters on entities" rule.
- Adding update timestamps to an entity that is never updated.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModel.cs.create|IUpdateInfoModel.cs]]

# Check list
- [ ] Interface defined in `Shared/Timestamps/IUpdateInfoModel.cs`.
- [ ] Inherits `IUpdateInfoModelReadOnly`.
- [ ] Both properties have `get` and `set`.
- [ ] Entity class-level timestamp properties have `internal set`.
- [ ] Entity implements the interface setter explicitly.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModel.cs.create|IUpdateInfoModel.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN allow assignment through `IUpdateInfoModel`.
- [ ] WHEN applied THEN keep entity class-level setter internal.
- [ ] WHEN applied THEN satisfy `IUpdateInfoModelReadOnly` through inheritance.
- [ ] WHEN naming THEN pattern matches convention.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/Shared.csproj.extend/IUpdateInfoModel.cs.create|IUpdateInfoModel.cs]]
