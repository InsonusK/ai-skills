---
description: Read-only creation timestamp contract
project_name: Shared
name: ICreationInfoModelReadOnly.cs
element_kind: class
change_kind: create
tags:
  - solution/entity-edit-timestamp
  - element/icreationinfomodelreadonly-cs
---

# Goals
- Expose creation timestamps for read models, DTOs, and other consumers that must not modify them.

# Core Principles
- Read-only contract — no setters.
- Implemented by read models and by `ICreationInfoModel` through inheritance.

# Structure

## Project Structure
```
/Shared
  /Timestamps
    ICreationInfoModelReadOnly.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Read-only creation timestamps | `ICreationInfoModelReadOnly` | `ICreationInfoModelReadOnly` | `ICreationInfoModelReadOnly.cs` | `ICreationInfoModelReadOnly.cs` |

# Implementation changes

```csharp
// Shared/Timestamps/ICreationInfoModelReadOnly.cs
namespace Shared.Timestamps;

public interface ICreationInfoModelReadOnly
{
    DateTimeOffset ServerCreatedDateTime { get; }
    DateTimeOffset UserCreatedDateTime { get; }
}
```

# Rule changes

## MUST
- Declare only getters.
- Use `DateTimeOffset` for both properties.
- Never add setters or methods.

## SHOULD
- Avoid using this interface where mutation is required — use `ICreationInfoModel` instead.

# Check list
- [ ] Interface defined in `Shared/Timestamps/ICreationInfoModelReadOnly.cs`.
- [ ] Both properties are `DateTimeOffset` with getters only.

# Unittest TestCases
- [ ] WHEN applied THEN expose `ServerCreatedDateTime` as read-only.
- [ ] WHEN applied THEN expose `UserCreatedDateTime` as read-only.
- [ ] WHEN naming THEN pattern matches convention.
