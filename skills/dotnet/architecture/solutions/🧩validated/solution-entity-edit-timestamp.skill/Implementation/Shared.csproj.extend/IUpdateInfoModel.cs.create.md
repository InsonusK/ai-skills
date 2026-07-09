---
description: Mutable update timestamp contract implemented by entities
project_name: Shared
name: IUpdateInfoModel.cs
element_kind: class
change_kind: create
---

# Goals
- Allow handlers and `AppDbContext` to assign update timestamps while keeping the entity's own setters internal.

# Core Principles
- Inherits the read-only contract so entities automatically satisfy `IUpdateInfoModelReadOnly`.
- Re-declares properties with setters to form a mutable contract.
- Entities implement this interface explicitly for the setter so the class-level property setter can remain `internal`.

# Structure

## Project Structure
```
/Shared
  /Timestamps
    IUpdateInfoModel.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Mutable update timestamps | `IUpdateInfoModel` | `IUpdateInfoModel` | `IUpdateInfoModel.cs` | `IUpdateInfoModel.cs` |

# Implementation changes

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
# Rule changes

## MUST
- Inherit `IUpdateInfoModelReadOnly`.
- Re-declare both properties with `get; set;`.
- Be implemented by `Internal Mutable` and `External Mutable` entities.
- `OnBeforeSaving()` sets `ServerUpdatedDateTime` for `Added` or `Modified` entries that implement `IUpdateInfoModel`.

## MUST NOT
- Be implemented by `External Immutable` or `Internal Immutable` entities.
- Add behavior methods.

# Anti-patterns
- Implementing this interface implicitly with public setters — breaks the "no public setters on entities" rule.
- Adding update timestamps to an entity that is never updated.

# Check list
- [ ] Interface defined in `Shared/Timestamps/IUpdateInfoModel.cs`.
- [ ] Inherits `IUpdateInfoModelReadOnly`.
- [ ] Both properties have `get` and `set`.
- [ ] Entity class-level timestamp properties have `internal set`.
- [ ] Entity implements the interface setter explicitly.

# Unittest TestCases
- [ ] WHEN applied THEN allow assignment through `IUpdateInfoModel`.
- [ ] WHEN applied THEN keep entity class-level setter internal.
- [ ] WHEN applied THEN satisfy `IUpdateInfoModelReadOnly` through inheritance.
- [ ] WHEN naming THEN pattern matches convention.
