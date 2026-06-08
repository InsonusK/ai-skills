---
uid: ae1e4d1a-3191-4e50-9675-647163c36dde
status: in-work
name: backend-project-structure
description: defines the top-level solution structure, all layers, and dependency rules
domain: skill
type: architecture
tags:
  - dotnet
  - architecture
  - structure
triggers:
  - project initialization
  - new module creation
  - file placement decision
---
# Goal
Define the top-level structure of the backend solution — what layers exist, where they live, and what dependencies are allowed between them. This skill is the entry point for understanding how the solution is organized. Layer details are delegated to dedicated layer skills.

# Solution Structure
```
/src
  /Modules              ← bounded context modules — see [[module-layer.skill]]
    /Task
    /TimeLog
    /User
  /App                  ← composition root layer
    /App.Host           ← see [[app-host.skill]]
    /App.Infrastructure ← see [[app-infrastructure.skill]]
    /App.Infrastructure.Migrations
    /App.Queries        ← see [[app-queries.skill]]
  /Shared               ← see [[shared-layer.skill]]
  /BuildingBlocks       ← see [[building-blocks.skill]]
```

# Layers Overview

| Layer                | Path                         | Project                                                                                                       | Responsibility                                              | Detail                                                                                             |
| -------------------- | ---------------------------- | ------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| Module — Api         | `/src/Modules/{ModuleName}/` | `{ModuleName}.Api`                                                                                            | HTTP endpoints, MediatR dispatch                            | [[module-api.skill]]                                                                               |
| Module — Interfaces  | `/src/Modules/{ModuleName}/` | `{ModuleName}.Interfaces`                                                                                     | Public contracts — commands, queries, DTOs, events          | [[skills/dotnet/skill-graph/developing/Module/module-layer.skill]]                                 |
| Module — Application | `/src/Modules/{ModuleName}/` | `{ModuleName}.Application`                                                                                    | Orchestration — handlers, validators, specs                 | [[skills/dotnet/skill-graph/developing/Module/Application csproj/module-application.csproj.skill]] |
| Module — Domain      | `/src/Modules/{ModuleName}/` | [[skills/dotnet/skill-graph/developing/Module/Domain csproj/module-domain-csproj.skill\|{ModuleName}.Domain]] | Business logic — entities, VOs, rules, events               | [[skills/dotnet/skill-graph/developing/Module/Domain csproj/module-domain-csproj.skill]]           |
| App.Host             | `/src/App/`                  | `App.Host`                                                                                                    | Composition root — DI, pipeline, module wiring              | [[app-host.skill]]                                                                                 |
| App.Infrastructure   | `/src/App/`                  | `App.Infrastructure`                                                                                          | Persistence — DbContext, repos, outbox, messaging           | [[app-infrastructure.skill]]                                                                       |
| App.Queries          | `/src/App/`                  | `App.Queries`                                                                                                 | Cross-module read models and JOIN queries                   | [[app-queries.skill]]                                                                              |
| Shared               | `/src/`                      | `Shared`                                                                                                      | Cross-cutting primitives — Result, Exceptions               | [[shared-layer.skill]]                                                                             |
| BuildingBlocks       | `/src/`                      | `BuildingBlocks`                                                                                              | Reusable framework patterns — pipeline behaviors, spec base | [[building-blocks.skill]]                                                                          |

# Dependency Rules

## Allowed

```
App.Host               → {ModuleName}.Api
App.Host               → {ModuleName}.Application
App.Host               → App.Infrastructure
App.Host               → App.Queries

{ModuleName}.Api       → {ModuleName}.Interfaces

App.Infrastructure.Migrations → App.Infrastructure

App.Queries            → App.Infrastructure
App.Queries            → {ModuleName}.Domain
App.Queries            → {ModuleName}.Interfaces

App.Infrastructure     → {ModuleName}.Domain
App.Infrastructure     → {ModuleName}.Interfaces
App.Infrastructure     → BuildingBlocks

{ModuleName}.Application → {ModuleName}.Interfaces
{ModuleName}.Application → {ModuleName}.Domain
{ModuleName}.Application → {OtherModuleName}.Interfaces
{ModuleName}.Application → Shared
{ModuleName}.Application → BuildingBlocks

{ModuleName}.Domain    → Shared
{ModuleName}.Domain    → Microsoft.EntityFrameworkCore (IEntityTypeConfiguration only)
```

## Forbidden

```
{ModuleName}.Domain      → {ModuleName}.Application
{ModuleName}.Domain      → App.Infrastructure
{ModuleName}.Domain      → App.Queries
{ModuleName}.Domain      → {OtherModuleName}.Domain
{ModuleName}.Application → App.Infrastructure
{ModuleName}.Application → App.Queries
{ModuleName}.Application → {OtherModuleName}.Application
{ModuleName}.Api         → App.Infrastructure
{ModuleName}.Api         → App.Queries
```

# Test Structure
Tests are co-located with modules — no global `/tests` folder.

```
/Modules/{ModuleName}
  /{ModuleName}.Api.Tests
  /{ModuleName}.Application.Tests
  /{ModuleName}.Domain.Tests
  /{ModuleName}.Integration.Tests
```

# Anti Goals
- Do not centralize tests in root folder
- Do not bypass Host composition layer
- Do not reference Infrastructure from Application, Domain, or API
- Do not put cross-module JOINs in Application or Domain
- Do not collapse modules into a shared monolith
- Do not flatten structure into only layers without modules

# Checklist
- [ ]  Each module has its own .csproj separation
- [ ]  Domain is pure — no infrastructure dependencies except EF Core for configurations
- [ ]  Application does not depend on Infrastructure or App.Queries
- [ ]  API is thin and dispatch-only
- [ ]  Cross-module queries implemented in App.Queries, declared in Interfaces
- [ ]  EF entity configurations live in Domain/Configurations
- [ ]  Cross-module FK configurations live in App.Infrastructure only
- [ ]  Host composes all module APIs, infrastructure, and queries
- [ ]  Tests are colocated with modules
- [ ]  No cross-module Domain references
- [ ]  File placement follows rules consistently

# Relations
- [[skills/dotnet/skill-graph/developing/Module/module-layer.skill]] — module structure, 4 projects, inter-module rules
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/module-domain-csproj.skill]] — Domain project detail
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/module-application.csproj.skill]] — Application project detail
- [[module-api.skill]] — Api project detail
- [[app-host.skill]] — Host project detail
- [[app-infrastructure.skill]] — Infrastructure project detail
- [[app-queries.skill]] — Queries project detail
- [[shared-layer.skill]] — Shared project detail
- [[building-blocks.skill]] — BuildingBlocks project detail