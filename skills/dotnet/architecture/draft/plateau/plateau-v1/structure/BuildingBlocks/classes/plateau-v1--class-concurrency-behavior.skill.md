---
name: class-concurrency-behavior
description: Classes ConcurrencyBehavior/ETagEncoder in the v1 plateau
whenToUse: when reviewing or changing how a mutable entity's version is validated before its handler runs
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
---

# Goal
- Validate every `IHasVersions` command's expected versions against the current stored versions before the handler runs, returning `Result.Conflict` on any mismatch
- Encode/decode entity versions as base64 JSON — transport-specific, used only once an HTTP API layer attaches it to `ETag`/`If-Match`

# Core Principles
- `ConcurrencyBehavior` is transport-agnostic — it activates on any `IHasVersions` command regardless of dispatch origin; see [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md#boundaries|solution-entity-concurrency-change's Boundaries]]
- Constrained to `where TRequest : IHasVersions` — only update/patch commands are checked
- `ETagEncoder` has no callers yet in this plateau — it is created here, wired to HTTP once `solution-http-api-publication` composes on top

# Implementation
```csharp
//Skill: class-concurrency-behavior
//Plateau: v1
//Version: 20260825140000

public class ConcurrencyBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IHasVersions, IRequest<TResponse>
    where TResponse : IResult
{
    private readonly IEntityVersionResolverFactory _factory;

    public ConcurrencyBehavior(IEntityVersionResolverFactory factory) => _factory = factory;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        foreach (var (entityName, expected) in request.Versions)
        {
            if (!request.EntityIds.TryGetValue(entityName, out var entityId))
                throw new InvalidOperationException($"No entity id provided for '{entityName}'.");

            var resolver = _factory.GetFor(entityName);
            var current = await resolver.GetCurrentVersionForAsync(entityId, ct);

            if (current != expected)
                return (TResponse)Result.Conflict($"{entityName} was modified by another request.");
        }

        return await next();
    }
}

public static class ETagEncoder
{
    public static string Encode(IReadOnlyDictionary<string, uint> versions)
        => Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(versions));

    public static IReadOnlyDictionary<string, uint>? TryDecode(string etag)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, uint>>(Convert.FromBase64String(etag));
        }
        catch
        {
            return null;
        }
    }
}
```

# Rules
MUST:
- `ConcurrencyBehavior` registered right after `ValidationBehavior` in `PipelineRegistration.AddPipeline()`
- Return `Result.Conflict` on any version mismatch — handler never runs for stale updates
MUST NOT:
- `ConcurrencyBehavior` catch `DbUpdateConcurrencyException` — that is EF's own final guard, this behavior is the early client-friendly check
- `ETagEncoder` be called from anywhere before an HTTP API layer exists

# Check list
- [ ] `ConcurrencyBehavior` registered after `ValidationBehavior` in `AddPipeline()`
- [ ] Any version mismatch returns `Result.Conflict`, handler never runs
- [ ] `ETagEncoder` exists but is unused until an HTTP API layer wires it up

__Applied solutions:__
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/BuildingBlocks.csproj.extend/ConcurrencyBehavior.cs.create.md|ConcurrencyBehavior.cs.create]], [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/BuildingBlocks.csproj.extend/ETagEncoder.cs.create.md|ETagEncoder.cs.create]]
