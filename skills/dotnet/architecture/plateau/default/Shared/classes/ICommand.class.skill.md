---
uid: 5bcf46b7-e406-4dca-8216-9590a2bb67c8
name: icommand-class
description: Write operation marker interfaces
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration.solution.skill]]"
---

# Goal
- Mark a MediatR request as a write operation so pipeline behaviors can activate selectively on commands only
- Provide two variants: `ICommand` for commands with no response value, `ICommand<TResponse>` for commands that return a result

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]

# Core Principals
- Interface only — no properties, no methods
- `ICommand<TResponse>` is the standard form — almost all commands return `Result<T>`
- Pipeline behaviors in BuildingBlocks use `where TRequest : ICommand` to activate only for write operations

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Command marker (no return) | `ICommand` | `ICommand` | `ICommand.cs` | `ICommand.cs` |
| Command marker (with return) | `ICommand<TResponse>` | `ICommand<Result<CreateTaskResult>>` | `ICommand.cs` | `ICommand.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]

# Implementation
Both variants defined in one file:

```csharp
// Shared/MediatR/ICommand.cs
using MediatR;

namespace Shared.MediatR;

public interface ICommand : IRequest { }

public interface ICommand<TResponse> : IRequest<TResponse> { }
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]

# Rules
MUST:
	- All command records implement `ICommand<Result<T>>` — not `IRequest<T>` directly
	- `ICommand` used only when the command truly produces no return value
	- Defined in Shared — never in BuildingBlocks or any module project
MUST NOT:
	- Add methods or properties to the marker interfaces

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Mark a MediatR request as a write operation so pipeline behaviors can activate selectively on commands only
- [ ] WHEN component is requested THEN it provide two variants: ICommand for commands with no response value, ICommand<TResponse> for commands that return a result
- [ ] WHEN applied THEN Interface only — no properties, no methods
- [ ] WHEN applied THEN ICommand<TResponse> is the standard form — almost all commands return Result<T>
- [ ] WHEN applied THEN Pipeline behaviors in BuildingBlocks use where TRequest : ICommand to activate only for write operations
- [ ] WHEN naming 'Command marker (no return)' THEN pattern matches convention
- [ ] WHEN naming 'Command marker (with return)' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/Shared.csproj.extend/ICommand.cs.create.md|ICommand.cs.create]]
