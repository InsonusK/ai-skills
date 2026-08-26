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
- Provide two variants: `ICommand` for commands with no response value, `ICommand<TResponse>` for commands that return a result

# Core Principles
- Interface only — no properties, no methods
- `ICommand<TResponse>` is the standard form — almost all commands return `Result<T>`
- Pipeline behaviors in BuildingBlocks use `where TRequest : ICommand` to activate only for write operations

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
| Command marker (no return) | `ICommand` | `ICommand` | `ICommand.cs` | `ICommand.cs` |
| Command marker (with return) | `ICommand<TResponse>` | `ICommand<Result<CreateTaskResult>>` | `ICommand.cs` | `ICommand.cs` |

# Implementation changes

Both variants defined in one file:

```csharp
// Shared/MediatR/ICommand.cs
using MediatR;

namespace Shared.MediatR;

public interface ICommand : IRequest { }

public interface ICommand<TResponse> : IRequest<TResponse> { }
```
# Rule changes

## MUST
- All command records implement `ICommand<Result<T>>` — not `IRequest<T>` directly
- `ICommand` used only when the command truly produces no return value
- `ICommand` and `ICommand<TResponse>` defined in Shared — not BuildingBlocks, not any module

## MUST NOT
- Add methods or properties to the marker interfaces
- `ICommand` defined in BuildingBlocks — belongs in Shared

# Unittest TestCases
- [ ] WHEN applied THEN Mark a MediatR request as a write operation so pipeline behaviors can activate selectively on commands only
- [ ] WHEN component is requested THEN it provide two variants: ICommand for commands with no response value, ICommand<TResponse> for commands that return a result
- [ ] WHEN applied THEN Interface only — no properties, no methods
- [ ] WHEN applied THEN ICommand<TResponse> is the standard form — almost all commands return Result<T>
- [ ] WHEN applied THEN Pipeline behaviors in BuildingBlocks use where TRequest : ICommand to activate only for write operations
- [ ] WHEN naming 'Command marker (no return)' THEN pattern matches convention
- [ ] WHEN naming 'Command marker (with return)' THEN pattern matches convention
