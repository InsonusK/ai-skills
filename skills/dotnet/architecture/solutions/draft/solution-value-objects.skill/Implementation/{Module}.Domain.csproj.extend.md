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

# Rule changes

## MUST
- Add a `/ValueObjects` folder to `{Module}.Domain`
- Reference `{Module}.Interfaces` for the `Soft{ValueObject}` base types

## MUST NOT
- Reference any other module's `{Module}.Domain` or `{Module}.Interfaces`
