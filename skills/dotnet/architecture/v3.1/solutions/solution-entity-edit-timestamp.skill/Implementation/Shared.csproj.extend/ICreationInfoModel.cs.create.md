---
description: Mutable creation timestamp contract implemented by entities
project_name: Shared
name: ICreationInfoModel.cs
element_kind: class
change_kind: create
tags:
  - solution/entity-edit-timestamp
  - element/icreationinfomodel-cs
---

# Goals
- Allow handlers and `AppDbContext` to assign creation timestamps while keeping the entity's own setters internal.

# Core Principles
- Inherits the read-only contract so entities automatically satisfy `ICreationInfoModelReadOnly`.
- Re-declares properties with setters to form a mutable contract.
- Entities implement this interface explicitly for the setter so the class-level property setter can remain `internal`.

# Structure

## Project Structure
```
/Shared
  /Timestamps
    ICreationInfoModel.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Mutable creation timestamps | `ICreationInfoModel` | `ICreationInfoModel` | `ICreationInfoModel.cs` | `ICreationInfoModel.cs` |

# Implementation changes

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
# Rule changes

## MUST
- Inherit `ICreationInfoModelReadOnly`.
- Re-declare both properties with `get; set;`.
- Be implemented by `External Immutable`, `Internal Mutable`, and `External Mutable` entities.
- `Internal Mutable` and `External Mutable` entities implement both `ICreationInfoModel` and `IUpdateInfoModel`.
- `External Immutable` entities implement `ICreationInfoModel` only.
- `OnBeforeSaving()` sets `ServerCreatedDateTime` for `Added` entries that implement `ICreationInfoModel`.
- Never be implemented by `Internal Immutable` entities.
- Never add behavior methods.

## SHOULD
- Avoid implementing this interface implicitly with public setters — breaks the "no public setters on entities" rule.
- Avoid forgetting to implement the setter explicitly when the class-level setter is `internal`.

# Check list
- [ ] Interface defined in `Shared/Timestamps/ICreationInfoModel.cs`.
- [ ] Inherits `ICreationInfoModelReadOnly`.
- [ ] Both properties have `get` and `set`.
- [ ] Entity class-level timestamp properties have `internal set`.
- [ ] Entity implements the interface setter explicitly.

# Unittest TestCases
- [ ] WHEN applied THEN allow assignment through `ICreationInfoModel`.
- [ ] WHEN applied THEN keep entity class-level setter internal.
- [ ] WHEN applied THEN satisfy `ICreationInfoModelReadOnly` through inheritance.
- [ ] WHEN naming THEN pattern matches convention.
