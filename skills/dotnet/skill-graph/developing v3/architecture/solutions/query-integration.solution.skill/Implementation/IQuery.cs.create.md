---
description: Read-only operation marker interface
project_name: Shared
name: IQuery.cs
change_kind: create
---

# Goals
- Mark a MediatR request as a read-only operation
- Exclude the request from `ValidationBehavior` by not implementing `ICommand`

# Core Principles
- Interface only — no properties, no methods
- Extends `IRequest<TResponse>` so MediatR routes it to a handler
- Does NOT extend `ICommand` — this is the mechanism that excludes it from write-side behaviors

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Query marker | `IQuery<TResponse>` | `IQuery<Result<TaskDto>>` | `IQuery.cs` | `IQuery.cs` |

# Implementation changes

```csharp
// Shared/MediatR/IQuery.cs
using MediatR;

namespace Shared.MediatR;

public interface IQuery<TResponse> : IRequest<TResponse> { }
```

# Rules

MUST:
- All query records implement `IQuery<Result<T>>` — not `IRequest<T>` directly
- Defined in Shared — never in BuildingBlocks or any module project
- `IQuery<TResponse>` extends `IRequest<TResponse>`

MUST NOT:
- Extend `ICommand` or `ICommand<TResponse>`
- Contain properties or methods
