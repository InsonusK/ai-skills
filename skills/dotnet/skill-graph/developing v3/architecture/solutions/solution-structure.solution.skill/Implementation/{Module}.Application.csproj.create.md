---
description: Orchestrate use cases by connecting the API contract to the domain model
name: "{Module}.Application.csproj"
change_kind: create
---

# Goals
- Orchestrate use cases by connecting the API contract to the domain model

# Core Principals
- Application coordinates — it never contains business logic
- Application knows its own Domain and its own Interfaces
- Application may reference other modules' Interfaces for cross-module dispatch

# Structure

## Project Structure
```
/{ModuleName}.Application
  /Handlers
  /Validators
  /Specifications
  {ModuleName}.Application.csproj
```

## Directory and class skills
| `Directory\|file` | Description                |
| ----------------- | -------------------------- |
| /Handlers         | Command and query handlers |
| /Validators       | Input validation           |
| /Specifications   | Query specifications       |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Business logic — belongs to Domain
- Infrastructure implementations — belongs to App.Infrastructure
- Cross-module JOIN queries — belongs to App.Queries

# Allowed Dependencies
- {Module}.Interfaces (own module)
- {Module}.Domain (own module)
- {OtherModule}.Interfaces (other modules — contracts only)
- Shared

# Rules

MUST:
- Application references only own Interfaces, own Domain

MUST NOT:
- Application reference another module's Domain
- Application reference another module's Application
- Application contain business logic — delegate to Domain

# Anti-patterns
- Calling another module's Application method directly — use MediatR dispatch through Interfaces
- Writing business rules in a handler — delegate to entity or domain service

# Check list
- [ ] Application.csproj does not reference another module's Domain or Application
- [ ] No business logic in any handler class
