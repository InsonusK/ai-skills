---
description: Own the entities, value objects, rules, and domain events for this bounded context
name: "{Module}.Domain.csproj"
element_kind: project
change_kind: create
tags:
  - solution/sln-structure
  - element/module-domain-csproj
---
# Goals
- Own the entities, value objects, rules, and domain events for this bounded context
- Store all entity types for this bounded context
- Own the business logic and invariant enforcement for all entities in this module

# Core Principles
- All entities live in /{Module}.Domain/Entities
- Domain is the only layer that contains entity definitions

# Structure

## Project Structure
```
/{Module}.Domain
  /Entities
    InternalImmutableEntity.cs
    InternalMutableEntity.cs
    ExternalImmutableEntity.cs
    ExternalMutableEntity.cs
```

## Directory and class skills
| Directory \| file | Description                      |
| ----------------- | -------------------------------- |
| /Entities         | All entity types for this module |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| Microsoft.EntityFrameworkCore | * | IEntityTypeConfiguration only |

# What Does NOT Belong Here
- Business logic orchestration — belongs to Application
- Infrastructure implementations — belongs to App.Infrastructure
- Cross-module queries — belongs to App.Queries

# Allowed Dependencies
- Shared
- Microsoft.EntityFrameworkCore (IEntityTypeConfiguration only)

# Rules

## MUST
- Domain depends only on Shared and EF Core (for IEntityTypeConfiguration only)
- All entities live in /{Module}.Domain/Entities

## MUST NOT
- Domain reference any other module's project
- Domain use EF Core beyond IEntityTypeConfiguration

# Anti-patterns
- Injecting DbContext into a domain class — domain has no persistence dependency
- Referencing another module's Domain for shared entity types — each module owns its own entities
- Using EF Core attributes on domain entities — use configuration classes instead
- Placing entities outside /Entities folder — breaks navigation and discoverability
- Defining entities in Application or Interfaces — entities belong in Domain only

# Check list
- [ ] Domain.csproj references only EF Core
- [ ] No DbContext reference in any domain class
- [ ] No cross-module domain references
- [ ] /Entities folder exists in {Module}.Domain
- [ ] All entity classes placed in /Entities
