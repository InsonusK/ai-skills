---
name: plateau-shared-rules--class-cross-module-query-handler
description: Class {CrossModuleQueryHandler} in the shared-rules plateau
whenToUse: when a read operation needs to JOIN entity types across two or more modules
domain: skill
type: template
plateau: shared-rules
version: 20260824163000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]]"
---

# Goal
- Answer one cross-module read by querying `AppDbContext` directly, projecting straight to the DTO declared in the owning module's Interfaces

# Core Principles
- Uses `AppDbContext` with `AsNoTracking()` directly — no repository abstraction, since a JOIN spans entity types no single module's `IReadRepository<T>` can reach
- Never `Include()`s — mapping happens explicitly in the handler
- The query contract is declared in `{Module}.Interfaces/Queries`; this handler only implements it

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Cross-module query handler | Get{Thing}With{OtherThing}Handler | GetTaskWithUserDetailsHandler | {QueryName}.Handler.cs | GetTaskWithUserDetails.Handler.cs |

# Implementation
```csharp
//Skill: class-cross-module-query-handler
//Plateau: shared-rules
//Version: 20260824163000

public class GetTaskWithUserDetailsHandler(AppDbContext db)
    : IRequestHandler<GetTaskWithUserDetailsQuery, Result<TaskWithUserDetailsDto>>
{
    public async Task<Result<TaskWithUserDetailsDto>> Handle(GetTaskWithUserDetailsQuery query, CancellationToken ct)
    {
        var dto = await (
            from task in db.Set<TodoTask>().AsNoTracking()
            join user in db.Set<User>().AsNoTracking() on task.AssigneeId equals user.Id
            where task.Id == query.TaskId
            select new TaskWithUserDetailsDto(task.Id, task.Title, user.Name)
        ).FirstOrDefaultAsync(ct);

        return dto is null ? Result.NotFound() : Result.Success(dto);
    }
}
```

# Rules
MUST:
- Inject `AppDbContext` directly, apply `AsNoTracking()`
- Live in `/App.Queries/Queries/{QueryName}/`
MUST NOT:
- Use `Include()` — map explicitly in the query/handler
- Mutate entity state or call `SaveChangesAsync`

# Check list
- [ ] Handler injects `AppDbContext` directly, applies `AsNoTracking()`
- [ ] No `Include()`, no state mutation, no `SaveChangesAsync`
- [ ] Query contract declared in the owning module's Interfaces, not here

__Applied solutions:__
- [[../../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[../../../../../solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create.md|CrossModuleQueryHandler.cs.create]]
