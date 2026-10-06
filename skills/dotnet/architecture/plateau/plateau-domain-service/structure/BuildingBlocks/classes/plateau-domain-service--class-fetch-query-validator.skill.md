---
name: plateau-domain-service--class-fetch-query-validator
description: Class FetchQueryValidator<TQuery> in the plateau-domain-service plateau — the one Page/PageSize validator every IFetchQuery gets via an open-generic registration
whenToUse: when creating or editing FetchQueryValidator, or changing the paging limits of fetch queries
domain: skill
type: template
plateau: domain-service
version: 20261006000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]]"
---

# Goal
- Reject an out-of-range `Page`/`PageSize` with `Result.Invalid` before any fetch query handler runs, with no per-query validator code.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]] - [[skills/dotnet/architecture/solutions/solution-query-integration.skill/Implementation/BuildingBlocks.csproj.extend/FetchQueryValidator.cs.create|FetchQueryValidator.cs]]

# Core Principles
- Apply ONE plateau template per class.
- Constrained to `IFetchQuery` and registered once as the open generic `IValidator<>` in `PipelineRegistration`; `ValidationBehavior` resolves it with each query's own validators.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Paging validator | `FetchQueryValidator<TQuery>` | `FetchQueryValidator<TQuery>` | `FetchQueryValidator.cs` | `FetchQueryValidator.cs` |

# Implementation
```csharp
// Skill: plateau-domain-service--class-fetch-query-validator
// Plateau: domain-service
// Version: 20261006000000
using FluentValidation;
using Shared.MediatR;

namespace BuildingBlocks.MediatR;

public sealed class FetchQueryValidator<TQuery> : AbstractValidator<TQuery>
    where TQuery : IFetchQuery
{
    public FetchQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, IFetchQuery.MaxPageSize);
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]] - [[skills/dotnet/architecture/solutions/solution-query-integration.skill/Implementation/BuildingBlocks.csproj.extend/FetchQueryValidator.cs.create|FetchQueryValidator.cs]]

# Rules
MUST:
- Keep the `where TQuery : IFetchQuery` constraint.
- Never repeat `Page`/`PageSize` rules in a query's own validator.
- Never apply several plateau templates per class.

# Check list
- [ ] `FetchQueryValidator<TQuery>` in `BuildingBlocks/MediatR`, constrained to `IFetchQuery`, registered in `PipelineRegistration`.

# Unittest TestCases
- [ ] WHEN a fetch query has `Page = 0` THEN `Result.Invalid` before the handler runs
- [ ] WHEN a fetch query has `PageSize = 1001` THEN `Result.Invalid`
- [ ] WHEN a non-fetch query is sent THEN `FetchQueryValidator` is not resolved for it
