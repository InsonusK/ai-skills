---
description: Paging contract for a fetch query — Page and PageSize with fixed defaults and a fixed cap
project_name: "Shared"
name: "IFetchQuery.cs"
element_kind: class
change_kind: create
tags:
  - solution/query-integration
  - element/ifetchquery-cs
---

# Goals
- Standardize the paging fields of every fetch query, so a caller can send one with no paging parameters and get the first 100 items.

# Core Principles
- A fetch query is any query that returns a collection; it implements `IFetchQuery` in addition to `IQuery<TResponse>`.
- `Page` is 1-based; `PageSize` defaults to `DefaultPageSize` (100) and is capped at `MaxPageSize` (1000). Decision recorded in [fetch-query-paging-shape](../../adr/fetch-query-paging-shape.md).
- The handler returns the requested page only — no total count.

# Structure

## Project Structure
```
/Shared
  /MediatR
    IQuery.cs
    IFetchQuery.cs
```

# Implementation changes

```csharp
// Shared/MediatR/IFetchQuery.cs
namespace Shared;

public interface IFetchQuery
{
    const int DefaultPage = 1;
    const int DefaultPageSize = 100;
    const int MaxPageSize = 1000;

    int Page { get; }
    int PageSize { get; }
}
```

A fetch query in `{Module}.Interfaces` takes the defaults as optional record parameters:

```csharp
// {Module}.Interfaces/Queries/GetTasksQuery.cs
public sealed record GetTasksQuery(
    long AssigneeId,
    int Page = IFetchQuery.DefaultPage,
    int PageSize = IFetchQuery.DefaultPageSize)
    : IQuery<Result<IReadOnlyList<TaskSummaryDto>>>, IFetchQuery;
```

# Rule changes

## MUST
- Implement `IFetchQuery` on every query that returns a collection.
  - Risk: each list query invents its own paging names and defaults, or loads the whole table.
  - Fix: add `IFetchQuery` and the two optional record parameters.
- Default `Page` to `IFetchQuery.DefaultPage` and `PageSize` to `IFetchQuery.DefaultPageSize` in the record's parameter list.
  - Risk: a caller sending no paging parameters gets `PageSize = 0` and an empty page.
  - Fix: use the constants as parameter defaults, never literals.

# Check list
- [ ] `IFetchQuery` in `Shared/MediatR/IFetchQuery.cs` with `DefaultPage = 1`, `DefaultPageSize = 100`, `MaxPageSize = 1000`.
- [ ] Every collection-returning query implements `IFetchQuery` and defaults `Page`/`PageSize` from its constants.

# Unittest TestCases
- [ ] WHEN a fetch query is constructed with no paging arguments THEN `Page` is 1 and `PageSize` is 100
