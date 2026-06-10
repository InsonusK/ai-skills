---
description: Wire pipeline behaviors and centralized module registration in the composition root
name: App.Host.csproj
change_kind: extend
---

# Goals
- Register MediatR pipeline behaviors in the correct order — this is the only place pipeline order is defined
- Centralize module registration behind a single `AddModules` extension so `Program.cs` does not change when adding new modules

# Core Principles
- Pipeline behaviors registered as open generics with `Transient` lifetime
- Pipeline registration order is enforced here — behaviors execute in registration order
- Every registered module must be added inside `ModuleRegistration.AddModules`
- `Program.cs` only calls the high-level composition extensions: `AddPipeline()` and `AddModules()`

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs
    ModuleRegistration.cs
  Program.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /DependencyInjection/PipelineRegistration.cs | Pipeline behavior registration in correct order |
| /DependencyInjection/ModuleRegistration.cs | Centralized module registration extension |

# Implementation changes

Update `Program.cs` to call the high-level composition extensions:

```csharp
// App.Host/Program.cs
builder.Services
    .AddPipeline()
    .AddModules(builder.Configuration);
```

# Allowed Dependencies
- BuildingBlocks
- `{Module}.Application` for module registration extensions
- App.Infrastructure

# Rules

MUST:
- `AddPipeline()` called in `Program.cs`
- `AddModules()` called in `Program.cs`
- All module `Register{ModuleName}Module()` calls made inside `ModuleRegistration.AddModules`
- `ModuleRegistration.cs` live under `/App.Host/DependencyInjection`
- Pipeline behaviors registered as open generics with `AddTransient`
- Pipeline registration order defined here — not in any module

MUST NOT:
- Call individual `Register{ModuleName}Module()` methods directly from `Program.cs`
- Module registration methods called from within another module
- Pipeline behaviors registered inside any module's registration method
- Add module registration calls outside `ModuleRegistration.AddModules`

# Anti-patterns
- `Program.cs` listing every module explicitly — centralize module calls in `ModuleRegistration`
- Defining pipeline order in multiple places
- Scattering module registration across multiple extension methods called from `Program.cs`

# Check list
- [ ] `AddPipeline()` called in `Program.cs`
- [ ] `AddModules(builder.Configuration)` called in `Program.cs`
- [ ] `PipelineRegistration.cs` exists under `/DependencyInjection`
- [ ] `ModuleRegistration.cs` exists under `/DependencyInjection`
- [ ] Every registered module is added inside `ModuleRegistration.AddModules`
