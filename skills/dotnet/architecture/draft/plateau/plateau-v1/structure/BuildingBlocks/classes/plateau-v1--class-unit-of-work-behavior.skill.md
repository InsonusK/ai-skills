---
name: class-unit-of-work-behavior
description: Classes UnitOfWorkContext/UnitOfWorkBehavior in the v1 plateau
whenToUse: when reviewing when staged entity changes actually get committed, especially across nested command dispatch
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]"
---

# Goal
- Commit all staged changes exactly once per top-level command, even when that command dispatches sub-commands via `_mediator.Send()`

# Core Principles
- `UnitOfWorkContext` tracks nesting depth — only the outermost command (`Depth == 1`) commits
- `UnitOfWorkBehavior` activates only on `ICommand<TResponse>` (the standard command shape every command in this plateau implements) — queries never trigger a commit
- No explicit rollback: if the handler throws, `SaveChangesAsync` is never called, and EF's implicit transaction discards staged changes when the `DbContext` is disposed at request scope end

# Implementation
```csharp
//Skill: class-unit-of-work-behavior
//Plateau: v1
//Version: 20260825140000

public sealed class UnitOfWorkContext
{
    public int Depth { get; private set; }
    public void Enter() => Depth++;
    public void Leave() => Depth--;
}

public class UnitOfWorkBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly UnitOfWorkContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkBehavior(UnitOfWorkContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        _context.Enter();
        try
        {
            var response = await next();

            if (_context.Depth == 1)
                await _unitOfWork.SaveChangesAsync(ct);

            return response;
        }
        finally
        {
            _context.Leave();
        }
    }
}
```

# Rules
MUST:
- `UnitOfWorkContext` registered `Scoped` — one instance per request, shared across nested dispatches
- `UnitOfWorkBehavior` use `try/finally` with `Leave()` for the depth decrement
- Commit only when `Depth == 1`
MUST NOT:
- Register `UnitOfWorkContext` as `Singleton` (leaks depth across requests) or `Transient` (depth never reaches 1)
- Call `SaveChangesAsync` anywhere else

# Check list
- [ ] `UnitOfWorkContext`/`UnitOfWorkBehavior` live in `BuildingBlocks/MediatR`
- [ ] Commit happens only at `Depth == 1`, inside `try/finally`
- [ ] No `SaveChangesAsync` call anywhere outside `UnitOfWorkBehavior`

__Applied solutions:__
- [[../../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] - [[../../../../../solutions/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkContext.cs.create.md|UnitOfWorkContext.cs.create]], [[../../../../../solutions/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create.md|UnitOfWorkBehavior.cs.create]]
