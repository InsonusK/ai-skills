---
description: Cross-module JOIN query handler implementation
project_name: App.Queries
name: "{CrossModuleQueryHandler}.cs"
element_kind: class
change_kind: create
---

# Goals
- Implement a query that requires data from multiple module entity types in a single database query
- Use direct DbContext LINQ projection — the most efficient path for cross-module reads

# Core Principles
- Implements `IRequestHandler<TQuery, Result<T>>`
- Injects `AppDbContext` directly — cross-module JOIN cannot be expressed through single-entity `IReadRepository<T>`
- Always applies `AsNoTracking()` — read-only operation, no tracking overhead
- Uses LINQ projection (`.Select(...)`) directly in handler — or delegates to a cross-module projection spec
- Does **not** use `Include()` — all custom mapping is performed in the handler via `Select()` or manual composition
- Returns `Result.NotFound()` when entity is missing

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Cross-module query handler | `Get{Entity}With{Related}Handler` | `GetTaskWithUserDetailsHandler` | `Get{Entity}With{Related}Handler.cs` | `GetTaskWithUserDetailsHandler.cs` |

# Implementation changes

Inline LINQ projection without `Include()` — all mapping is explicit in `Select()`:

```csharp
// App.Queries/Queries/GetTaskWithUserDetails/GetTaskWithUserDetails.Handler.cs
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace App.Queries.Queries.GetTaskWithUserDetails;

public class GetTaskWithUserDetailsHandler
    : IRequestHandler<GetTaskWithUserDetailsQuery, Result<TaskWithUserDetailsDto>>
{
    private readonly AppDbContext _dbContext;

    public GetTaskWithUserDetailsHandler(AppDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<TaskWithUserDetailsDto>> Handle(
        GetTaskWithUserDetailsQuery query, CancellationToken ct)
    {
        var result = await _dbContext.Set<TodoTask>()
            .AsNoTracking()
            .Where(t => t.Id == query.TaskId)
            .Select(t => new TaskWithUserDetailsDto(
                t.Id,
                t.Title,
                t.Assignee.FullName,
                t.Assignee.Email))
            .FirstOrDefaultAsync(ct);

        return result is null
            ? Result.NotFound()
            : Result.Success(result);
    }
}
```

Alternative — delegate to cross-module projection spec:

```csharp
// App.Queries/Queries/GetTaskWithUserDetails/GetTaskWithUserDetails.Handler.cs
using Ardalis.Result;
using Ardalis.Specification.EntityFrameworkCore;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace App.Queries.Queries.GetTaskWithUserDetails;

public class GetTaskWithUserDetailsHandler
    : IRequestHandler<GetTaskWithUserDetailsQuery, Result<TaskWithUserDetailsDto>>
{
    private readonly AppDbContext _dbContext;

    public GetTaskWithUserDetailsHandler(AppDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<TaskWithUserDetailsDto>> Handle(
        GetTaskWithUserDetailsQuery query, CancellationToken ct)
    {
        var result = await SpecificationEvaluator.Default
            .GetQuery(
                _dbContext.Set<TodoTask>().AsNoTracking(),
                new TaskWithUserDetailsSpec(query.TaskId))
            .FirstOrDefaultAsync(ct);

        return result is null
            ? Result.NotFound()
            : Result.Success(result);
    }
}
```
# Rule changes

## MUST
- Implement `IRequestHandler<TQuery, Result<T>>`
- Inject `AppDbContext` directly
- Perform all mapping in handler — no `Include()` calls
- Return `Result.NotFound()` when entity is missing
- Live in `/App.Queries/Queries/{QueryName}/`
- Cross-module handlers apply `AsNoTracking()` on all queries
- Queries declared as `record` in `/{Module}.Interfaces/Queries`
- Single-module handlers in `/{Module}.Application/Queries` — inject `IReadRepository<T>`
- Single-module handlers load via named specs — no inline LINQ
## MUST NOT
- Modify entity state
- Call `SaveChangesAsync`
- Dispatch commands
- Cross-module handler live in `{Module}.Application` — Application has no multi-module DB access
- Cross-module handlers do not use `Include()` — all mapping is done in handler via `Select()` or manual projection
- Single-module handler use DbContext directly — use `IReadRepository<T>`

# Anti-patterns
- Using `IReadRepository<T>` for cross-module JOIN — repository is single-entity, use DbContext
- Forgetting `AsNoTracking()` — causes unnecessary change tracking overhead
- Using `Include()` followed by client-side mapping — projection belongs in `Select()`
- Loading full entities and mapping afterwards — always project in LINQ or handler

# Unittest TestCases
- [ ] WHEN inspected THEN it implement a query that requires data from multiple module entity types in a single database query
- [ ] WHEN applied THEN Use direct DbContext LINQ projection — the most efficient path for cross-module reads
- [ ] WHEN applied THEN Implements IRequestHandler<TQuery, Result<T>>
- [ ] WHEN applied THEN Injects AppDbContext directly — cross-module JOIN cannot be expressed through single-entity IReadRepository<T>
- [ ] WHEN applied THEN Always applies AsNoTracking() — read-only operation, no tracking overhead
- [ ] WHEN applied THEN Uses LINQ projection (.Select(...)) directly in handler — or delegates to a cross-module projection spec
- [ ] WHEN applied THEN Does **not** use Include() — all custom mapping is performed in the handler via Select() or manual composition
- [ ] WHEN applied THEN Returns Result.NotFound() when entity is missing
- [ ] WHEN naming 'Cross-module query handler' THEN pattern matches convention
