---
description: Command and result record declaration
name: "{Command}.cs"
change_kind: create
---

# Goals
- Express a named write intent as an immutable record that carries all input needed for the operation
- Implement `ICommand<Result<T>>` so the MediatR pipeline routes it to the correct handler and activates write-side behaviors

# Core Principles
- Declared as `record` — immutable, structural equality by default
- Implements `ICommand<Result<{CommandName}Result>>` — return type is always `Result<T>`
- Properties are primitives or simple value types — no domain entity references
- Result record declared in the same file — named `{CommandName}Result`
- One command per write intent

# Structure

## Project Structure
```
/{Module}.Interfaces
  /Commands
    {Command}.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Create entity | `Create{Entity}Command` | `CreateTaskCommand` | `{Command}.cs` | `CreateTaskCommand.cs` |
| Update entity | `Update{Entity}Command` | `UpdateTaskCommand` | `{Command}.cs` | `UpdateTaskCommand.cs` |
| Delete entity | `Delete{Entity}Command` | `DeleteTaskCommand` | `{Command}.cs` | `DeleteTaskCommand.cs` |
| Domain action | `{Verb}{Entity}Command` | `AssignTaskCommand` | `{Command}.cs` | `AssignTaskCommand.cs` |
| Command result | `{CommandName}Result` | `CreateTaskResult` | same file as command | `CreateTaskCommand.cs` |

# Implementation changes

Command and result declared together in one file:

```csharp
// {Module}.Interfaces/Commands/CreateTaskCommand.cs
using Ardalis.Result;
using Shared.MediatR;

namespace {Module}.Interfaces.Commands;

public record CreateTaskCommand(
    string Title,
    int AssigneeId
) : ICommand<Result<CreateTaskResult>>;

public record CreateTaskResult(int Id);
```

```csharp
// {Module}.Interfaces/Commands/AssignTaskCommand.cs
using Ardalis.Result;
using Shared.MediatR;

namespace {Module}.Interfaces.Commands;

public record AssignTaskCommand(
    int TaskId,
    int AssigneeId
) : ICommand<Result>;
```

# Rules

MUST:
- Declared as `record`
- Implement `ICommand<Result<T>>` or `ICommand<Result>` — never `IRequest<T>` directly
- Result type declared in the same file as the command
- Properties are primitives or simple types — no domain entity references

MUST NOT:
- Command contain methods or logic
- Command reference domain entity types as properties
