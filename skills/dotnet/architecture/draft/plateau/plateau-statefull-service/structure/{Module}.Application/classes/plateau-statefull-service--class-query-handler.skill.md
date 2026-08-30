---
name: plateau-statefull-service--class-query-handler
description: Class {FeatureName}Handler (query) in the statefull-service plateau
whenToUse: when implementing a single-module read operation
domain: skill
type: template
plateau: statefull-service
version: 20260824100000
tags:
  - skill/template/class
  - plateau/statefull-service
created_by:
  - "[[../../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]]"
---

# Goal
- Answer one single-module read using `IReadRepository<T>` and a named spec — never `DbContext`, never `IRepository<T>`

# Core Principles
- Lives in `/{Module}.Application/Queries/{FeatureName}`, separate from the command `/Features` tree
- An optional transport validator may exist alongside it — same shape as a command validator, structural correctness only

# Implementation
```csharp
//Skill: class-query-handler
//Plateau: statefull-service
//Version: 20260824100000

public class GetTaskSummaryHandler(IReadRepository<TodoTask> repository)
    : IRequestHandler<GetTaskSummaryQuery, Result<TaskSummaryDto>>
{
    public async Task<Result<TaskSummaryDto>> Handle(GetTaskSummaryQuery query, CancellationToken ct)
    {
        var dto = await repository.FirstOrDefaultAsync(new TaskSummarySpec(query.TaskId), ct);
        return dto is null ? Result.NotFound() : Result.Success(dto);
    }
}
```

# Rules
MUST:
- Inject `IReadRepository<T>`, load via a named spec
- Live in `/{Module}.Application/Queries/{FeatureName}`
MUST NOT:
- Inject `IRepository<T>` or `DbContext`
- Mutate entity state or dispatch a command

# Check list
- [ ] Handler injects `IReadRepository<T>` only, loads via named spec
- [ ] No entity mutation, no `SaveChangesAsync`

__Applied solutions:__
- [[../../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[../../../../../solutions/solution-query-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Handler.cs.create.md|{FeatureName}.Handler.cs.create]]
