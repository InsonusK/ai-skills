---
name: class-command
description: Class {Command} in the v1 plateau
whenToUse: when declaring a new write-intent command for this module
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
  - "[[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]]"
---

# Goal
- Express a named write intent as an immutable record that carries all input needed for the operation
- Implement `ICommand<Result<T>>` so the MediatR pipeline routes it to the correct handler and activates write-side behaviors

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Core Principles
- Declared as `record` — immutable, structural equality by default
- Implements `ICommand<Result<{CommandName}Result>>` — return type is always `Result<T>`
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
A command implements exactly the extra marker interfaces its target entity's classification requires — `IHasVersions` for an update/patch on a mutable entity, `IHasGuid` for a create on an externally-created entity, `ICommandWithTimestamp` for either, on a user-initiated entity (see `class-entity`'s Entity Type Matrix / Timestamp Matrix).

```csharp
//Skill: class-command
//Plateau: v1
//Version: 20260825140000

// Create, on an External Immutable entity — carries the client Guid and the action timestamp
public record CreateAttachmentCommand(
    Guid Guid,
    string FileName
) : ICommand<Result<CreateAttachmentResult>>, IHasGuid, ICommandWithTimestamp
{
    public DateTimeOffset ActionTimeStamp { get; init; }
}

public record CreateAttachmentResult(int Id);

// Update, on an Internal Mutable entity — carries expected versions and the action timestamp
public record UpdateOrderCommand(
    int OrderId,
    string Comment,
    IReadOnlyDictionary<string, uint> Versions
) : ICommand<Result>, IHasVersions, ICommandWithTimestamp
{
    public DateTimeOffset ActionTimeStamp { get; init; }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Rules
MUST:
- Implement `ICommand<Result<T>>` or `ICommand<Result>` — never `IRequest<T>` directly
- Result type declared in the same file as the command
- Properties are primitives, Value Objects, or DTOs — no domain entity references
- Commands declared as `record` in `/{Module}.Interfaces/Commands`
- When a command property is a `Soft{ValueObject}`/DTO from another module, its `{FeatureName}.Validator.cs` injects `IValidator<T>` and uses `SetValidator`
- Implement `IHasVersions` for every update/patch on a mutable entity, `IHasGuid` for every create on an externally-created entity, `ICommandWithTimestamp` for either on a user-initiated entity — exactly per the target entity's classification, never more
MUST NOT:
- Contain methods or logic
- Reference domain entity types as properties
- Have its validator duplicate rules already defined in `{ValueObject}PropertyValidator` or `{Dto}Validator`
- Implement `IHasVersions`/`IHasGuid` when the target entity's classification forbids it (e.g. `IHasVersions` on a create command, `IHasGuid` on an internal entity's command)

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Check list
- [ ] Command is a `record` in `/{Module}.Interfaces/Commands`, implements `ICommand<Result<T>>`
- [ ] Result record co-located in the same file
- [ ] No domain entity types among the command's properties

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]
