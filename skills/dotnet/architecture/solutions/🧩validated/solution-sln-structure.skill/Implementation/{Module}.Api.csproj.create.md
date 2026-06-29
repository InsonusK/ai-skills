---
description: Expose HTTP endpoints as thin MediatR adapters for this module
name: "{Module}.Api.csproj"
element_kind: project
change_kind: create
---

# Goals
- Expose HTTP endpoints as thin MediatR adapters for this module

# Core Principles
- Api is a thin adapter — no business logic, no domain rules
- Api references only its own Interfaces project for contracts

# Structure

## Project Structure
```
/{ModuleName}.Api
  /Controllers
  {ModuleName}.Api.csproj
```

## Directory and class skills
| `Directory\|file` | Description    |
| ----------------- | -------------- |
| /Controllers      | HTTP endpoints |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Business logic — belongs to Domain
- Infrastructure implementations — belongs to App.Infrastructure
- Handler implementations — belong to Application

# Allowed Dependencies
- {Module}.Interfaces (own module only)
- Shared

# Rules

MUST:
- Every endpoint dispatches exactly one MediatR command or query
- Api references only own Interfaces and BuildingBlocks

MUST NOT:
- Api reference Domain directly
- Api reference Application directly
- Api contain business logic, validation logic, or domain rules

# Anti-patterns
- Injecting a repository or DbContext into a controller — use MediatR dispatch only
- Writing business logic in a controller action — belongs in Domain
- Referencing Application project from Api — Api knows only Interfaces contracts

# Check list
- [ ] Api.csproj does not reference Domain
- [ ] Api.csproj does not reference Application
- [ ] Every controller action dispatches exactly one MediatR request
- [ ] No business logic in any controller
