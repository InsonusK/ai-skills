---
description: Read-only operation marker interface
project_name: Shared
name: IQuery.cs
element_kind: class
change_kind: create
---

# Goals
- Mark a MediatR request as a read-only operation
- Keep read-side markers distinct from write-side markers

# Core Principles
- Interface only — no properties, no methods
- Extends `IRequest<TResponse>` so MediatR routes it to a handler
- Does NOT extend `ICommand` — queries are read-only operations and must remain distinct from write-side markers

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
# Rule changes

## MUST
- All query records implement `IQuery<Result<T>>` — not `IRequest<T>` directly
- Defined in Shared — never in BuildingBlocks or any module project
- `IQuery<TResponse>` extends `IRequest<TResponse>`
- `IQuery<TResponse>` defined in Shared — not BuildingBlocks, not any module
- `IQuery` does not extend `ICommand` — queries are read-only operations and must remain distinct from write-side markers
- All queries implement `IQuery<Result<T>>` — not `IRequest<T>` directly

## MUST NOT
- Extend `ICommand` or `ICommand<TResponse>`
- Contain properties or methods
- `IQuery` extend `ICommand` — queries must remain distinct from write-side markers

# Unittest TestCases
- [ ] WHEN applied THEN Mark a MediatR request as a read-only operation
- [ ] WHEN applied THEN Keep read-side markers distinct from write-side markers
- [ ] WHEN applied THEN Interface only — no properties, no methods
- [ ] WHEN applied THEN Extends IRequest<TResponse> so MediatR routes it to a handler
- [ ] WHEN applied THEN Does NOT extend ICommand — queries are read-only operations and must remain distinct from write-side markers
- [ ] WHEN naming 'Query marker' THEN pattern matches convention
