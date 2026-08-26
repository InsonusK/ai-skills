---
name: plateau-shared-rules--class-specification
description: Classes {Entity}ByIdSpec/{Entity}SummarySpec/{Entity}ByGuidSpec in the shared-rules plateau
whenToUse: when a handler or resolver needs to load an entity, or project it to a read shape, without inline LINQ
domain: skill
type: template
plateau: shared-rules
version: 20260824163000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
---

# Goal
- Encapsulate query intent in named, reusable specification classes — a repository executes them, it never touches the database directly

# Core Principles
- `Specification<T>` returns the entity itself; `Specification<T, TResult>` projects to a DTO inside the query
- Spec name reflects intent, not implementation — `TaskByIdSpec`, not `TaskWhereId`
- All specs for a module live in `{Module}.Application/Specifications` — one discoverable place

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Load by internal id | {Entity}ByIdSpec | TaskByIdSpec | {Entity}ByIdSpec.cs | TaskByIdSpec.cs |
| Projection | {Entity}SummarySpec | TaskSummarySpec | {Entity}SummarySpec.cs | TaskSummarySpec.cs |
| Load by client Guid | {Entity}ByGuidSpec | TaskByGuidSpec | {Entity}ByGuidSpec.cs | TaskByGuidSpec.cs |

# Implementation
```csharp
//Skill: class-specification
//Plateau: shared-rules
//Version: 20260824163000

public sealed class TaskByIdSpec : Specification<TodoTask>
{
    public TaskByIdSpec(int id) => Query.Where(t => t.Id == id);
}

public sealed class TaskSummarySpec : Specification<TodoTask, TaskSummaryDto>
{
    public TaskSummarySpec(int id)
    {
        Query.Where(t => t.Id == id)
             .Select(t => new TaskSummaryDto(t.Id, t.Title));
    }
}

public sealed class TaskByGuidSpec : Specification<TodoTask>
{
    public TaskByGuidSpec(Guid guid) => Query.Where(t => t.Guid == guid);
}
```

# Rules
MUST:
- Live in `{Module}.Application/Specifications`, one class per query intent
- `{Entity}ByGuidSpec` exist for every `IHasGuid` entity, used only by that entity's `Create{Entity}GuidResolver`
MUST NOT:
- Contain a business rule (e.g. `.Where(t => t.Price * 0.9m > threshold)`) — that belongs in Domain
- Be shared across unrelated entity types

# Check list
- [ ] Every entity loaded by Id has a `{Entity}ByIdSpec`
- [ ] Every `IHasGuid` entity has a `{Entity}ByGuidSpec`
- [ ] Spec names reflect intent, not field names

__Applied solutions:__
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByIdSpec.cs.create.md|{Entity}ByIdSpec.cs.create]], [[../../../../../solutions/solution-repository-integration.skill/Implementation/{Module}.Application.csproj.extend/{Entity}SummarySpec.cs.create.md|{Entity}SummarySpec.cs.create]]
- [[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[../../../../../solutions/solution-external-created-entity.skill/Implementation/{Module}.Application.csproj.extend/{Entity}ByGuidSpec.cs.create.md|{Entity}ByGuidSpec.cs.create]]
