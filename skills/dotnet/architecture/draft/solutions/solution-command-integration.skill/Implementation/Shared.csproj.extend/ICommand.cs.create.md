---
description: Write operation marker interfaces
project_name: Shared
name: ICommand.cs
element_kind: class
change_kind: create
tags:
  - solution/command-integration
  - element/icommand-cs
---

# Goals
- Mark a MediatR request as a write operation so pipeline behaviors can activate selectively on commands only
- Provide two variants: `ICommand` for commands that return only `Result` (success/failure, no payload), `ICommand<TResponse>` for commands that return `Result<TResponse>`

# Core Principles
- Interface only — no properties, no methods
- `ICommand<TResponse>` is the standard form — almost all commands return `Result<T>`, written explicitly as `ICommand<Result<T>>` (the marker is a raw pass-through to `IRequest<TResponse>`, it does not auto-wrap `TResponse` in `Result<>`)
- `ICommand` is for commands that signal success/failure without returning a payload
- Pipeline behaviors in BuildingBlocks use `where TRequest : ICommand<TResponse>` to activate only for write operations

# Structure

## Project Structure
```
/Shared
  /MediatR
    ICommand.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Command marker (no payload) | `ICommand` | `ICommand` | `ICommand.cs` | `ICommand.cs` |
| Command marker (with payload) | `ICommand<TResponse>` | `ICommand<Result<CreateTaskResult>>` | `ICommand.cs` | `ICommand.cs` |

# Implementation changes

Both variants defined in one file:

```csharp
// Shared/MediatR/ICommand.cs
using Ardalis.Result;
using MediatR;

namespace Shared.MediatR;

public interface ICommand : IRequest<Result> { }

public interface ICommand<TResponse> : IRequest<TResponse> { }
```
# Rule changes

## MUST
- All command records implement `ICommand<Result<T>>` — not `IRequest<T>` directly
- `ICommand` used only when the command truly produces no return payload
- `ICommand` and `ICommand<TResponse>` defined in Shared — not BuildingBlocks, not any module

## MUST NOT
- Add methods or properties to the marker interfaces
- `ICommand` defined in BuildingBlocks — belongs in Shared

# Unittest TestCases
- [ ] WHEN applied THEN Mark a MediatR request as a write operation so pipeline behaviors can activate selectively on commands only
- [ ] WHEN component is requested THEN it provide two variants: ICommand for commands with no payload, ICommand<TResponse> for commands that return Result<TResponse>
- [ ] WHEN applied THEN Interface only — no properties, no methods
- [ ] WHEN applied THEN ICommand<TResponse> is the standard form — almost all commands return Result<T>, written explicitly as ICommand<Result<T>>
- [ ] WHEN applied THEN Pipeline behaviors in BuildingBlocks use where TRequest : ICommand<TResponse> to activate only for write operations
- [ ] WHEN naming 'Command marker (no payload)' THEN pattern matches convention
- [ ] WHEN naming 'Command marker (with payload)' THEN pattern matches convention
