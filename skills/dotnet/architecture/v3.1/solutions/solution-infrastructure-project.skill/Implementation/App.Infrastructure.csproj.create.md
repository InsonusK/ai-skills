---
description: Empty project, the single home for future outbound infrastructure integrations
project_name: "App.Infrastructure"
name: "App.Infrastructure.csproj"
element_kind: project
change_kind: create
tags:
  - solution/infrastructure-project
  - element/app-infrastructure-csproj
---

# Goals
- Give the service a single, reusable project every outbound infrastructure integration extends, instead of each integration solution deciding on its own where its implementation code lives

# Structure

## Project Structure
```
/App.Infrastructure
  App.Infrastructure.csproj
```

Empty by design — a concern-specific solution (e.g. `solution-repository-integration`) adds its own folder here (`/Persistence`, `/Repositories`, ...) when it extends this project.

## Allowed Dependencies
- BuildingBlocks
- Shared
- `{ModuleName}.Domain` (all modules)
- `{ModuleName}.Interfaces` (all modules)

# Rule changes

## MUST
- Exist as its own project, referenced only by `App.Host`
- Stay empty of concern-specific code — an extending solution adds its own folder, never this solution
- Never be referenced by any module `Application`, `Domain`, or `Api` project directly — only through abstractions in `Shared`
- Never contain any integration-specific code (persistence, cache, ...) as part of this solution

