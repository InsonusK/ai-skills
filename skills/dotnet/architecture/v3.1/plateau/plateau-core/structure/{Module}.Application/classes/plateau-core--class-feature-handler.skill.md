---
name: plateau-core--class-feature-handler
description: Class {FeatureName}Handler in the plateau-core plateau — a command or query handler in {Module}.Application/Features, fixed shape, no persistence
whenToUse: when creating or editing a command/query handler in {Module}.Application/Features, or deciding what belongs in a handler versus a validator or the domain
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
- Handle one command (a write) or one query (a read): guard, do the work (dispatch / shape data), return `Result<T>`.
- Contain no business rules — at plateau-core there is no domain layer, so a handler only shapes data and dispatches; cross-request facts are checked as guards.

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- Implements `IRequestHandler<TRequest, Result<T>>`.
- Fixed shape: `guard → (dispatch | read | shape) → return Result<T>`. `load`/`stage` via `IRepository<T>` appear only once VP2 is applied.
- Never `DbContext`, never inline LINQ, never `SaveChangesAsync`.
- Cross-module interaction is `ISender.Send` / `IPublisher.Publish` against another module's `Interfaces` — never a direct call.
- Result status: `Created` for a new entity, `Success` for update/read, `NotFound` after a failed load-guard, `Conflict` for a failed cross-request precondition, `Error` for a failed sub-request.

# Naming convention
| use case | class name | file name |
| --- | --- | --- |
| Command / query handler | `{FeatureName}Handler` (e.g. `CreateTaskHandler`, `GetTaskByIdHandler`) | `{FeatureName}.Handler.cs` |

# Implementation
```csharp
// Skill: plateau-core--class-feature-handler
// Plateau: core
// Version: 20260902000000
using Ardalis.Result;
using MediatR;

namespace {Module}.Application.Features.SubmitReport;

public sealed class SubmitReportHandler(ISender sender, IPublisher publisher)
    : IRequestHandler<SubmitReportCommand, Result<SubmitReportResult>>
{
    public async Task<Result<SubmitReportResult>> Handle(SubmitReportCommand request, CancellationToken ct)
    {
        var ack = await sender.Send(new NotifyReviewersCommand(request.ReportId), ct);
        if (!ack.IsSuccess)
            return Result.Error("Reviewers could not be notified.");

        await publisher.Publish(new ReportSubmitted(request.ReportId, DateTimeOffset.UtcNow), ct);
        return Result.Success(new SubmitReportResult(request.ReportId));
    }
}
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
- Add `load`/`stage` (via `IRepository<T>`/`IReadRepository<T>` + named specs) only when the request touches stored state (VP2).
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
