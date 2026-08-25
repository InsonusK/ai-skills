---
name: class-query-handler
description: Class {FeatureName}Handler (query) in the v1 plateau
whenToUse: when implementing a single-module read operation
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/class
  - plateau/v1
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
//Plateau: v1
//Version: 20260825140000

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
