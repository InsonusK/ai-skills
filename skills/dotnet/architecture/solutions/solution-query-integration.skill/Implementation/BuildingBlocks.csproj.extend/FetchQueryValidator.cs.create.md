---
description: Generic paging validator applied to every IFetchQuery through an open-generic registration
project_name: "BuildingBlocks"
name: "FetchQueryValidator.cs"
element_kind: class
change_kind: create
tags:
  - solution/query-integration
  - element/fetchqueryvalidator-cs
---

# Goals
- Reject an out-of-range `Page`/`PageSize` with `Result.Invalid` before any fetch query handler runs.

# Core Principles
- One generic validator constrained to `IFetchQuery`, registered once as an open generic; `ValidationBehavior` resolves it together with the query's own validator, if any. Decision recorded in [fetch-query-validator-location](../../adr/fetch-query-validator-location.md).
- Transport correctness only: `Page >= 1`, `1 <= PageSize <= MaxPageSize`.

# Implementation changes

```csharp
// BuildingBlocks/MediatR/FetchQueryValidator.cs
using FluentValidation;
using Shared;

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

# Rule changes

## MUST
- Constrain the validator to `where TQuery : IFetchQuery` and register it as an open generic (see [App.Host.csproj](../App.Host.csproj.extend.md)).
  - Risk: without the constraint the registration fails for every non-fetch request; without the registration no fetch query is validated.
  - Fix: keep the constraint; register `typeof(IValidator<>)` → `typeof(FetchQueryValidator<>)`.
- Never repeat `Page`/`PageSize` rules in a query's own `{FeatureName}Validator`.
  - Risk: two copies of the limits drift.
  - Fix: the query's validator covers its own fields only.

# Check list
- [ ] `FetchQueryValidator<TQuery>` in `BuildingBlocks/MediatR`, constrained to `IFetchQuery`.
- [ ] No query-specific validator contains a `Page`/`PageSize` rule.

# Unittest TestCases
- [ ] WHEN a fetch query has `Page = 0` THEN `ValidationBehavior` returns `Result.Invalid` before the handler runs
- [ ] WHEN a fetch query has `PageSize = 1001` THEN `Result.Invalid`
- [ ] WHEN a fetch query has `PageSize = 1000` THEN no paging error
- [ ] WHEN a non-fetch query is sent THEN `FetchQueryValidator` is not resolved for it
