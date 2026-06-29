---
description: Be the single composition root — wire all modules, infrastructure, pipeline behaviors, and DI registrations together
name: App.Host.csproj
element_kind: project
change_kind: create
---
# Goals
- Be the single composition root — wire all modules, infrastructure, pipeline behaviors, and DI registrations together
- Be the only project that knows about all other projects simultaneously

# Core Principles
- App.Host is the only composition root — it wires everything together
- App.Host references BuildingBlocks directly; Shared is consumed transitively through BuildingBlocks
- App.Host contains no business logic — only wiring
- Pipeline behaviors are registered once here — not inside individual modules

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    ModuleRegistration.cs
    PipelineRegistration.cs
    InfrastructureRegistration.cs
  Program.cs
  App.Host.csproj
```

## Directory and class skills
| `Directory\|file`    | Description                         |
| -------------------- | ----------------------------------- |
| /DependencyInjection | DI registrations and pipeline setup |
| ModuleRegistration.cs | Centralized module registration extension |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Business logic — belongs to Domain
- Handler implementations — belong to module Application
- Infrastructure implementations — belong to App.Infrastructure

# Allowed Dependencies
- {ModuleName}.Api (all modules)
- {ModuleName}.Application (all modules — for registration methods)
- App.Infrastructure
- App.Queries
- BuildingBlocks

# Rules

MUST:
- Pipeline behaviors registered once here in correct order
- Each module registration method called here
- App.Host is the only project referencing all modules simultaneously

MUST NOT:
- App.Host contain business logic
- App.Host contain handler implementations

# Anti-patterns
- Putting business logic in App.Host — wiring only
- Putting handler implementations in App.Host — belong in module Application
- Registering pipeline behaviors inside module registration — register once in App.Host

# Check list
- [ ] All module registration methods called
- [ ] Pipeline behaviors registered in correct order
- [ ] No business logic in App.Host
