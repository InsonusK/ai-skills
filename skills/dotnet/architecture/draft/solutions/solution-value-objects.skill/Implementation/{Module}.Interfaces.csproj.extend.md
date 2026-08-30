---
description: Add a ValueObjects folder to {Module}.Interfaces for permissive value-object records
project_name: "{Module}.Interfaces"
name: "{Module}.Interfaces.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/value-objects
  - element/module-interfaces-csproj
---

# Goals
- Give every module a stable, validation-agnostic place to declare value-object-shaped types that DTOs and other modules can reference

# Implementation changes

**AS IS** (from `plateau-stateless-non-interactive-service`, via `solution-sln-structure`):
```
/{Module}.Interfaces
  /Commands
  /Queries
  /DTOs
  /Events
  {Module}.Interfaces.csproj
```
Allowed Dependencies: `Shared`.

**TO BE** (after this solution):
```
/{Module}.Interfaces
  /Commands
  /Queries
  /DTOs
  /Events
  /ValueObjects
    Soft{ValueObject}.cs
  {Module}.Interfaces.csproj
```
Allowed Dependencies: `Shared` (unchanged).

# Rule changes

## MUST
- Add a `/ValueObjects` folder to `{Module}.Interfaces` for `Soft{ValueObject}` declarations

## MUST NOT
- Reference `{Module}.Domain`, FluentValidation, or any other project from `{Module}.Interfaces`
