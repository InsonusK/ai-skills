---
description: Read-only update timestamp contract
project_name: Shared
name: IUpdateInfoModelReadOnly.cs
element_kind: class
change_kind: create
tags:
  - solution/entity-edit-timestamp
  - element/iupdateinfomodelreadonly-cs
---

# Goals
- Expose update timestamps for read models, DTOs, and other consumers that must not modify them.

# Core Principles
- Read-only contract — no setters.
- Implemented by read models and by `IUpdateInfoModel` through inheritance.

# Structure

## Project Structure
```
/Shared
  /Timestamps
    IUpdateInfoModelReadOnly.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Read-only update timestamps | `IUpdateInfoModelReadOnly` | `IUpdateInfoModelReadOnly` | `IUpdateInfoModelReadOnly.cs` | `IUpdateInfoModelReadOnly.cs` |

# Implementation changes

```csharp
// Shared/Timestamps/IUpdateInfoModelReadOnly.cs
namespace Shared.Timestamps;

public interface IUpdateInfoModelReadOnly
{
    DateTimeOffset ServerUpdatedDateTime { get; }
    DateTimeOffset UserUpdatedDateTime { get; }
}
```

# Rule changes

## MUST
- Declare only getters.
- Use `DateTimeOffset` for both properties.
- Never add setters or methods.

## SHOULD
- Avoid using this interface where mutation is required — use `IUpdateInfoModel` instead.

# Check list
- [ ] Interface defined in `Shared/Timestamps/IUpdateInfoModelReadOnly.cs`.
- [ ] Both properties are `DateTimeOffset` with getters only.

# Unittest TestCases
- [ ] WHEN applied THEN expose `ServerUpdatedDateTime` as read-only.
- [ ] WHEN applied THEN expose `UserUpdatedDateTime` as read-only.
- [ ] WHEN naming THEN pattern matches convention.
