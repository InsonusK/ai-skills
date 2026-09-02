---
name: plateau-offline-sync-service--class-feature-handler
description: Class {FeatureName}Handler in the plateau-offline-sync-service plateau — a command or query handler in {Module}.Application/Features, fixed shape, no persistence
whenToUse: when creating or editing a command/query handler in {Module}.Application/Features, or deciding what belongs in a handler versus a validator or the domain
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
  - "[[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]]"
---

# Goal
- Handle one command (a write) or one query (a read): guard, do the work (dispatch / shape data), return `Result<T>`.
- Contain no business rules — delegate every decision to the entity's guarded method (which throws `DomainException` on an invariant violation); the handler only loads, calls, stages, and shapes the result.

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- Implements `IRequestHandler<TRequest, Result<T>>`.
- Fixed shape for a persisted command: `guard → load (named spec + IRepository<T>) → domain call (entity's guarded method) → stage (Add/UpdateAsync) → return Result<T>`. A command with no persisted-entity effect keeps the shorter `guard → dispatch → return` shape.
- Never `DbContext`, never inline LINQ, never `SaveChangesAsync`.
- Cross-module interaction is `ISender.Send` / `IPublisher.Publish` against another module's `Interfaces` — never a direct call.
- Result status: `Created` for a new entity, `Success` for update/read, `NotFound` after a failed load-guard, `Conflict` for a failed cross-request precondition, `Error` for a failed sub-request.

# Naming convention
| use case | class name | file name |
| --- | --- | --- |
| Command / query handler | `{FeatureName}Handler` (e.g. `CreateTaskHandler`, `GetTaskByIdHandler`) | `{FeatureName}.Handler.cs` |

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-feature-handler
// Plateau: core
// Version: 20260902000000
using Ardalis.Result;
using MediatR;

namespace {Module}.Application.Features.SubmitReport;

public sealed class Rename{Entity}Handler(IRepository<{Entity}> repository)
    : IRequestHandler<Rename{Entity}Command, Result>
{
    public async Task<Result> Handle(Rename{Entity}Command request, CancellationToken ct)
    {
        var entity = await repository.FirstOrDefaultAsync(new {Entity}ByIdSpec(request.Id), ct);
        if (entity is null)
            return Result.NotFound();                        // guard after load

        entity.Rename(new {ValueObject}(request.NewTitle.Value)); // domain guard: throws DomainException on violation
        entity.RecordUpdatedByUser(request.ActionTimeStamp);      // VP7: copy the user timestamp
        await repository.UpdateAsync(entity, ct);                 // stage; UnitOfWorkBehavior commits

        return Result.Success();
    }
}
// Concurrency was already checked by ConcurrencyBehavior before this handler ran.
```
Query handler (no persistence):
```csharp
public sealed class GetReviewStatusHandler(ISender sender)
    : IRequestHandler<GetReviewStatus, Result<ReviewStatusDto>>
{
    public Task<Result<ReviewStatusDto>> Handle(GetReviewStatus q, CancellationToken ct)
        => sender.Send(new FetchStatusFrom(q.ReportId), ct);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]

# Rules
MUST:
- Implement `IRequestHandler<TRequest, Result<T>>`; follow `guard → (dispatch | read | shape) → return`.
- For a persisted command: load via a named spec + `IRepository<T>`, guard the null, call the entity's guarded method, copy the VP7 `ActionTimeStamp` to the entity, stage via `Add/UpdateAsync` — never inline LINQ, never `SaveChangesAsync`.
- Never inject `DbContext`, never write inline LINQ, never call `SaveChangesAsync`.
- Never contain a business rule; dispatch cross-module only via `ISender`/`IPublisher`.
- Live in `/Features/{FeatureName}/{FeatureName}.Handler.cs`, class `{FeatureName}Handler`.
- Never apply several plateau templates per class.

# Check list
- [ ] `{FeatureName}Handler : IRequestHandler<{Request}, Result<T>>` in `/Features/{FeatureName}`.
- [ ] Shape is `guard → (dispatch | read | shape) → return Result<T>`.
- [ ] No `IRepository<T>` / `DbContext` / inline LINQ / `SaveChangesAsync`.
- [ ] No business rule; cross-module via `ISender`/`IPublisher` only.

# Unittest TestCases
- [ ] WHEN the command is valid THEN the handler returns `Success`/`Created` with the expected payload and publishes its fact.
- [ ] WHEN a dispatched sub-request fails THEN the handler returns `Error` and publishes nothing.
- [ ] WHEN a query's target is absent THEN the handler returns `NotFound`.
