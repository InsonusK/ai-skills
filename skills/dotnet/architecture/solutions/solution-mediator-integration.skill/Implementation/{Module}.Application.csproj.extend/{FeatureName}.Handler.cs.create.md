---
description: Command or query handler — fixed shape, persistence steps added only when stored state is involved
project_name: "{Module}.Application"
name: "{FeatureName}.Handler.cs"
element_kind: class
change_kind: create
tags:
  - solution/mediator-integration
  - element/featurename-handler-cs
---

# Goals
- Handle one Command (a write) or one Query (a read): guard, do the work, return `Result<T>`.
- Contain no business rules — a Command with a domain layer delegates every decision to the entity / domain service; a Command without one only shapes data and dispatches.

# Core Principles
- Implements `IRequestHandler<TRequest, Result<T>>`.
- **Fixed shape:** `guard → (domain call | read) → return Result<T>`.
- **Persistence steps are conditional.** `load`/`stage` through `IRepository<T>`/`IReadRepository<T>` are added *only* when the request touches stored state — i.e. once `solution-repository-integration` (VP2) is applied. A handler with no persistence and no domain layer skips them entirely and is still a complete, valid handler.
- Never `DbContext`, never inline LINQ, never `SaveChangesAsync` (commit is the unit-of-work behavior's job, once persistence exists).

# Naming convention
| use case | class name | file name |
| --- | --- | --- |
| Command / Query handler | `{FeatureName}Handler` (e.g. `CreateTaskHandler`, `GetTaskByIdHandler`) | `{FeatureName}.Handler.cs` |

# Implementation changes

## Baseline handler — no persistence, no domain layer

Shapes input, dispatches, returns a result. This is the shape every module has from `solution-mediator-integration` alone.

```csharp
// {Module}.Application/Features/SubmitReport/SubmitReport.Handler.cs
using Ardalis.Result;
using MediatR;

namespace {Module}.Application.Features.SubmitReport;

public class SubmitReportHandler(ISender sender) : IRequestHandler<SubmitReport, Result<SubmitReportResult>>
{
    public async Task<Result<SubmitReportResult>> Handle(SubmitReport request, CancellationToken ct)
    {
        // guard: transport-shape already checked by the validator; guard here on cross-request facts only
        var ack = await sender.Send(new NotifyReviewers(request.ReportId), ct);
        if (!ack.IsSuccess)
            return Result.Error("Reviewers could not be notified.");

        return Result.Success(new SubmitReportResult(request.ReportId, DateTimeOffset.UtcNow));
    }
}
```

## Query handler — no persistence

Answers from another module (or, later, from a repository).

```csharp
public class GetReviewStatusHandler(ISender sender) : IRequestHandler<GetReviewStatus, Result<ReviewStatusDto>>
{
    public async Task<Result<ReviewStatusDto>> Handle(GetReviewStatus q, CancellationToken ct)
        => await sender.Send(new FetchStatusFrom(q.ReportId), ct);
}
```

## Command handler — with domain layer (VP1) and persistence (VP2)

Once `solution-domain-behaviour` and `solution-repository-integration` are applied, `load` / `stage` steps wrap the domain call:

```csharp
using Ardalis.Result;
using MediatR;
using Shared.Repositories;

public class CloseTaskHandler(IRepository<TodoTask> repository) : IRequestHandler<CloseTask, Result>
{
    public async Task<Result> Handle(CloseTask cmd, CancellationToken ct)
    {
        var task = await repository.FirstOrDefaultAsync(new TaskByIdSpec(cmd.TaskId), ct);
        if (task is null)
            return Result.NotFound();

        task.Close(cmd.Reason);            // domain method guards the invariant and throws DomainException on violation
        await repository.UpdateAsync(task, ct);   // stage; commit is UnitOfWorkBehavior's job

        return Result.Success();
    }
}
```

## Result status conventions
| Result | Use |
| --- | --- |
| `Result.Created(value)` | new entity created |
| `Result.Success()` / `Result.Success(value)` | updated / read succeeded |
| `Result.NotFound()` | required entity absent (guard after load) |
| `Result.Conflict(msg)` | a cross-request precondition fails (not an entity invariant — that throws) |
| `Result.Error(msg)` | a dispatched sub-request failed, or unrecoverable state |

# Rule changes

## MUST
- Implement `IRequestHandler<TRequest, Result<T>>` and follow `guard → (domain call | read) → return`.
  - Risk: an ad-hoc handler shape makes every handler read differently and hides where the decision is made.
  - Fix: the fixed sequence; the middle step is a domain call for a Command with a domain layer, a repository/dispatch read for a Query.
- Add `load`/`stage` (via `IRepository<T>`/`IReadRepository<T>`, named specs) **only** when the request touches stored state.
  - Risk: mandating a repository makes the solution unusable for a module with no persistence — the very case v3.1 makes common.
  - Fix: the baseline handler has no repository; VP2 adds those steps.
- Never inject `DbContext`, never write inline LINQ, never call `SaveChangesAsync`.
  - Risk: persistence details and premature commits leak into orchestration and break atomicity.
  - Fix: `DbContext`/commit are infrastructure concerns; queries go through named specs once VP2 exists.
- Never contain a business rule — delegate to the entity / domain service (when a domain layer exists) or model it as a guard on cross-request facts.
  - Risk: logic in the handler cannot be found by a reader of the entity and is not covered by the domain tests.
  - Fix: `entity.DoThing()`; the entity throws `DomainException` on violation.
- Dispatch cross-module interaction via `ISender.Send` / `IPublisher.Publish` — never a direct call, never another module's `Domain`/`Application` type.
  - Risk: a direct reference couples the modules and bypasses the pipeline.
  - Fix: send a request defined in the target module's `Interfaces`.

## SHOULD
- Guard checks return early, before the domain call / read — fail fast.
- Use the transport-validation boundary to decide validator vs handler vs domain.

# Check list
- [ ] `{FeatureName}Handler : IRequestHandler<{Request}, Result<T>>` in `Features/{FeatureName}/`.
- [ ] Shape is `guard → (domain call | read) → return Result<T>`.
- [ ] No repository / `IRepository<T>` unless the request touches stored state (VP2).
- [ ] No `DbContext`, no inline LINQ, no `SaveChangesAsync`.
- [ ] No business rule in the handler; cross-module via `ISender`/`IPublisher` only.
