---
name: class-i-command
description: Class ICommand in the service-with-api plateau
whenToUse: when declaring a new command record, or deciding whether a MediatR request is a command
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/class
  - plateau/service-with-api
created_by:
  - "[[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
---

# Goal
- Mark a MediatR request as a write operation so pipeline behaviors can activate selectively on commands only
- Provide two variants: `ICommand` for commands that return only `Result` (success/failure, no payload), `ICommand<TResponse>` for commands that return `Result<TResponse>`

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]

# Core Principles
- Interface only — no properties, no methods
- `ICommand<TResponse>` is the standard form — almost all commands return `Result<TResponse>`
- `ICommand` is for commands that signal success/failure without returning a payload
- Lives in Shared — every layer can reference it without coupling to BuildingBlocks

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Command marker (no payload) | ICommand | ICommand | ICommand.cs | ICommand.cs |
| Command marker (with payload) | ICommand<TResponse> | ICommand<CreateTaskResult> | ICommand.cs | ICommand.cs |

# Implementation
```csharp
//Skill: class-i-command
//Plateau: service-with-api
//Version: 20260825120000

using Ardalis.Result;
using MediatR;

public interface ICommand : IRequest<Result> { }

public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }
```

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]

# Rules
MUST:
- All command records implement `ICommand<T>` — the marker wraps the response as `Result<T>` automatically
- `ICommand` used for commands that return only success/failure with no payload
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
