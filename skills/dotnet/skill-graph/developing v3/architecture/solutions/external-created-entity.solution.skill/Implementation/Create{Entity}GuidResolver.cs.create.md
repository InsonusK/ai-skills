---
description: Per-entity IGuidResolver implementation
name: "Create{Entity}GuidResolver.cs"
change_kind: create
---

# Goals
- Implement `IGuidResolver<TResult>` for one specific external-created entity type
- Look up the entity by Guid using `IReadRepository<T>` and `{Entity}ByGuidSpec`
- Return the existing result if found, null if not found

# Core Principles
- Implements `IGuidResolver<Result<Create{Entity}Result>>`
- Injects `IReadRepository<T>` from Shared — read-only lookup
- Uses `{Entity}ByGuidSpec` from Application — no inline LINQ
- Maps found entity to the command result type — same shape the handler would return on success
- Returns null when not found — `GuidResolvingBehavior` proceeds to handler on null

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid resolver implementation | `Create{Entity}GuidResolver` | `CreateTaskGuidResolver` | `Create{Entity}GuidResolver.cs` | `CreateTaskGuidResolver.cs` |

# Implementation changes

```csharp
// {Module}.Application/Resolvers/Create{Entity}GuidResolver.cs
public class Create{Entity}GuidResolver
    : IGuidResolver<Result<Create{Entity}Result>>
{
    private readonly IReadRepository<{EntityName}> _repository;

    public Create{Entity}GuidResolver(IReadRepository<{EntityName}> repository)
        => _repository = repository;

    public async Task<Result<Create{Entity}Result>?> ResolveAsync(
        Guid guid, CancellationToken ct)
    {
        var entity = await _repository.FirstOrDefaultAsync(
            new {Entity}ByGuidSpec(guid), ct);

        // null — Guid not found, first request, handler should run
        if (entity is null)
            return null;

        // non-null — Guid already exists, return existing result
        // GuidResolvingBehavior will throw ConflictException with this value
        return Result.Success(new Create{Entity}Result(entity.Id));
    }
}
```

# Rules

MUST:
- Return null when entity not found — never throw
- Return `Result.Success(new Create{Entity}Result(...))` when entity found — same shape as handler success
- Inject `IReadRepository<T>` — never `IRepository<T>` or DbContext
- Use `{Entity}ByGuidSpec` — never inline LINQ

MUST NOT:
- Throw exceptions — null signals not found, non-null signals exists
- Return `Result.NotFound()` — null is the "not found" signal in this contract

# Anti-patterns
- Inline LINQ in resolver instead of named spec
- Returning `Result.NotFound()` instead of null

# Check list
- [ ] Returns null when entity not found
- [ ] Returns `Result.Success(...)` when entity found
- [ ] Uses `IReadRepository<T>` and `{Entity}ByGuidSpec`
