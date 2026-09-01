---
description: Read-only operation marker interface
project_name: Shared
name: IQuery.cs
element_kind: class
change_kind: create
tags:
  - solution/query-integration
  - element/iquery-cs
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

namespace Shared;

public interface IQuery<TResponse> : IRequest<TResponse> { }
```
# Rule changes

## MUST
- All query records implement `IQuery<Result<T>>` — not `IRequest<T>` directly
- `IQuery<TResponse>` extends `IRequest<TResponse>`
- `IQuery<TResponse>` defined in Shared — not BuildingBlocks, not any module
- `IQuery` does not extend `ICommand` — queries are read-only operations and must remain distinct from write-side markers
- Never extend `ICommand` or `ICommand<TResponse>`
- Never contain properties or methods

# Unittest TestCases
- [ ] WHEN applied THEN Mark a MediatR request as a read-only operation
- [ ] WHEN applied THEN Keep read-side markers distinct from write-side markers
- [ ] WHEN applied THEN Interface only — no properties, no methods
- [ ] WHEN applied THEN Extends IRequest<TResponse> so MediatR routes it to a handler
- [ ] WHEN applied THEN Does NOT extend ICommand — queries are read-only operations and must remain distinct from write-side markers
- [ ] WHEN naming 'Query marker' THEN pattern matches convention
