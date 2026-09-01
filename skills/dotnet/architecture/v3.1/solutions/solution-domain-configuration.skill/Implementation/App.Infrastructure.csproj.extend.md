---
description: Add cross-module foreign key configuration support to App.Infrastructure
name: "App.Infrastructure.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/domain-configuration
  - element/app-infrastructure-csproj
---

# Goals
- Host cross-module foreign key configurations that span multiple bounded contexts
- Register all module entity configurations via `ApplyConfigurationsFromAssembly` in AppDbContext

# Core Principles
- DbContext uses `ApplyConfigurationsFromAssembly` to automatically discover all `IEntityTypeConfiguration<T>` implementations from module Domain assemblies
- App.Infrastructure references all module Domain projects to access entities for cross-module configuration

# Implementation changes

**AS IS** (from `solution-infrastructure-project`, possibly already extended by `solution-repository-integration` with `/Persistence/AppDbContext.cs` and `/Repositories/Repository.cs`):
```
/App.Infrastructure
  App.Infrastructure.csproj
```

**TO BE** (after this solution):
```
/App.Infrastructure
  /Persistence
    /Configurations
      {Module1}To{Module2}Config.cs
  App.Infrastructure.csproj
```

## Directory and class skills
| Directory \| file           | Description                                              |
| --------------------------- | -------------------------------------------------------- |
| /Persistence/Configurations | Cross-module foreign key and relationship configurations |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Intra-module entity configurations — belong in respective `{Module}.Domain/Configurations`
- Domain entities — belong in `{Module}.Domain`
- Value Object definitions — belong in `{Module}.Domain/ValueObjects`

# Allowed Dependencies
- Shared
- {ModuleName}.Domain (all modules) — required to access entities and their configurations
- {ModuleName}.Interfaces (all modules)

# Rules

## MUST
- Register all configurations via `ApplyConfigurationsFromAssembly` scanning all module Domain assemblies in DbContext
- Place cross-module foreign key configurations in `/Persistence/Configurations`
- Never define intra-module entity configurations here — those belong in `{Module}.Domain/Configurations`
- Never register configurations manually one by one in `OnModelCreating`
- Never reference BuildingBlocks directly
- Never put mapping logic in `DbContext.OnModelCreating` directly

## SHOULD
- Avoid putting module-internal entity configuration in App.Infrastructure — violates separation of concerns
- Avoid manually registering each config class in `OnModelCreating` instead of using assembly scan

# Check list
- [ ] DbContext uses `ApplyConfigurationsFromAssembly` on all module Domain assemblies
- [ ] Cross-module FK configs live in `/Persistence/Configurations`
- [ ] No intra-module entity config placed in App.Infrastructure
