---
name: class-i-query
description: Read-only operation marker interface
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration.skill]]"
---

# Goal
- Mark a MediatR request as a read-only operation
- Keep read-side markers distinct from write-side markers

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend/IQuery.cs.create.md|IQuery.cs.create]]

# Core Principals
- Apply ONE plateau template per class
- Interface only — no properties, no methods
- Extends `IRequest<TResponse>` so MediatR routes it to a handler
- Does NOT extend `ICommand` — queries are read-only operations and must remain distinct from write-side markers

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend/IQuery.cs.create.md|IQuery.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Query marker | `IQuery<TResponse>` | `IQuery<Result<TaskDto>>` | `IQuery.cs` | `IQuery.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend/IQuery.cs.create.md|IQuery.cs.create]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-i-query
//Plateau: default
//Version: 20260628
```

```csharp
// Shared/MediatR/IQuery.cs
using MediatR;

namespace Shared.MediatR;

public interface IQuery<TResponse> : IRequest<TResponse> { }
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend/IQuery.cs.create.md|IQuery.cs.create]]

# Rules
MUST:
	- All query records implement `IQuery<Result<T>>` — not `IRequest<T>` directly
	- Defined in Shared — never in BuildingBlocks or any module project
	- `IQuery<TResponse>` extends `IRequest<TResponse>`
MUST NOT:
	- Extend `ICommand` or `ICommand<TResponse>`
	- Contain properties or methods

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend/IQuery.cs.create.md|IQuery.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Mark a MediatR request as a read-only operation
- [ ] WHEN applied THEN Keep read-side markers distinct from write-side markers
- [ ] WHEN applied THEN Interface only — no properties, no methods
- [ ] WHEN applied THEN Extends IRequest<TResponse> so MediatR routes it to a handler
- [ ] WHEN applied THEN Does NOT extend ICommand — queries are read-only operations and must remain distinct from write-side markers
- [ ] WHEN naming 'Query marker' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/Shared.csproj.extend/IQuery.cs.create.md|IQuery.cs.create]]
