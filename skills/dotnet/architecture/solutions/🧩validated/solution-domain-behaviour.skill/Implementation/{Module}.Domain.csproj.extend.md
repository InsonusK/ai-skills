---
description: Add a Services folder to {Module}.Domain for static domain service extension methods
project_name: "{Module}.Domain"
name: "{Module}.Domain.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/domain-behaviour
  - element/module-domain-csproj
---

# Goals
- Give every module a place for bulky or multi-step entity behavior that doesn't fit naturally inside the Entity itself

# Rule changes

## MUST
- Add a `/Services` folder to `{Module}.Domain` for static domain service extension methods
