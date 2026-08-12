---
name: class-create-entity-guid-resolver
description: Per-entity IGuidResolver implementation
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
  - stack/dotnet
  - concern/architecture

created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]]"
---

# Goal
- Implement `IGuidResolver<TResponse>` for one specific external-created entity type
- Look up the entity by Guid using `IReadRepository<T>` and `{Entity}ByGuidSpec`
- Return a conflict result with the existing entity Id if found, null if not found

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/Create{Entity}GuidResolver.cs.create|Create{Entity}GuidResolver.cs]]

# Core Principles
- Apply ONE plateau template per class
- Implements `IGuidResolver<Result<Create{Entity}Result>>` from Shared
- Injects `IReadRepository<T>` from Shared — read-only lookup
- Uses `{Entity}ByGuidSpec` from Application — no inline LINQ
- Maps found entity to the command result type — same shape the handler would return on success
- Returns `ConflictResult<Create{Entity}Result>` when entity exists — same response type as handler, but marked as conflict
- Returns null when not found — `GuidResolvingBehavior` proceeds to handler on null

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/Create{Entity}GuidResolver.cs.create|Create{Entity}GuidResolver.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Guid resolver implementation | `Create{Entity}GuidResolver` | `CreateTaskGuidResolver` | `Create{Entity}GuidResolver.cs` | `CreateTaskGuidResolver.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/Create{Entity}GuidResolver.cs.create|Create{Entity}GuidResolver.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-create-entity-guid-resolver
//Plateau: default
//Version: 20260628
```

```csharp
// {Module}.Application/Resolvers/Create{Entity}GuidResolver.cs
using Ardalis.Result;
using Shared.Guid;
using Shared.Results;

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

        // non-null — Guid already exists, return conflict result with same shape as handler success
        // GuidResolvingBehavior will return this result directly
        return new ConflictResult<Create{Entity}Result>(
            new Create{Entity}Result(entity.Id));
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/Create{Entity}GuidResolver.cs.create|Create{Entity}GuidResolver.cs]]

# Rules
MUST:
	- Return null when entity not found — never throw
	- Return `ConflictResult<Create{Entity}Result>` when entity found — same type as handler response
	- Inject `IReadRepository<T>` — never `IRepository<T>` or DbContext
	- Use `{Entity}ByGuidSpec` — never inline LINQ
MUST NOT:
	- Throw exceptions — null signals not found, non-null signals exists
	- Return a different response type than the command handler
	- Return `Result.NotFound()` — null is the "not found" signal in this contract

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/Create{Entity}GuidResolver.cs.create|Create{Entity}GuidResolver.cs]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- Inline LINQ in resolver instead of named spec
- Returning `Result.Success(...)` instead of `ConflictResult<...>` — would make the API return 200/201 for a duplicate
- Resolver returning a response type different from the command handler

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/Create{Entity}GuidResolver.cs.create|Create{Entity}GuidResolver.cs]]

# Check list
- [ ] Returns null when entity not found
- [ ] Returns `ConflictResult<Create{Entity}Result>` when entity found
- [ ] Uses `IReadRepository<T>` and `{Entity}ByGuidSpec`
- [ ] Response type matches command handler response type

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/Create{Entity}GuidResolver.cs.create|Create{Entity}GuidResolver.cs]]

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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/solution-external-created-entity.skill|solution-external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/Create{Entity}GuidResolver.cs.create|Create{Entity}GuidResolver.cs]]
