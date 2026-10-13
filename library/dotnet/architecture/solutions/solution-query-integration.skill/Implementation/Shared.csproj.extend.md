---
description: Add IFetchQuery to Shared — the paging contract every collection-returning query implements
project_name: "Shared"
name: "Shared.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/query-integration
  - element/shared-csproj
---

# Goals
- Give every fetch query (a query returning a collection) the same paging fields and defaults, callable with no parameters at all

# Rule changes

## MUST
- Add `IFetchQuery` to `Shared/MediatR`, beside `IQuery<TResponse>`
- Keep `Shared` free of FluentValidation — the paging validator lives in `BuildingBlocks`
