---
description: Provide cross-cutting primitives that every layer can safely depend on without creating coupling
name: Shared.csproj
change_kind: create
---

# Goals
- Provide cross-cutting primitives that every layer can safely depend on without creating coupling
- Define base types used across module and infrastructure boundaries

# Core Principals
- Shared has no business logic — only framework-level primitives
- Shared has no dependencies on any other project in this solution
- Any project at any layer may depend on Shared

# Structure

## Project Structure
```
/Shared
  /Events
    IDomainEvent.cs
  /Exceptions
    DomainException.cs
    ConflictException.cs
  Shared.csproj
```

## Directory and class skills
| `Directory\|file` | Description                               |
| ----------------- | ----------------------------------------- |
| /Events           | Base event interfaces                     |
| /Exceptions       | Shared exception types used across layers |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Business logic — belongs to Domain
- Pipeline behaviors — belongs to BuildingBlocks
- Infrastructure implementations — belongs to App.Infrastructure

# Allowed Dependencies
- None — Shared has no project dependencies

# Rules

MUST:
- Shared has zero project references
- All types in Shared are purely cross-cutting primitives

MUST NOT:
- Shared reference any module, BuildingBlocks, or infrastructure project
- Shared contain business logic or domain rules

# Anti-patterns
- Placing domain entities in Shared — they belong in module Domain
- Placing pipeline behaviors in Shared — they belong in BuildingBlocks
- Adding project references to Shared.csproj

# Check list
- [ ] Shared.csproj has no project references
- [ ] No business logic in any Shared class
