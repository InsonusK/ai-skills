---
name: class-entity-version-resolver
description: Class {Entity}VersionResolver in the statefull-service plateau
whenToUse: when a mutable entity's current version needs to be readable by ConcurrencyBehavior
domain: skill
type: template
plateau: statefull-service
version: 20260824100000
tags:
  - skill/template/class
  - plateau/statefull-service
created_by:
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
---

# Goal
- Read one entity type's current version, discoverable by the app-wide `EntityVersionResolverFactory` via a stable business name

# Core Principles
- Uses `IReadRepository<T>` + `{Entity}ByIdSpec` — never inline LINQ, never `IRepository<T>`
- `VersionedEntityName` is a stable business name, not a C# type name — decouples the resolver's DI key from assembly structure

# Implementation
```csharp
//Skill: class-entity-version-resolver
//Plateau: statefull-service
//Version: 20260824100000

public sealed class TaskVersionResolver(IReadRepository<TodoTask> repository) : IEntityVersionResolver
{
    public string VersionedEntityName => "Task";

    public async Task<uint?> GetCurrentVersionForAsync(int id, CancellationToken ct)
    {
        var task = await repository.FirstOrDefaultAsync(new TaskByIdSpec(id), ct);
        return task?.Version;
    }
}
```

# Rules
MUST:
- Live in `{Module}.Application/Concurrency`, one per `IVersioned` entity in this module
- `VersionedEntityName` match the same constant declared on that entity's `{Entity}Config`
- Use `IReadRepository<T>` + the entity's `{Entity}ByIdSpec`
MUST NOT:
- Duplicate the spec's query logic inline

# Check list
- [ ] `{Entity}VersionResolver` exists for every `IVersioned` entity, registered as `IEntityVersionResolver`
- [ ] `VersionedEntityName` matches the entity config's constant
- [ ] Uses `IReadRepository<T>` + `{Entity}ByIdSpec`, no inline LINQ

__Applied solutions:__
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/{Module}.Application.csproj.extend/{Entity}VersionResolver.cs.create.md|{Entity}VersionResolver.cs.create]]
