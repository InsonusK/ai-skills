---
description: Pipeline behavior that commits at depth 1 after handler completes
project_name: BuildingBlocks
name: UnitOfWorkBehavior.cs
element_kind: class
change_kind: create
tags:
  - solution/unit-of-work
  - element/unitofworkbehavior-cs
---

# Goals
- Automatically commit all staged changes after the top-level command handler completes
- Prevent sub-commands from committing prematurely by checking `UnitOfWorkContext.Depth`
- Guarantee that if the handler throws, `SaveChangesAsync` is never called — changes are discarded

# Core Principles
- Calls `UnitOfWorkContext.Enter()` on entry and `UnitOfWorkContext.Leave()` in `finally` — depth always restored even on exception
- Sub-commands reach this behavior with `Depth > 1` — they stage changes but do not commit

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| UoW pipeline behavior | `UnitOfWorkBehavior<TRequest, TResponse>` | `UnitOfWorkBehavior<AssignTaskCommand, Result>` | `UnitOfWorkBehavior.cs` | `UnitOfWorkBehavior.cs` |

# Implementation changes

```csharp
// BuildingBlocks/MediatR/UnitOfWorkBehavior.cs
using MediatR;
using Shared;
using Shared.UnitOfWork;

namespace BuildingBlocks.MediatR;

public class UnitOfWorkBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UnitOfWorkContext _context;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork, UnitOfWorkContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        _context.Enter();
        try
        {
            var response = await next();

            // only the outermost command commits
            if (_context.Depth == 1)
                await _unitOfWork.SaveChangesAsync(ct);

            return response;
        }
        finally
        {
            // always restore depth — even on exception
            _context.Leave();
        }
    }
}
```

## Nesting depth flow
```
HTTP Request arrives
    ↓
UnitOfWorkBehavior: Enter() → Depth = 1   (root command)
    ↓
Handler dispatches sub-command via _mediator.Send()
    ↓
UnitOfWorkBehavior: Enter() → Depth = 2   (sub-command)
    ↓
Sub-command handler completes — stages changes
    ↓
UnitOfWorkBehavior: Depth == 2 → skip SaveChanges
UnitOfWorkBehavior: Leave() → Depth = 1   (finally)
    ↓
Root handler continues — stages its own changes
    ↓
UnitOfWorkBehavior: Depth == 1 → call SaveChangesAsync  ← single atomic commit
UnitOfWorkBehavior: Leave() → Depth = 0   (finally)

On handler exception:
    ↓
UnitOfWorkBehavior: Depth == 1 → SaveChangesAsync NOT called (response = await next() threw)
UnitOfWorkBehavior: Leave() → Depth = 0   (finally)
DbContext disposed at request scope end → all pending changes abandoned automatically
```

# Rule changes

## MUST
- Constrained to `where TRequest : ICommand<TResponse>` — never activates on queries
- Use `try/finally` to guarantee depth counter is always restored
- Call `SaveChangesAsync` only when `Depth == 1`
- Call `_context.Enter()` before `next()` — call `_context.Leave()` in `finally`
- Sub-commands safe to dispatch from handlers — depth counter prevents premature commit
- Never call `SaveChangesAsync` when `Depth > 1` — sub-commands must not commit
- Never add a catch/rollback block — EF implicit transactions do not require it; adding one without explicit transaction management would be incorrect
- Never catch exceptions to swallow them — let them propagate, `SaveChangesAsync` is skipped naturally

## SHOULD
- Avoid `UnitOfWorkBehavior` without depth counter — sub-commands commit prematurely, breaking atomicity
- Avoid `catch { rollback }` without explicit `BeginTransaction` — false safety, EF Core already provides implicit transactions
- Avoid catching exceptions to swallow them — breaks error propagation

# Check list
- [ ] `UnitOfWorkBehavior` constrained to `where TRequest : ICommand<TResponse>`
- [ ] `try/finally` wraps the entire handler invocation
- [ ] `SaveChangesAsync` called only when `_context.Depth == 1`
- [ ] `_context.Enter()` called before `next()` and `_context.Leave()` called in `finally`

# Unittest TestCases
- [ ] WHEN applied THEN Automatically commit all staged changes after the top-level command handler completes
- [ ] WHEN applied THEN Prevent sub-commands from committing prematurely by checking UnitOfWorkContext.Depth
- [ ] WHEN applied THEN Guarantee that if the handler throws, SaveChangesAsync is never called — changes are discarded
- [ ] WHEN applied THEN Calls UnitOfWorkContext.Enter() on entry and UnitOfWorkContext.Leave() in finally — depth always restored even on exception
- [ ] WHEN applied THEN Calls SaveChangesAsync only when Depth == 1 — the outermost command in the current request
- [ ] WHEN applied THEN Sub-commands reach this behavior with Depth > 1 — they stage changes but do not commit
- [ ] WHEN applied THEN **No catch/rollback block** — EF Core uses implicit transactions. When SaveChangesAsync is not called (because handler threw), the DbContext is disposed at end of request scope and all pending changes are silently abandoned. No explicit rollback is necessary. If explicit transactions are introduced in the future, a catch/rollback block must be added at that point
- [ ] WHEN applied THEN try/finally ensures depth counter is always restored — no leaked depth on exception
- [ ] WHEN applied THEN Constrained to where TRequest : ICommand<TResponse> — never activates for query requests
- [ ] WHEN verified THEN UnitOfWorkBehavior constrained to where TRequest : ICommand<TResponse>
- [ ] WHEN verified THEN try/finally wraps the entire handler invocation
- [ ] WHEN verified THEN SaveChangesAsync called only when _context.Depth == 1
- [ ] WHEN verified THEN _context.Enter() called before next() and _context.Leave() called in finally
- [ ] WHEN naming 'UoW pipeline behavior' THEN pattern matches convention
