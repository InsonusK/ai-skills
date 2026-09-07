---
description: Add a ValueObjects folder to {Module}.Interfaces for permissive value-object records
project_name: "{Module}.Interfaces"
name: "{Module}.Interfaces.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/soft-value-objects
  - element/module-interfaces-csproj
---

# Goals
- Give every module a stable, validation-agnostic place to declare value-object-shaped types that DTOs and other modules can reference.

# Implementation changes

**AS IS** (from `solution-sln-structure`):
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
- Add a `/ValueObjects` folder to `{Module}.Interfaces` for `Soft{ValueObject}` declarations.
  - Risk: `Soft{ValueObject}` records scattered across `/DTOs` or the project root make the value vocabulary hard to find and review.
  - Fix: one `/ValueObjects` folder, one record per file.
- Never reference `{Module}.Domain`, FluentValidation, or any project other than `Shared` from `{Module}.Interfaces`.
  - Risk: the module's public-contract project pulls in its own internals or a validation framework, and every consumer inherits that dependency.
  - Fix: `{Module}.Interfaces` stays a leaf that depends only on `Shared`.
