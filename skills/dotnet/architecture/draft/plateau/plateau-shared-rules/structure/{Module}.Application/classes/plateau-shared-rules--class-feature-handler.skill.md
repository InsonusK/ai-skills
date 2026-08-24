---
name: class-feature-handler
description: Class {FeatureName}Handler in the shared-rules plateau
whenToUse: when implementing the handler for a new command
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
- Orchestrate one write operation: guard against business failures, delegate to domain, return a typed result
- Never contain business rules — always delegate decisions to the domain layer

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]

# Core Principles
- Implements `IRequestHandler<TCommand, Result<T>>`
- A handler that touches a persisted entity injects `IRepository<T>`/`IReadRepository<T>` (never `DbContext`) once `solution-repository-integration` is applied — a handler with no persisted state skips load/stage entirely
- All entity loading uses named specs — no inline LINQ
- Cross-module writes go through `_mediator.Send()` — never a direct method call on another module's class

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Command handler | {FeatureName}Handler | CreateTaskHandler | {FeatureName}.Handler.cs | CreateTask.Handler.cs |

# Implementation
```csharp
//Skill: class-feature-handler
//Plateau: shared-rules
//Version: 20260824150000

public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, Result<CreateTaskResult>>
{
    public async Task<Result<CreateTaskResult>> Handle(CreateTaskCommand command, CancellationToken ct)
    {
        // No persisted entity yet in this plateau — guard, call the domain, return.
        // Once solution-repository-integration is applied, load/stage go here via
        // IRepository<T>/IReadRepository<T>, still following guard -> domain call -> return.
        var task = new Order(); // domain call — construction/behavior enforces its own invariants

        return Result.Created(new CreateTaskResult(task.Id));
    }
}
```

## Result status conventions
| Result | Meaning |
| --- | --- |
| `Result.Created(value)` | Entity created successfully |
| `Result.Success()` / `Result.Success(value)` | Operation succeeded |
| `Result.NotFound()` | Required entity does not exist |
| `Result.Conflict(msg)` | Business state prevents the operation |
| `Result.Error(msg)` | Unexpected failure |

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]

# Rules
MUST:
- Implement `IRequestHandler<TCommand, Result<T>>`
- Never inject `DbContext` directly — inject `IRepository<T>`/`IReadRepository<T>` when the command touches a persisted entity
- Load entities via named specs — never inline LINQ
- Return `Result<T>` for all outcomes — never throw for flow control
- Dispatch cross-module writes via `_mediator.Send()` — never direct method calls
MUST NOT:
- Contain business logic or domain rules — delegate to entity or domain service
- Reference another module's Domain or Application projects directly
- Call `SaveChangesAsync` — Unit of Work owns commit

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]

# Check list
- [ ] Implements `IRequestHandler<TCommand, Result<T>>`
- [ ] No `DbContext` injected directly
- [ ] Returns `Result<T>` for every outcome, never throws for flow control
- [ ] Cross-module writes go through `_mediator.Send()`

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]
