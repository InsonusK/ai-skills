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

# Implementation changes

**AS IS** (from `plateau-stateless-non-interactive-service`, via `solution-sln-structure`):
```
/{Module}.Domain
  /Entities
    {Entity}.cs
  {Module}.Domain.csproj
```
Allowed Dependencies: `Shared`, `Microsoft.EntityFrameworkCore` (`IEntityTypeConfiguration` only).

**TO BE** (after this solution):
```
/{Module}.Domain
  /Entities
    {Entity}.cs
  /Services
    {Behavior}Service.cs
  {Module}.Domain.csproj
```
Allowed Dependencies: unchanged.

# Rule changes

## MUST
- Add a `/Services` folder to `{Module}.Domain` for static domain service extension methods
