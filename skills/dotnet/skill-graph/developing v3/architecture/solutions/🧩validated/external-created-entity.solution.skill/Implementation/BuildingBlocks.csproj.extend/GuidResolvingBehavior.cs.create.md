---
description: Pipeline behavior that short-circuits on duplicate Guid
project_name: BuildingBlocks
name: GuidResolvingBehavior.cs
element_kind: class
change_kind: create
---

# Goals
- Intercept any create command implementing `IHasGuid` and check whether the Guid already exists
- Short-circuit with the resolver's conflict result if the entity already exists — handler never runs for duplicate requests
- Pass through to the next behavior if Guid not found — first request proceeds normally

# Core Principles
- Constrained on `where TRequest : IHasGuid` — only activates for commands carrying a Guid; `IHasGuid` is defined in Shared
- Resolves `IGuidResolver<TResponse>` from DI — the resolver is specific to the command's result type; `IGuidResolver<TResponse>` is defined in Shared
- Returns the resolver's conflict result on duplicate — never throws an exception
- The resolver returns the same response type as the command handler, so the behavior can pass it through unchanged
- Does not call `SaveChangesAsync` — purely a read and guard operation

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid resolving behavior | `GuidResolvingBehavior<TRequest, TResponse>` | `GuidResolvingBehavior<CreateTaskCommand, Result<CreateTaskResult>>` | `GuidResolvingBehavior.cs` | `GuidResolvingBehavior.cs` |

# Implementation changes

```csharp
// BuildingBlocks/MediatR/GuidResolvingBehavior.cs
using Ardalis.Result;
using Shared.Guid;
using Shared.Results;

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
            return existing;

        return await next();
    }
}
```

# Rules

MUST:
- Constrained to `where TRequest : IHasGuid`
- Consume `IHasGuid` and `IGuidResolver<TResponse>` from Shared — BuildingBlocks does not define these contracts
- Return the resolver's result when it returns non-null — never throw
- Pass through (`return await next()`) when resolver returns null

MUST NOT:
- Be registered as open generic — DI resolves per concrete `TRequest`/`TResponse` pair
- Call `SaveChangesAsync`
- Construct response DTOs or shape the API response

# Anti-patterns
- `GuidResolvingBehavior` constrained on `IRequest<T>` instead of `IHasGuid` — would check all commands including queries
- Throwing `ConflictException` from the behavior — breaks the command-integration principle of no exceptions for flow control
- Behavior constructing response DTOs instead of delegating to the resolver

# Check list
- [ ] `GuidResolvingBehavior` constrained to `where TRequest : IHasGuid`
- [ ] Returns resolver result on duplicate
- [ ] Passes through when resolver returns null
- [ ] Never calls `SaveChangesAsync`
- [ ] Never constructs response DTOs

# Unittest TestCases
- [ ] WHEN applied THEN Intercept any create command implementing IHasGuid and check whether the Guid already exists
- [ ] WHEN applied THEN Short-circuit with resolver's conflict result if the entity already exists — handler never runs for duplicate requests
- [ ] WHEN applied THEN Pass through to the next behavior if Guid not found — first request proceeds normally
- [ ] WHEN applied THEN Constrained on where TRequest : IHasGuid — only activates for commands carrying a Guid
- [ ] WHEN applied THEN Resolves IGuidResolver<TResponse> from DI — the resolver is specific to the command's result type
- [ ] WHEN applied THEN Returns resolver result on duplicate — never throws
- [ ] WHEN applied THEN Does not call SaveChangesAsync — purely a read and guard operation
- [ ] WHEN verified THEN GuidResolvingBehavior constrained to where TRequest : IHasGuid
- [ ] WHEN verified THEN Returns resolver result on duplicate
- [ ] WHEN verified THEN Passes through when resolver returns null
- [ ] WHEN verified THEN Never calls SaveChangesAsync
- [ ] WHEN naming 'Guid resolving behavior' THEN pattern matches convention
