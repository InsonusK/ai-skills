---
description: Pipeline behavior that short-circuits on duplicate Guid
name: GuidResolvingBehavior.cs
change_kind: create
---

# Goals
- Intercept any create command implementing `IHasGuid` and check whether the Guid already exists
- Short-circuit with `ConflictException<TResponse>` if the entity already exists — handler never runs for duplicate requests
- Pass through to the next behavior if Guid not found — first request proceeds normally

# Core Principles
- Constrained on `where TRequest : IHasGuid` — only activates for commands carrying a Guid; `IHasGuid` is defined in Shared
- Resolves `IGuidResolver<TResponse>` from DI — the resolver is specific to the command's result type; `IGuidResolver<TResult>` is defined in Shared
- Throws `ConflictException<TResponse>` on duplicate — never returns a result directly from the behavior
- The exception carries the full existing result — `ConflictExceptionMiddleware` extracts the entity body via `.GetValue()` for the 409 response
- Does not call `SaveChangesAsync` — purely a read and guard operation

# Pipeline position
```
ValidationBehavior         ← validation-behavior.solution.skill — rejects invalid input
    ↓
GuidResolvingBehavior      ← this solution — rejects duplicate Guid (create only)
    ↓
ConcurrencyBehavior        ← entity-concurrency-change.solution.skill — rejects stale versions (update only)
    ↓
UnitOfWorkBehavior         ← unit-of-work.solution.skill — commits after handler
    ↓
Handler
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid resolving behavior | `GuidResolvingBehavior<TRequest, TResponse>` | `GuidResolvingBehavior<CreateTaskCommand, Result<CreateTaskResult>>` | `GuidResolvingBehavior.cs` | `GuidResolvingBehavior.cs` |

# Implementation changes

```csharp
// BuildingBlocks/MediatR/GuidResolvingBehavior.cs
public class GuidResolvingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasGuid
{
    private readonly IGuidResolver<TResponse> _resolver;

    public GuidResolvingBehavior(IGuidResolver<TResponse> resolver)
        => _resolver = resolver;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var existing = await _resolver.ResolveAsync(request.Guid, ct);

        if (existing is not null)
            throw new ConflictException<TResponse>(existing);

        return await next();
    }
}
```

# Rules

MUST:
- Constrained to `where TRequest : IHasGuid`
- Consume `IHasGuid` and `IGuidResolver<TResult>` from Shared — BuildingBlocks does not define these contracts
- Throw `ConflictException<TResponse>` when resolver returns non-null — never return a result directly
- Pass through (`return await next()`) when resolver returns null

MUST NOT:
- Be registered as open generic — DI resolves per concrete `TRequest`/`TResponse` pair
- Call `SaveChangesAsync`
- Swallow the `ConflictException` — it must propagate to `ConflictExceptionMiddleware`

# Anti-patterns
- `GuidResolvingBehavior` registered after `UnitOfWorkBehavior` — duplicate commands open a unit of work unnecessarily
- `GuidResolvingBehavior` constrained on `IRequest<T>` instead of `IHasGuid` — would check all commands including queries

# Check list
- [ ] `GuidResolvingBehavior` constrained to `where TRequest : IHasGuid`
- [ ] Throws `ConflictException<TResponse>` on duplicate
- [ ] Passes through when resolver returns null
- [ ] Never calls `SaveChangesAsync`
