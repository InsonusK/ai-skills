---
description: Cross-module JOIN query handler implementation
project_name: App.Queries
name: "{CrossModuleQueryHandler}.cs"
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

# Rules

MUST:
- Implement `IRequestHandler<TQuery, Result<T>>`
- Inject `AppDbContext` directly
- Apply `AsNoTracking()` on all queries
- Perform all mapping in handler — no `Include()` calls
- Return `Result.NotFound()` when entity is missing
- Live in `/App.Queries/Queries/{QueryName}/`

MUST NOT:
- Modify entity state
- Call `SaveChangesAsync`
- Dispatch commands

# Anti-patterns
- Using `IReadRepository<T>` for cross-module JOIN — repository is single-entity, use DbContext
- Forgetting `AsNoTracking()` — causes unnecessary change tracking overhead
- Using `Include()` followed by client-side mapping — projection belongs in `Select()`
- Loading full entities and mapping afterwards — always project in LINQ or handler
