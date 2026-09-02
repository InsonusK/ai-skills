---
name: plateau-offline-sync-service--class-i-command
description: Class ICommand / ICommand<TResponse> in the plateau-offline-sync-service plateau — the write-intent request markers in Shared/MediatR
whenToUse: when creating or editing the ICommand markers in Shared/MediatR, or deciding whether a new request marker belongs in Shared
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
- Give a module one write-intent marker that routes a request to its handler and activates every write-side pipeline behavior.
- `ICommand` for no payload beyond success/failure; `ICommand<TResponse>` when the caller needs a value back.

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- Pure markers — no members. `ICommand : IRequest<Result>`, `ICommand<TResponse> : IRequest<TResponse>`.
- The `Result<T>` wrapper is written explicitly by the command (`ICommand<Result<{X}Result>>`), never implied by the marker.
- Lives in `Shared/MediatR`, `namespace Shared.MediatR` — never in `BuildingBlocks`.

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Write marker, no payload | `ICommand` | `ICommand` | `ICommand.cs` | `ICommand.cs` |
| Write marker, payload | `ICommand<TResponse>` | `ICommand<Result<CreateTaskResult>>` | `ICommand.cs` | `ICommand.cs` |

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-i-command
// Plateau: core
// Version: 20260902000000
using Ardalis.Result;
using MediatR;

namespace Shared.MediatR;

public interface ICommand : IRequest<Result> { }

public interface ICommand<TResponse> : IRequest<TResponse> { }
```

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]

# Rules
MUST:
- Keep both interfaces member-free; both extend a MediatR `IRequest<>`.
- Place them in `Shared/MediatR/ICommand.cs`, `namespace Shared.MediatR`.
- Never apply several plateau templates per class.
- Never add a member, a default method, or a base other than `IRequest<>`.
- Never define this marker in `BuildingBlocks`.

# Check list
- [ ] `ICommand : IRequest<Result>` and `ICommand<TResponse> : IRequest<TResponse>`, both empty.
- [ ] File is `Shared/MediatR/ICommand.cs`, `namespace Shared.MediatR`.

# Unittest TestCases
- [ ] WHEN the markers are inspected THEN each is in `Shared.MediatR` and declares no instance members.
- [ ] WHEN a command implements `ICommand<Result<T>>` THEN MediatR resolves its `IRequestHandler`.
