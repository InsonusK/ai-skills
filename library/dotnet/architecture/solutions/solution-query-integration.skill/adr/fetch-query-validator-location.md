---
name: fetch-query-validator-location
description: Decide where the shared Page/PageSize validator for IFetchQuery lives, given that Shared must not reference FluentValidation and modules must not reference BuildingBlocks.
problem: The paging rules must be written once and applied to every fetch query, but Shared forbids FluentValidation and a module referencing BuildingBlocks inverts the layer rule.
decision: A generic FetchQueryValidator<TQuery> where TQuery : IFetchQuery in BuildingBlocks, registered once in App.Host as the open generic IValidator<>.
tags:
  - solution/query-integration
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`solution-mediator-integration` keeps FluentValidation out of `Shared`, and a module's projects must not reference `BuildingBlocks`. A paging validator that each query's own validator `Include`s would need one of the two.

# Selected variant
**Selected variant:** [[#Open-generic validator in BuildingBlocks]]
- No per-query code, no forbidden reference; it sits beside `ValidationBehavior`, which already collects every `IValidator<TRequest>`.

# Searched variants

## Open-generic validator in BuildingBlocks

**Selected.**

### Description
`FetchQueryValidator<TQuery> : AbstractValidator<TQuery> where TQuery : IFetchQuery` in `BuildingBlocks/MediatR`; App.Host registers `typeof(IValidator<>)` → `typeof(FetchQueryValidator<>)`. The DI container skips it for requests that do not satisfy the constraint.

### Benefits
- Every fetch query is validated with zero per-query code — a forgotten `Include` is impossible.
- Respects both layer rules.

### Costs
- Relies on the container skipping constrained open generics during `IEnumerable<IValidator<T>>` resolution (Microsoft.Extensions.DependencyInjection does on current .NET).
- The rule is invisible from the query's own folder.

## Include from each query validator

### Description
A `FetchQueryValidator` each `{FeatureName}Validator` pulls in via `Include(...)`.

### Benefits
- Explicit at the query.

### Costs
- The validator must live where modules can reference it — `Shared` (forbidden FluentValidation) or `BuildingBlocks` (forbidden reference).
- A query without its own validator gets no paging validation; a forgotten `Include` silently drops it.

## Rules in every query validator

### Description
Each query's validator repeats `Page`/`PageSize` rules.

### Benefits
- No shared component.

### Costs
- The limits are copied per query and drift.
