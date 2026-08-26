---
name: class-i-command
description: Class ICommand in the shared-rules plateau
whenToUse: when declaring a new command record, or deciding whether a MediatR request is a command
domain: skill
type: template
plateau: shared-rules
version: 20260824163000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
---

# Goal
- Mark a MediatR request as a write operation so pipeline behaviors can activate selectively on commands only
- Provide two variants: `ICommand` for commands with no response value, `ICommand<TResponse>` for commands that return a result

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]

# Core Principles
- Interface only — no properties, no methods
- `ICommand<TResponse>` is the standard form — almost all commands return `Result<T>`
- Lives in Shared — every layer can reference it without coupling to BuildingBlocks

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Command marker (no return) | ICommand | ICommand | ICommand.cs | ICommand.cs |
| Command marker (with return) | ICommand<TResponse> | ICommand<Result<CreateTaskResult>> | ICommand.cs | ICommand.cs |

# Implementation
```csharp
//Skill: class-i-command
//Plateau: shared-rules
//Version: 20260824163000

public interface ICommand : IRequest<Result> { }

public interface ICommand<TResponse> : IRequest<TResponse> { }
```

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]

# Rules
MUST:
- All command records implement `ICommand<Result<T>>` — not `IRequest<T>` directly
- `ICommand` and `ICommand<TResponse>` defined in Shared — not BuildingBlocks, not any module
MUST NOT:
- Add methods or properties to the marker interfaces

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]

# Check list
- [ ] `ICommand`/`ICommand<TResponse>` live in `Shared/MediatR/ICommand.cs`
- [ ] Neither variant carries properties or methods

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]
