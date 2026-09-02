---
name: plateau-offline-sync-service--class-create-entity-guid-resolver
description: Class Create{Entity}GuidResolver in the plateau-offline-sync-service plateau — the per-entity IGuidResolver implementation that returns the existing entity's response as a ConflictResult on a duplicate Guid
whenToUse: when adding idempotent creation for a new external-created entity, or editing an existing resolver
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
---

# Goal
- Implement `IGuidResolver<Result<Create{Entity}Result>>` for one external-created entity: look up by Guid via `IReadRepository<{Entity}>` + `{Entity}ByGuidSpec`; return `null` if absent, `ConflictResult<Create{Entity}Result>(new(existing.Id))` if present.

__Applied solutions:__
- [[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[../../../../../solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/Create{Entity}GuidResolver.cs.create.md|Create{Entity}GuidResolver.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `sealed class Create{Entity}GuidResolver : IGuidResolver<Result<Create{Entity}Result>>` in `/{Module}.Application/Resolvers`.
- Injects `IReadRepository<{Entity}>` (never `IRepository<T>`, never `DbContext`); loads via `{Entity}ByGuidSpec` (no inline LINQ).
- Returns the **same response type** as the command handler, so `201` and `409` share the API contract; `Create{Entity}Result` carries only the Id.
- Returns `null` for not-found — never `Result.NotFound()`, never throws. Registered `Scoped` in the module registration.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-create-entity-guid-resolver
// Plateau: offline-sync-service
// Version: 20260902000000
using Ardalis.Result;
using {Module}.Application.Specifications;
using {Module}.Domain.Entities;
using {Module}.Interfaces.Commands;
using Shared.Guid;
using Shared.Repositories;
using Shared.Results;

namespace {Module}.Application.Resolvers;

public sealed class Create{Entity}GuidResolver(IReadRepository<{Entity}> repository)
    : IGuidResolver<Result<Create{Entity}Result>>
{
    public async Task<Result<Create{Entity}Result>?> ResolveAsync(System.Guid guid, CancellationToken ct)
    {
        var existing = await repository.FirstOrDefaultAsync(new {Entity}ByGuidSpec(guid), ct);
        return existing is null ? null : new ConflictResult<Create{Entity}Result>(new Create{Entity}Result(existing.Id));
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[../../../../../solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.extend.md|{Module}ApplicationRegistration.cs.extend]]

# Rules
MUST:
- Implement `IGuidResolver<Result<Create{Entity}Result>>` in `/{Module}.Application/Resolvers`; inject `IReadRepository<{Entity}>`; load via `{Entity}ByGuidSpec`.
- Return `null` for not-found; `ConflictResult<Create{Entity}Result>` (same shape as handler success) for found; never throw, never `Result.NotFound()`.
- Be registered `Scoped` as `IGuidResolver<Result<Create{Entity}Result>>` — never as an open generic.
- Never apply several plateau templates per class.

# Check list
- [ ] Implements `IGuidResolver<Result<Create{Entity}Result>>` in `/Resolvers`; uses `IReadRepository<{Entity}>` + `{Entity}ByGuidSpec`.
- [ ] `null` for absent, `ConflictResult<...>` for present; registered `Scoped`.

# Unittest TestCases
- [ ] WHEN the Guid is unseen THEN `ResolveAsync` returns `null`.
- [ ] WHEN the Guid exists THEN `ResolveAsync` returns a `ConflictResult` carrying the existing Id.
