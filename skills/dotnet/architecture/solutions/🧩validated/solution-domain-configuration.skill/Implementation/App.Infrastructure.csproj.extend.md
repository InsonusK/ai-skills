---
description: Add cross-module foreign key configuration support to App.Infrastructure
name: "App.Infrastructure.csproj"
element_kind: project
change_kind: extend
---

# Goals
- Host cross-module foreign key configurations that span multiple bounded contexts
- Register all module entity configurations via `ApplyConfigurationsFromAssembly` in AppDbContext

# Core Principles
- App.Infrastructure is the only place where cross-module foreign key relationships are configured
- DbContext uses `ApplyConfigurationsFromAssembly` to automatically discover all `IEntityTypeConfiguration<T>` implementations from module Domain assemblies
- App.Infrastructure references all module Domain projects to access entities for cross-module configuration

# Structure

## Project Structure
```
/App.Infrastructure
  /Persistence
    /Configurations
      CrossModuleFkConfig.cs
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

MUST:
- Register all configurations via `ApplyConfigurationsFromAssembly` scanning all module Domain assemblies in DbContext
- Place cross-module foreign key configurations in `/Persistence/Configurations`

MUST NOT:
- Define intra-module entity configurations here — those belong in `{Module}.Domain/Configurations`
- Register configurations manually one by one in `OnModelCreating`
- Reference BuildingBlocks directly

# Anti-patterns
- Putting module-internal entity configuration in App.Infrastructure — violates separation of concerns
- Manually registering each config class in `OnModelCreating` instead of using assembly scan

# Check list
- [ ] DbContext uses `ApplyConfigurationsFromAssembly` on all module Domain assemblies
- [ ] Cross-module FK configs live in `/Persistence/Configurations`
- [ ] No intra-module entity config placed in App.Infrastructure
