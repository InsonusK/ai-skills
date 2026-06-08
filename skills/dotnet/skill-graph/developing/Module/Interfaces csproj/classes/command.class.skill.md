---
name: command
description: defines how to declare a command contract in the Interfaces project
domain: skill
type: class
tags:
  - skill/pattern/class
  - dotnet
  - cqrs
  - command
  - mediatr
triggers:
  - declare command
  - create command contract
  - write intent contract
---
# Goal
Define how to declare a MediatR command in `{Module}.Interfaces`. A command expresses write intent — it carries the input data needed to perform one state-changing operation. The declaration lives in Interfaces so any module can dispatch it without referencing the handler.

# Core Principles
- Command is a declaration only — no logic, no validation
- Implements `ICommand<Result<T>>` — activates `UnitOfWorkBehavior` in pipeline
- `record` type — immutable input contract
- Result type declares what the handler returns on success
- Validation belongs in Application validator — not here

# Governed by
- command-handling.solution.skill.md — pipeline this command flows through
- guid-resolving.solution.skill.md — if command implements `IHasGuid`
- concurrency-control.solution.skill.md — if command implements `IHasVersions`

# Structure
## Place in csproj
Defined in `module-interfaces.csproj.skill.md`
```
/{ModuleName}.Interfaces
  /Commands
    CreateTaskCommand.cs
```

## Naming convention
```
class name:
  rule: verb + entity name + Command suffix
  pattern: {Verb}{Entity}Command
  example: CreateTaskCommand

file name:
  rule: matches class name exactly
  pattern: {Verb}{Entity}Command.cs
  example: CreateTaskCommand.cs

result class name:
  rule: verb + entity name + Result suffix, declared in same file
  pattern: {Verb}{Entity}Result
  example: CreateTaskResult
```

# Contracts

## Simple command
```csharp
public record CreateTaskCommand(
    string Title,
    int AssigneeId
) : ICommand<Result<CreateTaskResult>>;

public record CreateTaskResult(int Id);
```

## Command with Guid — externally created entity
Implements `IHasGuid` to opt into `GuidResolvingBehavior`.
```csharp
public record CreateTaskCommand(
    Guid Guid,
    string Title,
    int AssigneeId
) : ICommand<Result<CreateTaskResult>>, IHasGuid;

public record CreateTaskResult(int Id);
```

## Command with versions — update operation
Implements `IHasVersions` to opt into `ConcurrencyBehavior`.
```csharp
public record UpdateTaskCommand(
    int TaskId,
    string Title,
    IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions
) : ICommand<Result>, IHasVersions;
```

## Delete command
```csharp
public record DeleteTaskCommand(int TaskId) : ICommand<Result>;
```

# Rules
MUST:
- Implement `ICommand<Result<T>>` or `ICommand<Result>`
- Be a `record` type
- Implement `IHasGuid` if entity is externally created
- Implement `IHasVersions` if command updates a mutable entity
- Result type declared in same file as command
MUST NOT:
- Contain validation logic
- Contain business logic
- Reference Domain entities or value objects
- Reference Application types

# Anti-patterns
- Command contains `if` logic — belongs in validator or handler
- Command returns raw DTO without `Result<T>` wrapper — handler cannot signal NotFound or Conflict
- Result type declared in a separate file — keep command and result co-located

# Checklist
- [ ] `record` type
- [ ] Implements `ICommand<Result<T>>`
- [ ] Result type in same file
- [ ] `IHasGuid` added if entity is externally created
- [ ] `IHasVersions` added if update command
- [ ] No logic of any kind

# Relations
- module-interfaces.csproj.skill.md — project this command lives in
- feature-command-handler.class.skill.md — handler that implements this command
- feature-validator.class.skill.md — validator that validates this command
- command-handling.solution.skill.md — pipeline this command flows through
