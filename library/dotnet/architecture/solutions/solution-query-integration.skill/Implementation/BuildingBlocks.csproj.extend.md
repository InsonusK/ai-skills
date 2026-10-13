---
description: Add FetchQueryValidator<TQuery> to BuildingBlocks — the one paging validator every fetch query gets
project_name: "BuildingBlocks"
name: "BuildingBlocks.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/query-integration
  - element/buildingblocks-csproj
---

# Goals
- Validate the paging fields of every fetch query in one place, next to `ValidationBehavior`, without any per-query validator code

# Rule changes

## MUST
- Add `FetchQueryValidator<TQuery>` to `BuildingBlocks/MediatR`, beside `ValidationBehavior`
