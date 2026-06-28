---
name: class-cross-module-query-handler
description: Cross-module JOIN query handler implementation
domain: skill
type: template
version: 20260616
plateau: default
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration.skill]]"
---

# Goal
- Implement a query that requires data from multiple module entity types in a single database query
- Use direct DbContext LINQ projection — the most efficient path for cross-module reads

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create.md|CrossModuleQueryHandler.cs.create]]

# Core Principals
- Implements `IRequestHandler<TQuery, Result<T>>`
- Injects `AppDbContext` directly — cross-module JOIN cannot be expressed through single-entity `IReadRepository<T>`
- Always applies `AsNoTracking()` — read-only operation, no tracking overhead
- Uses LINQ projection (`.Select(...)`) directly in handler — or delegates to a cross-module projection spec
- Does **not** use `Include()` — all custom mapping is performed in the handler via `Select()` or manual composition
- Returns `Result.NotFound()` when entity is missing

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create.md|CrossModuleQueryHandler.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Cross-module query handler | `Get{Entity}With{Related}Handler` | `GetTaskWithUserDetailsHandler` | `Get{Entity}With{Related}Handler.cs` | `GetTaskWithUserDetailsHandler.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create.md|CrossModuleQueryHandler.cs.create]]

# Implementation
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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create.md|CrossModuleQueryHandler.cs.create]]

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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create.md|CrossModuleQueryHandler.cs.create]]

# Anti-patterns
- Using `IReadRepository<T>` for cross-module JOIN — repository is single-entity, use DbContext
- Forgetting `AsNoTracking()` — causes unnecessary change tracking overhead
- Using `Include()` followed by client-side mapping — projection belongs in `Select()`
- Loading full entities and mapping afterwards — always project in LINQ or handler

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create.md|CrossModuleQueryHandler.cs.create]]

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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/CrossModuleQueryHandler.cs.create.md|CrossModuleQueryHandler.cs.create]]
