---
description: Command and result record declaration
project_name: "{Module}.Interfaces"
name: "{Command}.cs"
element_kind: class
change_kind: create
tags:
  - solution/mediator-integration
  - element/command-cs
---

# Goals
- Express a named write intent as an immutable record that carries all input needed for the operation
- Implement `ICommand<TResponse>` (`ICommand<Result>` when there is no payload beyond success/failure) so the MediatR pipeline routes it to the correct handler and activates every write-side behavior constrained on `ICommand<TResponse>`; reserve bare `ICommand` for commands with no persisted-entity effect at all

# Core Principles
- Declared as `record` — immutable, structural equality by default
- Implements `ICommand<Result<{CommandName}Result>>` — `Result<T>` is written explicitly in the type argument, not auto-wrapped by the marker interface
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
| Domain action that stages a persisted-entity write | `{Verb}{Entity}Command` | `AssignTaskCommand` | `{Command}.cs` | `AssignTaskCommand.cs` |
| Command result | `{CommandName}Result` | `CreateTaskResult` | same file as command | `CreateTaskCommand.cs` |

# Implementation changes

Command and result declared together in one file:

```csharp
// {Module}.Interfaces/Commands/CreateTaskCommand.cs
using Ardalis.Result;
using Shared;

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
using Shared;

namespace {Module}.Interfaces.Commands;

public record AssignTaskCommand(
    int TaskId,
    int AssigneeId
) : ICommand<Result>;
```

`AssignTaskCommand` stages a persisted-entity write (it mutates a `Task`), so it needs `UnitOfWorkBehavior` (and any other `ICommand<TResponse>`-constrained pipeline behavior) to activate for it — see the MUST rule below. It implements `ICommand<Result>` rather than bare `ICommand`, even though it returns no payload beyond success/failure: `Result` is a legitimate, if minimal, `TResponse`.

## Command property order (the fixed convention)

Later per-entity solutions (`solution-external-created-entity` VP6, `solution-entity-edit-timestamp` VP7, `solution-entity-concurrency-change` VP5) each add a field to some commands. To keep the record deterministic no matter which combination applies, the order is **fixed here**, and no such solution claims "first":

1. **Business fields** — the command's own meaningful inputs.
2. `Guid` — client-generated id (VP6), present only for a Create command of an external-created entity.
3. `ActionTimeStamp` — user action time (VP7), present only for a command of a user-initiated entity.
4. Version / concurrency token (VP5) — present only on an update command of a concurrency-controlled entity.

Each of steps 2–4 appears only when its VP applies; the relative order is what is fixed. See the [delta-conflict analysis](skills/dotnet/architecture/v3.1/delta-conflict-analysis.md#command-cs) for why (`FMC` between VP6 and VP7, resolved by convention).

# Rule changes

## MUST
- Implement `ICommand<Result<T>>` for a result payload of `T`, or `ICommand<Result>` when no payload beyond success/failure is returned — never `IRequest<T>` directly
- Follow the fixed command property order above: business fields, then `Guid` (VP6), then `ActionTimeStamp` (VP7), then a version token (VP5) — each only if its solution is applied. No later solution reorders an earlier one's field or claims a fixed absolute position.
  - Risk: two per-entity solutions each requiring "first property" conflict directly (this is the v3 `command-cs` registry entry).
  - Fix: the sub-order is defined here once; each solution appends at its slot.
- Any command that stages a write to a persisted entity (and therefore needs `UnitOfWorkBehavior`, or any other behavior constrained on `ICommand<TResponse>`, to commit or guard it) must implement `ICommand<TResponse>` — using `ICommand<Result>` when there is no payload beyond success/failure. Bare, non-generic `ICommand` is reserved for commands with no persisted-entity effect at all (pure domain computation, an external call, orchestration that only dispatches further commands)
- Result type declared in the same file as the command
- Properties are primitives or simple types — no domain entity references
- Commands declared as `record` in `/{Module}.Interfaces/Commands`
- One handler per command — `IRequestHandler<TCommand, Result<T>>`
- One `AbstractValidator<TCommand>` per command — co-located with handler in feature folder
- Validator extends `AbstractValidator<TCommand>`
- When a command property is a `Soft{ValueObject}` from another module, inject `IValidator<Soft{ValueObject}>` and use `SetValidator`
- When a command property is a DTO from another module, inject `IValidator<{Dto}>` and use `SetValidator`
- Never put methods or logic on a command
- Never reference a domain entity type as a command property
- Never duplicate, in a command validator, a rule already defined in `{ValueObject}PropertyValidator` or `{Dto}Validator` (`solution-dto-property-validators`)

## SHOULD
- Validator rules cover all command properties that carry input constraints

# Unittest TestCases
- [ ] WHEN applied THEN Express a named write intent as an immutable record that carries all input needed for the operation
- [ ] WHEN inspected THEN it implement ICommand<T> (or ICommand when no payload) so the MediatR pipeline routes it to the correct handler and activates write-side behaviors
- [ ] WHEN applied THEN Declared as record — immutable, structural equality by default
- [ ] WHEN applied THEN Implements ICommand<Result<{CommandName}Result>> — return type is always wrapped in Result<T>, written explicitly in the type argument
- [ ] WHEN applied THEN Properties are primitives or simple value types — no domain entity references
- [ ] WHEN applied THEN Result record declared in the same file — named {CommandName}Result
- [ ] WHEN applied THEN One command per write intent
- [ ] WHEN naming 'Create entity' THEN pattern matches convention
- [ ] WHEN naming 'Update entity' THEN pattern matches convention
- [ ] WHEN naming 'Delete entity' THEN pattern matches convention
- [ ] WHEN naming 'Domain action' THEN pattern matches convention
- [ ] WHEN naming 'Command result' THEN pattern matches convention
