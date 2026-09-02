---
name: plateau-core--class-command
description: Class {Command} in the plateau-core plateau — a module's write-intent record (with its {Command}Result) in {Module}.Interfaces/Commands
whenToUse: when creating or editing a command record in {Module}.Interfaces/Commands, or deciding a command's property order and result shape
domain: skill
type: template
plateau: core
version: 20260902000000
tags:
  - skill/template/class
  - plateau/core
created_by:
  - "[[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
---

# Goal
- Express one named write intent as an immutable record carrying every input the operation needs, implementing `ICommand<Result<{Command}Result>>` (or `ICommand<Result>`).
- Co-locate the `{Command}Result` record in the same file.

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `record`, immutable, structural equality. Properties are primitives or `Soft{ValueObject}` — never a domain entity or strict `{ValueObject}`.
- `Result<T>` is written explicitly in the type argument, not implied by the marker.
- **Fixed command property order** (deterministic across VP combinations): business fields → `Guid` (VP6) → `ActionTimeStamp` (VP7) → version token (VP5). At plateau-core only business fields exist; no VP field claims a fixed absolute position.
- A command that stages a persisted-entity write implements `ICommand<TResponse>` (use `ICommand<Result>` for no payload); bare `ICommand` is only for a command with no persisted-entity effect at all.
- One handler per command; one per-feature `AbstractValidator<{Command}>` co-located with the handler.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Create | `Create{Entity}Command` | `CreateTaskCommand` | `{Command}.cs` | `CreateTaskCommand.cs` |
| Update | `Update{Entity}Command` | `UpdateTaskCommand` | `{Command}.cs` | `UpdateTaskCommand.cs` |
| Domain action | `{Verb}{Entity}Command` | `AssignTaskCommand` | `{Command}.cs` | `AssignTaskCommand.cs` |
| Result | `{Command}Result` | `CreateTaskResult` | same file | `CreateTaskCommand.cs` |

# Implementation
```csharp
// Skill: plateau-core--class-command
// Plateau: core
// Version: 20260902000000
using Ardalis.Result;
using Shared.MediatR;

namespace {Module}.Interfaces.Commands;

public record CreateTaskCommand(
    string Title,
    int AssigneeId
) : ICommand<Result<CreateTaskResult>>;

public record CreateTaskResult(int Id);
```
No payload beyond success/failure:
```csharp
public record AssignTaskCommand(int TaskId, int AssigneeId) : ICommand<Result>;
```

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Rules
MUST:
- Implement `ICommand<Result<T>>` or `ICommand<Result>` — never `IRequest<T>` directly.
- Declare it a `record` in `/{Module}.Interfaces/Commands`; put `{Command}Result` in the same file.
- Type every property as a primitive or `Soft{ValueObject}`; no methods, no logic, no domain-entity reference.
- Follow the fixed property order; no later solution reorders an earlier one's field or claims "first".
- Never apply several plateau templates per class.
- Never duplicate a rule already owned by a `{ValueObject}PropertyValidator` / `{Dto}Validator`.

# Check list
- [ ] `record {Command}(...) : ICommand<Result<{Command}Result>>` (or `: ICommand<Result>`) in `/Commands`.
- [ ] `{Command}Result` in the same file (when there is a payload).
- [ ] Properties are primitives / `Soft{ValueObject}`; business fields only at plateau-core.

# Unittest TestCases
- [ ] WHEN a `{Command}` is constructed THEN it is assignable to `ICommand<Result<{Command}Result>>`.
- [ ] WHEN two `{Command}` values with equal fields are compared THEN they are equal (record semantics).
