---
name: plateau-v1--class-guid-resolver
description: Class Create{Entity}GuidResolver in the v1 plateau
whenToUse: when an idempotent create command needs to detect a duplicate client-generated Guid before the handler runs
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
---

# Goal
- Detect an existing entity by its client-generated `Guid`, returning the same response shape the create handler itself would return

# Core Principles
- Registered per concrete entity type via `IGuidResolver<Result<Create{Entity}Result>>` — never as an open generic
- Returns `null` when not found (handler proceeds), `ConflictResult<Create{Entity}Result>` when found (behavior short-circuits)

# Implementation
```csharp
//Skill: class-guid-resolver
//Plateau: v1
//Version: 20260825140000

public sealed class CreateTaskGuidResolver(IReadRepository<TodoTask> repository)
    : IGuidResolver<Result<CreateTaskResult>>
{
    public async Task<Result<CreateTaskResult>?> ResolveAsync(Guid guid, CancellationToken ct)
    {
        var existing = await repository.FirstOrDefaultAsync(new TaskByGuidSpec(guid), ct);
        return existing is null ? null : ConflictResult<CreateTaskResult>.For(new CreateTaskResult(existing.Id));
    }
}
```

# Rules
MUST:
- Live in `{Module}.Application/Resolvers`, one per external-created entity type
- Use `IReadRepository<T>` + `{Entity}ByGuidSpec` — no inline LINQ
- Registered in `{Module}ApplicationRegistration.cs` as `IGuidResolver<Result<Create{Entity}Result>>`, `Scoped`
MUST NOT:
- Return a response shape different from the command handler's own success response

# Check list
- [ ] `Create{Entity}GuidResolver` exists for every `IHasGuid` entity
- [ ] Returns `null` on not-found, `ConflictResult<T>` on found
- [ ] Registered per concrete type, `Scoped`, never as an open generic

__Applied solutions:__
- [[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[../../../../../solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/Create{Entity}GuidResolver.cs.create.md|Create{Entity}GuidResolver.cs.create]]
