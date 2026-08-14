---
description: Provide cross-module read model handlers that require JOIN queries across module boundaries
name: App.Queries.csproj
element_kind: project
change_kind: create
---

# Goals
- Provide cross-module read model handlers that require JOIN queries across module boundaries
- Be the only place where cross-module database joins are intentional and correct

# Core Principles
- App.Queries has direct DbContext access for cross-module JOINs
- Single-module queries belong in module Application — not here
- App.Queries implements query handlers declared in module Interfaces

# Structure

## Project Structure
```
/App.Queries
  /Queries
    /{ModuleName}
      GetTaskWithUserDetailsHandler.cs
  App.Queries.csproj
```

## Directory and class skills
| `Directory\|file`     | Description                                           |
| --------------------- | ----------------------------------------------------- |
| /Queries/{ModuleName} | Cross-module query handlers grouped by primary module |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Single-module queries — belong in module Application
- Write operations — belong in module Application handlers
- Business logic — belongs to Domain

# Allowed Dependencies
- App.Infrastructure (for DbContext access)
- {ModuleName}.Domain (all modules — for entity types in JOIN queries)
- {ModuleName}.Interfaces (all modules — for query and DTO contracts)

# Rules

## MUST
- Only cross-module JOIN queries live here
- Query handlers here implement contracts declared in module Interfaces
- All cross-module reads go through MediatR query dispatch or App.Queries

## MUST NOT
- App.Queries contain write operations
- App.Queries contain business logic
- Single-module queries be placed here
- App.Queries be referenced by module Application or Domain

# Anti-patterns
- Putting single-module queries in App.Queries — belongs in module Application
- Putting write operations in App.Queries — belongs in module Application

# Check list
- [ ] Only cross-module handlers present
- [ ] All handlers implement query contracts from module Interfaces
- [ ] No write operations in any handler
