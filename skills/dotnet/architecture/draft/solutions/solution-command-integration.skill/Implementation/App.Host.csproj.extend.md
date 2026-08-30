---
description: Wire module registrations in the composition root
name: App.Host.csproj
element_kind: project
change_kind: extend
tags:
  - solution/command-integration
  - element/app-host-csproj
---

# Goals
- Ensure module registrations are called via the centralized `AddModules()` extension in `Program.cs`

# Core Principles
- `Program.cs` calls `AddModules()` — the centralized registration point created by solution-structure
- Each module's `Register{ModuleName}Module(configuration)` is called inside `ModuleRegistration.AddModules`

# Implementation changes

**AS IS** (from `plateau-stateless-non-interactive-service`, via `solution-sln-structure`) — `ModuleRegistration.AddModules()` already exists as the centralized extension point and is already called once from `Program.cs`; it does not yet register this module:
```csharp
// App.Host/Program.cs
builder.Services
    .AddModules(builder.Configuration)
    .AddPipeline();
```

**TO BE** (after this solution) — `Program.cs` itself is unchanged; `ModuleRegistration.AddModules()` gains a call to the new module's self-registration extension:
```csharp
// App.Host/DependencyInjection/ModuleRegistration.cs
public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration)
{
    services.Register{ModuleName}Module(configuration);
    return services;
}
```

# Allowed Dependencies
- `{Module}.Application` for module registration extensions
- App.Infrastructure

# Rules

## MUST
- `AddModules()` called in `Program.cs`
- All module `Register{ModuleName}Module()` calls made inside `ModuleRegistration.AddModules`

## MUST NOT
- Call individual `Register{ModuleName}Module()` methods directly from `Program.cs`
- Module registration methods called from within another module
- Add module registration calls outside `ModuleRegistration.AddModules`

# Anti-patterns
- `Program.cs` listing every module explicitly — centralize module calls in `ModuleRegistration`
- Scattering module registration across multiple extension methods called from `Program.cs`

# Check list
- [ ] `AddModules(builder.Configuration)` called in `Program.cs`
- [ ] Every registered module is added inside `ModuleRegistration.AddModules`
