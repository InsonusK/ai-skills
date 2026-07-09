---
name: class-unit-of-work-behavior
description: Pipeline behavior that commits at depth 1 after handler completes
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]]"
---

# Goal
- Automatically commit all staged changes after the top-level command handler completes
- Prevent sub-commands from committing prematurely by checking `UnitOfWorkContext.Depth`
- Guarantee that if the handler throws, `SaveChangesAsync` is never called — changes are discarded

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create|UnitOfWorkBehavior.cs]]

# Core Principles
- Apply ONE plateau template per class
- Increments `UnitOfWorkContext.Depth` on entry, decrements in `finally` — depth always restored even on exception
- Calls `SaveChangesAsync` only when `Depth == 1` — the outermost command in the current request
- Sub-commands reach this behavior with `Depth > 1` — they stage changes but do not commit
- **No catch/rollback block** — EF Core uses implicit transactions. When `SaveChangesAsync` is not called (because handler threw), the DbContext is disposed at end of request scope and all pending changes are silently abandoned. No explicit rollback is necessary. If explicit transactions are introduced in the future, a catch/rollback block must be added at that point.
- `try/finally` ensures depth counter is always restored — no leaked depth on exception
- Constrained to `where TRequest : ICommand` — never activates for query requests

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create|UnitOfWorkBehavior.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| UoW pipeline behavior | `UnitOfWorkBehavior<TRequest, TResponse>` | `UnitOfWorkBehavior<AssignTaskCommand, Result>` | `UnitOfWorkBehavior.cs` | `UnitOfWorkBehavior.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create|UnitOfWorkBehavior.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-unit-of-work-behavior
//Plateau: default
//Version: 20260628
```

```csharp
// BuildingBlocks/MediatR/UnitOfWorkBehavior.cs
using MediatR;
using Shared.MediatR;
using Shared.UnitOfWork;

namespace BuildingBlocks.MediatR;

public class UnitOfWorkBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand
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
        _context.Depth++;
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
            _context.Depth--;
        }
    }
}
```

## Nesting depth flow
```
HTTP Request arrives
    ↓
UnitOfWorkBehavior: Depth++ → Depth = 1   (root command)
    ↓
Handler dispatches sub-command via _mediator.Send()
    ↓
UnitOfWorkBehavior: Depth++ → Depth = 2   (sub-command)
    ↓
Sub-command handler completes — stages changes
    ↓
UnitOfWorkBehavior: Depth == 2 → skip SaveChanges
UnitOfWorkBehavior: Depth-- → Depth = 1   (finally)
    ↓
Root handler continues — stages its own changes
    ↓
UnitOfWorkBehavior: Depth == 1 → call SaveChangesAsync  ← single atomic commit
UnitOfWorkBehavior: Depth-- → Depth = 0   (finally)

On handler exception:
    ↓
UnitOfWorkBehavior: Depth == 1 → SaveChangesAsync NOT called (response = await next() threw)
UnitOfWorkBehavior: Depth-- → Depth = 0   (finally)
DbContext disposed at request scope end → all pending changes abandoned automatically
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create|UnitOfWorkBehavior.cs]]

# Rules
MUST:
	- Constrained to `where TRequest : ICommand` — never activates on queries
	- Use `try/finally` to guarantee depth counter is always restored
	- Call `SaveChangesAsync` only when `Depth == 1`
	- Increment depth before `next()` — decrement in `finally`
MUST NOT:
	- Call `SaveChangesAsync` when `Depth > 1` — sub-commands must not commit
	- Add a catch/rollback block — EF implicit transactions do not require it; adding one without explicit transaction management would be incorrect
	- Catch exceptions to swallow them — let them propagate, `SaveChangesAsync` is skipped naturally

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create|UnitOfWorkBehavior.cs]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- `UnitOfWorkBehavior` without depth counter — sub-commands commit prematurely, breaking atomicity
- `catch { rollback }` without explicit `BeginTransaction` — false safety, EF Core already provides implicit transactions
- Catching exceptions to swallow them — breaks error propagation

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create|UnitOfWorkBehavior.cs]]

# Check list
- [ ] `UnitOfWorkBehavior` constrained to `where TRequest : ICommand`
- [ ] `try/finally` wraps the entire handler invocation
- [ ] `SaveChangesAsync` called only when `_context.Depth == 1`
- [ ] Depth incremented before `next()` and decremented in `finally`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create|UnitOfWorkBehavior.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Automatically commit all staged changes after the top-level command handler completes
- [ ] WHEN applied THEN Prevent sub-commands from committing prematurely by checking UnitOfWorkContext.Depth
- [ ] WHEN applied THEN Guarantee that if the handler throws, SaveChangesAsync is never called — changes are discarded
- [ ] WHEN applied THEN Increments UnitOfWorkContext.Depth on entry, decrements in finally — depth always restored even on exception
- [ ] WHEN applied THEN Calls SaveChangesAsync only when Depth == 1 — the outermost command in the current request
- [ ] WHEN applied THEN Sub-commands reach this behavior with Depth > 1 — they stage changes but do not commit
- [ ] WHEN applied THEN **No catch/rollback block** — EF Core uses implicit transactions. When SaveChangesAsync is not called (because handler threw), the DbContext is disposed at end of request scope and all pending changes are silently abandoned. No explicit rollback is necessary. If explicit transactions are introduced in the future, a catch/rollback block must be added at that point
- [ ] WHEN applied THEN try/finally ensures depth counter is always restored — no leaked depth on exception
- [ ] WHEN applied THEN Constrained to where TRequest : ICommand — never activates for query requests
- [ ] WHEN verified THEN UnitOfWorkBehavior constrained to where TRequest : ICommand
- [ ] WHEN verified THEN try/finally wraps the entire handler invocation
- [ ] WHEN verified THEN SaveChangesAsync called only when _context.Depth == 1
- [ ] WHEN verified THEN Depth incremented before next() and decremented in finally
- [ ] WHEN naming 'UoW pipeline behavior' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|class-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/BuildingBlocks.csproj.extend/UnitOfWorkBehavior.cs.create|UnitOfWorkBehavior.cs]]
