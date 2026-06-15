---
description: Pipeline behavior that short-circuits on duplicate Guid
project_name: BuildingBlocks
name: GuidResolvingBehavior.cs
element_kind: class
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
- `GuidResolvingBehavior` constrained on `IRequest<T>` instead of `IHasGuid` — would check all commands including queries

# Check list
- [ ] `GuidResolvingBehavior` constrained to `where TRequest : IHasGuid`
- [ ] Throws `ConflictException<TResponse>` on duplicate
- [ ] Passes through when resolver returns null
- [ ] Never calls `SaveChangesAsync`

# Unittest TestCases
- [ ] WHEN applied THEN Intercept any create command implementing IHasGuid and check whether the Guid already exists
- [ ] WHEN applied THEN Short-circuit with ConflictException<TResponse> if the entity already exists — handler never runs for duplicate requests
- [ ] WHEN applied THEN Pass through to the next behavior if Guid not found — first request proceeds normally
- [ ] WHEN applied THEN Constrained on where TRequest : IHasGuid — only activates for commands carrying a Guid; IHasGuid is defined in Shared
- [ ] WHEN applied THEN Resolves IGuidResolver<TResponse> from DI — the resolver is specific to the command's result type; IGuidResolver<TResult> is defined in Shared
- [ ] WHEN applied THEN Throws ConflictException<TResponse> on duplicate — never returns a result directly from the behavior
- [ ] WHEN applied THEN The exception carries the full existing result — ConflictExceptionMiddleware extracts the entity body via .GetValue() for the 409 response
- [ ] WHEN applied THEN Does not call SaveChangesAsync — purely a read and guard operation
- [ ] WHEN verified THEN GuidResolvingBehavior constrained to where TRequest : IHasGuid
- [ ] WHEN verified THEN Throws ConflictException<TResponse> on duplicate
- [ ] WHEN verified THEN Passes through when resolver returns null
- [ ] WHEN verified THEN Never calls SaveChangesAsync
- [ ] WHEN naming 'Guid resolving behavior' THEN pattern matches convention
