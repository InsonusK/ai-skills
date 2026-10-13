---
name: fetch-query-paging-shape
description: Decide the paging fields, defaults, limits, and response shape of a fetch query (a query returning a collection).
problem: Fetch queries had no common paging contract — each could name, default, and bound its paging differently, or load everything.
decision: IFetchQuery with Page (1-based, default 1) and PageSize (default 100, validated 1..1000); the handler returns the requested page only.
tags:
  - solution/query-integration
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem
A fetch query must be callable with no parameters, and every fetch query should page the same way. The open points were the upper bound on `PageSize` and whether the response carries a total count.

# Selected variant
**Selected variant:** [[#Page and PageSize, capped at 1000]]
- Chosen by the project owner: defaults from the original request, plus a cap so one call cannot load the whole table.

# Searched variants

## Page and PageSize, capped at 1000

**Selected.**

### Description
`Page` (1-based, default 1), `PageSize` (default 100), validated `Page >= 1`, `1 <= PageSize <= 1000`. The handler returns `IReadOnlyList<TDto>` for the page.

### Benefits
- One call is bounded at 1000 rows.
- No extra query per call.

### Costs
- A client cannot render "page N of M" without a separate count query.

## Page and PageSize, no cap

### Description
As above, validating only `> 0`.

### Benefits
- Exactly the original request.

### Costs
- `PageSize = 1000000` loads the table in one call.

## Page and PageSize plus TotalCount

### Description
As the selected variant, with a `PagedResult<T>` response carrying `TotalCount`.

### Benefits
- Clients can render pagers directly.

### Costs
- An extra `COUNT` query on every call, also for callers that never use it.
