---
description: Create the module's domain layer project — entities, value objects, domain services, domain events
name: "{Module}.Domain.csproj"
element_kind: project
change_kind: create
tags:
  - solution/domain-behaviour
  - element/module-domain-csproj
---

# Goals
- Own the entities, domain services, and (later) value objects and rules for this bounded context.
- Be the only layer that contains entity definitions.

# Core Principles
- All entities live in `/{Module}.Domain/Entities`; domain services in `/Services`.
- Domain references only `Shared` and `{Module}.Interfaces` — nothing else, no infrastructure, no EF Core.

# Structure

## Project Structure
```
/src/Modules/{ModuleName}
  /{ModuleName}.Domain
    /Entities
      {EntityName}.cs
    /Services
      {Behavior}Service.cs
    {ModuleName}.Domain.csproj
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Entities | All entity types for this module |
| /Services | Static domain-service extension methods for bulky behavior |

# NuGet Packages
| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
| — | — | none; EF Core is added by `solution-domain-configuration` (VP2), not here |

# What Does NOT Belong Here
- Orchestration / handlers — `{Module}.Application`.
- Persistence, `DbContext`, repositories, EF configuration — added by VP2 solutions.
- Cross-module queries — `App.Queries` (VP2).
- Public DTOs / commands / Soft VOs — `{Module}.Interfaces`.

# Allowed Dependencies
- `Shared`
- `{Module}.Interfaces` (for `Soft{ValueObject}` base types)

# Rules

## MUST
- Reference only `Shared` and `{Module}.Interfaces`.
  - Risk: an EF Core / infrastructure reference couples every domain-bearing module to persistence, breaking VP1↔VP2 independence.
  - Fix: VP2 solutions add persistence references; this project stays clean.
- Place every entity under `/Entities` and every domain service under `/Services`.
  - Risk: entities scattered across the project defeat navigation and the architecture tests that scan `/Entities`.
  - Fix: the two folders, one type per file.
- Never reference another module's `Domain` or `Application`.
  - Risk: shared entity types across modules break the bounded context.
  - Fix: exchange data as `{Module}.Interfaces` DTOs via MediatR.

# Check list
- [ ] `{Module}.Domain.csproj` references only `Shared` and `{Module}.Interfaces`.
- [ ] `/Entities` and `/Services` folders exist.
- [ ] No `DbContext` / EF Core reference anywhere in the project.
- [ ] No cross-module `Domain`/`Application` reference.
