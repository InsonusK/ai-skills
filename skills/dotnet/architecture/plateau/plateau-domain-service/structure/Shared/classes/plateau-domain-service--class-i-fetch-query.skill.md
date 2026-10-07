---
name: plateau-domain-service--class-i-fetch-query
description: Interface IFetchQuery in the plateau-domain-service plateau — the paging contract (Page, PageSize, defaults, cap) every collection-returning query implements, in Shared/MediatR
whenToUse: when creating or editing IFetchQuery, or adding a query that returns a collection
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
- Give every fetch query the same paging fields and defaults, so it is callable with no paging parameters and returns the first 100 items.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]] - [[skills/dotnet/architecture/solutions/solution-query-integration.skill/Implementation/Shared.csproj.extend/IFetchQuery.cs.create|IFetchQuery.cs]]

# Core Principles
- Apply ONE plateau template per class.
- `Page` is 1-based; `PageSize` defaults to 100 and is capped at 1000 by `FetchQueryValidator<TQuery>` in BuildingBlocks.
- A collection-returning query implements `IQuery<...>` and `IFetchQuery`, taking `Page`/`PageSize` as optional record parameters defaulted from the constants.
- Lives in `Shared/MediatR`, `namespace Shared.MediatR`.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Paging contract | `IFetchQuery` | `IFetchQuery` | `IFetchQuery.cs` | `IFetchQuery.cs` |

# Implementation
```csharp
// Skill: plateau-domain-service--class-i-fetch-query
// Plateau: domain-service
// Version: 20261006000000
namespace Shared.MediatR;

public interface IFetchQuery
{
    const int DefaultPage = 1;
    const int DefaultPageSize = 100;
    const int MaxPageSize = 1000;

    int Page { get; }
    int PageSize { get; }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]] - [[skills/dotnet/architecture/solutions/solution-query-integration.skill/Implementation/Shared.csproj.extend/IFetchQuery.cs.create|IFetchQuery.cs]]

# Rules
MUST:
- Implement `IFetchQuery` on every query that returns a collection, defaulting `Page`/`PageSize` from `IFetchQuery.DefaultPage`/`IFetchQuery.DefaultPageSize`.
- Never apply several plateau templates per class.

# Check list
- [ ] `IFetchQuery` in `Shared/MediatR/IFetchQuery.cs` with `DefaultPage = 1`, `DefaultPageSize = 100`, `MaxPageSize = 1000`.

# Unittest TestCases
- [ ] WHEN a fetch query is constructed with no paging arguments THEN `Page` is 1 and `PageSize` is 100
