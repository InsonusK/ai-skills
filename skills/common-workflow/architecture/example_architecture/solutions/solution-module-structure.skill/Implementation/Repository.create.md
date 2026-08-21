---
description: Lay out the module's two projects at repository level
element_kind: repository
change_kind: create
tags:
  - solution/module-structure
  - element/repository
---

# Structure

## Project Structure
```
/src/Modules/{ModuleName}
  /{Module}.Domain
  /{Module}.Application
```

## Directory and class skills
| Directory | file | Description |
| --- | --- | --- |
| /{Module}.Domain | {Module}.Domain.csproj | Value objects and entities, no outward references |
| /{Module}.Application | {Module}.Application.csproj | Commands, handlers, validators |

# Rule

## MUST
- Place every module under `/src/Modules/{ModuleName}`.
  - Risk: an agent scanning for module boundaries by folder convention misses a module placed elsewhere.
  - Fix: always create new modules under `/src/Modules/{ModuleName}`.
