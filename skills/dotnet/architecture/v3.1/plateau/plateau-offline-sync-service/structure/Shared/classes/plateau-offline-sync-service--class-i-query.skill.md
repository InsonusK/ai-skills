---
name: plateau-offline-sync-service--class-i-query
description: Class IQuery<TResponse> in the plateau-offline-sync-service plateau — the read-intent request marker in Shared/MediatR
whenToUse: when creating or editing the IQuery marker in Shared/MediatR, or deciding whether a request is a read (IQuery) or a write (ICommand)
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
---

# Goal
- Give a module one read-intent marker, distinct from `ICommand`, so the read/write split is visible at the type level and write-side behaviors never activate for a read.

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/IQuery.cs.create.md|IQuery.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- Pure marker — no members. `IQuery<TResponse> : IRequest<TResponse>`.
- A query that can fail to find its target uses `IQuery<Result<{X}Dto>>`; one that always returns data uses `IQuery<IReadOnlyList<{X}Dto>>`.
- Lives in `Shared/MediatR`, `namespace Shared.MediatR`.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Read marker | `IQuery<TResponse>` | `IQuery<Result<TaskDto>>` | `IQuery.cs` | `IQuery.cs` |

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-i-query
// Plateau: core
// Version: 20260902000000
using MediatR;

namespace Shared.MediatR;

public interface IQuery<TResponse> : IRequest<TResponse> { }
```

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/IQuery.cs.create.md|IQuery.cs.create]]

# Rules
MUST:
- Keep it member-free; extend `IRequest<TResponse>`.
- Place it in `Shared/MediatR/IQuery.cs`, `namespace Shared.MediatR`.
- Never apply several plateau templates per class.
- Never add a member or a base other than `IRequest<TResponse>`.
- Never use `IQuery` for a request that mutates state — that is `ICommand`.

# Check list
- [ ] `IQuery<TResponse> : IRequest<TResponse>`, empty, in `Shared/MediatR/IQuery.cs`.

# Unittest TestCases
- [ ] WHEN the marker is inspected THEN it is in `Shared.MediatR` and declares no instance members.
