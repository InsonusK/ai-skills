---
name: class-command
description: Class {Command} in the shared-rules plateau
whenToUse: when declaring a new write-intent command for this module
domain: skill
type: template
plateau: shared-rules
version: 20260824150000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
---

# Goal
- Express a named write intent as an immutable record that carries all input needed for the operation
- Implement `ICommand<T>` (or `ICommand` when no payload is returned) so the MediatR pipeline routes it to the correct handler and activates write-side behaviors

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Core Principles
- Declared as `record` — immutable, structural equality by default
- Implements `ICommand<{CommandName}Result>` — the marker interface wraps the response in `Result<T>` automatically
- Properties are primitives, Value Objects (`Soft{ValueObject}`), or DTOs — no domain entity references
- Result record declared in the same file, named `{CommandName}Result`
- One command per write intent

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Create entity | Create{Entity}Command | CreateTaskCommand | {Command}.cs | CreateTaskCommand.cs |
| Update entity | Update{Entity}Command | UpdateTaskCommand | {Command}.cs | UpdateTaskCommand.cs |
| Domain action | {Verb}{Entity}Command | AssignTaskCommand | {Command}.cs | AssignTaskCommand.cs |
| Command result | {CommandName}Result | CreateTaskResult | same file as command | CreateTaskCommand.cs |

# Implementation
```csharp
//Skill: class-command
//Plateau: shared-rules
//Version: 20260824150000

public record CreateTaskCommand(
    string Title,
    int AssigneeId
) : ICommand<CreateTaskResult>;

public record CreateTaskResult(int Id);
```

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Rules
MUST:
- Implement `ICommand<T>` or `ICommand` — never `IRequest<T>` directly
- Result type declared in the same file as the command
- Properties are primitives, Value Objects, or DTOs — no domain entity references
- Commands declared as `record` in `/{Module}.Interfaces/Commands`
- When a command property is a `Soft{ValueObject}`/DTO from another module, its `{FeatureName}.Validator.cs` injects `IValidator<T>` and uses `SetValidator`
MUST NOT:
- Contain methods or logic
- Reference domain entity types as properties
- Have its validator duplicate rules already defined in `{ValueObject}PropertyValidator` or `{Dto}Validator`

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Check list
- [ ] Command is a `record` in `/{Module}.Interfaces/Commands`, implements `ICommand<T>` (or `ICommand` when no payload is returned)
- [ ] Result record co-located in the same file
- [ ] No domain entity types among the command's properties

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]
