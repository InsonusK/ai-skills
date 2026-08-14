---
description: Add EF Core entity type configuration folder and pattern to module Domain project
name: "{Module}.Domain.csproj"
element_kind: project
change_kind: extend
---

# Goals
- Store all EF Core entity type configuration classes for this bounded context
- Own all persistence mapping concerns for this module's entities

# Core Principles
- One config class per entity — lives in /{Module}.Domain/Configurations

# Structure

## Project Structure
```
/{Module}.Domain
  /Configurations
    TodoTaskConfig.cs
    OrderConfig.cs
```

## Directory and class skills
| Directory \| file | Description                    | Pattern skill |
| ----------------- | ------------------------------ | ------------- |
| /Configurations   | One EF config class per entity |               |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Intra-module entity configurations — belong in respective `{Module}.Domain/Configurations`
- Domain entities — belong in `{Module}.Domain/Entities`
- Value Object definitions — belong in `{Module}.Domain/ValueObjects`
- Cross-module foreign key configurations — belong in App.Infrastructure/Persistence/Configurations

# Allowed Dependencies
- Shared
- Microsoft.EntityFrameworkCore (IEntityTypeConfiguration only)

# Rules

## MUST
- All EF configuration classes live in /{Module}.Domain/Configurations
- One config class per entity — no shared configs
- All configurations registered via `ApplyConfigurationsFromAssembly` in DbContext

## MUST NOT
- Place EF config classes outside /Configurations folder
- Use EF data annotations on domain entity classes
- Put mapping logic directly in `DbContext.OnModelCreating`
- Configure cross-module foreign keys here

# Anti-patterns
- Annotating domain entity with `[Column]`, `[Index]`, `[ForeignKey]` — all mapping belongs in config class
- Registering configs manually one by one in DbContext — use `ApplyConfigurationsFromAssembly`
- Sharing one config class across multiple entity types
- Placing cross-module FK config in Domain — belongs in App.Infrastructure

# Check list
- [ ] /Configurations folder exists in {Module}.Domain
- [ ] One config class per entity
- [ ] No EF attributes on any entity class in this module
- [ ] Configurations registered via `ApplyConfigurationsFromAssembly`
