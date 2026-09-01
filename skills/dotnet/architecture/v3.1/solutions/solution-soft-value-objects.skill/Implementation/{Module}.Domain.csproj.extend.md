---
description: Add a ValueObjects folder to {Module}.Domain and a project reference to {Module}.Interfaces for the Soft{ValueObject} base types
project_name: "{Module}.Domain"
name: "{Module}.Domain.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/value-objects
  - element/module-domain-csproj
---

# Goals
- Give every module a strict, self-validating Value Object type per domain concept that needs invariant enforcement

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
  /ValueObjects
    {ValueObject}.cs
  {Module}.Domain.csproj
```
Allowed Dependencies: `Shared`, `Microsoft.EntityFrameworkCore` (`IEntityTypeConfiguration` only), plus a new reference to `{Module}.Interfaces` — for the `Soft{ValueObject}` base types `{ValueObject}` inherits from.

# Rule changes

## MUST
- Add a `/ValueObjects` folder to `{Module}.Domain`
- Reference `{Module}.Interfaces` for the `Soft{ValueObject}` base types

## MUST NOT
- Reference any other module's `{Module}.Domain` or `{Module}.Interfaces`
