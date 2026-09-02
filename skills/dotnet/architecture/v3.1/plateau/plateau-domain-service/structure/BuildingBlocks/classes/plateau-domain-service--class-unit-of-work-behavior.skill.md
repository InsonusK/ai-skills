---
name: plateau-domain-service--class-unit-of-work-behavior
description: Class UnitOfWorkBehavior<TRequest, TResponse> in the plateau-domain-service plateau — the last pipeline behavior; commits all staged changes once, after the outermost command handler completes
whenToUse: when editing the unit-of-work pipeline behavior, or checking the atomic-commit / no-rollback semantics
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]"
---

# Goal
- Commit all changes staged by a command and its sub-commands in a single `SaveChangesAsync`, after the outermost handler completes; if the handler throws, never commit — the request scope disposes the tracked changes.

__Applied solutions:__
- [[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] - [[../../../../../solutions/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create.md|UnitOfWorkBehavior.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `sealed`, in `BuildingBlocks/MediatR`, constrained `where TRequest : ICommand<TResponse>` — never activates on a query or a bare `ICommand`.
- `_context.Enter()` before `next()`, `_context.Leave()` in `finally` — depth always restored, even on exception.
- Calls `IUnitOfWork.SaveChangesAsync` only when `_context.Depth == 1`; sub-commands (`Depth > 1`) stage but do not commit.
- **No catch/rollback block** — EF Core implicit transactions cover it; a throw simply skips the commit.
- Registered last in `PipelineRegistration.AddPipeline()`, after every rejecting behavior.

# Implementation
```csharp
// Skill: plateau-domain-service--class-unit-of-work-behavior
// Plateau: domain-service
// Version: 20260902000000
using MediatR;
using Shared.MediatR;
using Shared.UnitOfWork;

namespace BuildingBlocks.MediatR;

public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork, UnitOfWorkContext context)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        context.Enter();
        try
        {
            var response = await next();
            if (context.Depth == 1)
                await unitOfWork.SaveChangesAsync(ct);
            return response;
        }
        finally
        {
            context.Leave();
        }
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] - [[../../../../../solutions/solution-unit-of-work.skill/Implementation/App.Host.csproj.extend/PipelineRegistration.cs.extend.md|PipelineRegistration.cs.extend]]

# Rules
MUST:
- Constrain `where TRequest : ICommand<TResponse>`; wrap `next()` in `try/finally` with `_context.Leave()` in `finally`.
- Call `SaveChangesAsync` only at `_context.Depth == 1`; call `_context.Enter()` before `next()`.
- Be registered last in `AddPipeline()`.
- Never add a catch/rollback block; never swallow an exception; never activate on a query.
- Never apply several plateau templates per class.

# Check list
- [ ] `where TRequest : ICommand<TResponse>`; `try/finally` with `Leave()` in `finally`.
- [ ] `SaveChangesAsync` only at `Depth == 1`; no catch/rollback.
- [ ] Registered last in `AddPipeline()`.

# Unittest TestCases
- [ ] WHEN a root command succeeds THEN `SaveChangesAsync` is called once.
- [ ] WHEN a nested sub-command runs THEN it does not commit; the root commits both.
- [ ] WHEN the handler throws THEN `SaveChangesAsync` is not called and depth is restored.
