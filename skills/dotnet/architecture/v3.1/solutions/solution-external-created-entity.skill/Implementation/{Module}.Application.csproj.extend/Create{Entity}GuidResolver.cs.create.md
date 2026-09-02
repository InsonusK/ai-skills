---
description: Per-entity IGuidResolver implementation
project_name: "{Module}.Application"
name: "Create{Entity}GuidResolver.cs"
element_kind: class
change_kind: create
tags:
  - solution/external-created-entity
  - element/create-entity-guidresolver-cs
---

# Goals
- Implement `IGuidResolver<TResponse>` for one specific external-created entity type
- Look up the entity by Guid using `IReadRepository<T>` and `{Entity}ByGuidSpec`
- Return a conflict result with the existing entity Id if found, null if not found

# Core Principles
- Implements `IGuidResolver<Result<Create{Entity}Result>>` from Shared
- Injects `IReadRepository<T>` from Shared — read-only lookup
- Uses `{Entity}ByGuidSpec` from Application — no inline LINQ
- Maps found entity to the command result type — same shape the handler would return on success
- Returns `ConflictResult<Create{Entity}Result>` when entity exists — same response type as handler, but marked as conflict
- Returns null when not found — `GuidResolvingBehavior` proceeds to handler on null

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid resolver implementation | `Create{Entity}GuidResolver` | `CreateTaskGuidResolver` | `Create{Entity}GuidResolver.cs` | `CreateTaskGuidResolver.cs` |

# Implementation changes

```csharp
// {Module}.Application/Resolvers/Create{Entity}GuidResolver.cs
using Ardalis.Result;
using Shared.Guid;
using Shared.Results;

public class Create{Entity}GuidResolver
    : IGuidResolver<Result<Create{Entity}Result>>
{
    private readonly IReadRepository<{Entity}> _repository;

    public Create{Entity}GuidResolver(IReadRepository<{Entity}> repository)
        => _repository = repository;

    public async Task<Result<Create{Entity}Result>?> ResolveAsync(
        Guid guid, CancellationToken ct)
    {
        var entity = await _repository.FirstOrDefaultAsync(
            new {Entity}ByGuidSpec(guid), ct);

        // null — Guid not found, first request, handler should run
        if (entity is null)
            return null;

        // non-null — Guid already exists, return conflict result with same shape as handler success
        // GuidResolvingBehavior will return this result directly
        return new ConflictResult<Create{Entity}Result>(
            new Create{Entity}Result(entity.Id));
    }
}
```

# Rule changes

## MUST
- Return null when entity not found — never throw
- Return `ConflictResult<Create{Entity}Result>` when entity found — same type as handler response
- Inject `IReadRepository<T>` — never `IRepository<T>` or DbContext
- Use `{Entity}ByGuidSpec` — never inline LINQ
- One `Create{Entity}GuidResolver` per external-created entity type in `/{Module}.Application/Resolvers`
- Never throw exceptions — null signals not found, non-null signals exists
- Never return a different response type than the command handler
- Never return `Result.NotFound()` — null is the "not found" signal in this contract
- Never resolver throw exceptions — null means not found, non-null means exists

## SHOULD
- Avoid inline LINQ in resolver instead of named spec
- Avoid returning `Result.Success(...)` instead of `ConflictResult<...>` — would make the API return 200/201 for a duplicate
- Avoid resolver returning a response type different from the command handler

# Check list
- [ ] Returns null when entity not found
- [ ] Returns `ConflictResult<Create{Entity}Result>` when entity found
- [ ] Uses `IReadRepository<T>` and `{Entity}ByGuidSpec`
- [ ] Response type matches command handler response type

# Unittest TestCases
- [ ] WHEN inspected THEN it implement IGuidResolver<TResponse> for one specific external-created entity type
- [ ] WHEN applied THEN Look up the entity by Guid using IReadRepository<T> and {Entity}ByGuidSpec
- [ ] WHEN applied THEN Return the existing conflict result if found, null if not found
- [ ] WHEN applied THEN Implements IGuidResolver<Result<Create{Entity}Result>> from Shared
- [ ] WHEN applied THEN Injects IReadRepository<T> from Shared — read-only lookup
- [ ] WHEN applied THEN Uses {Entity}ByGuidSpec from Application — no inline LINQ
- [ ] WHEN applied THEN Maps found entity to the command result type — same shape as handler success
- [ ] WHEN applied THEN Returns null when not found — GuidResolvingBehavior proceeds to handler on null
- [ ] WHEN verified THEN Returns null when entity not found
- [ ] WHEN verified THEN Returns ConflictResult<Create{Entity}Result> when entity found
- [ ] WHEN verified THEN Uses IReadRepository<T> and {Entity}ByGuidSpec
- [ ] WHEN verified THEN Response type matches command handler response type
- [ ] WHEN naming 'Guid resolver implementation' THEN pattern matches convention
