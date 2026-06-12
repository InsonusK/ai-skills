---
description: Wire App.Queries registration into the composition root
name: App.Host.csproj
element_kind: project
change_kind: extend
---

# Goals
- Add `RegisterAppQueries()` call to the composition root alongside module registrations
- Ensure cross-module query handlers are discovered by MediatR

# Core Principles
- App.Host is the only composition root — all wiring happens here
- `RegisterAppQueries()` called after all module registrations — App.Queries depends on module entity types being registered
- Centralized in `ModuleRegistration.AddModules` or called directly from `Program.cs` if following the high-level extension pattern

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    ModuleRegistration.cs    ← solution-structure
    PipelineRegistration.cs  ← pipeline-registration
  Program.cs
```

# Implementation changes

Wire `RegisterAppQueries()` into the composition root. If following the centralized `AddModules` pattern, add it inside `ModuleRegistration.AddModules`:

```csharp
// App.Host/DependencyInjection/ModuleRegistration.cs
public static class ModuleRegistration
{
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterTaskModule(configuration);
        services.RegisterUserModule(configuration);
        // register additional modules here

        services.RegisterAppQueries();  // ← cross-module query handlers

        return services;
    }
}
```

Or call directly from `Program.cs` if not using centralized module registration:

```csharp
// App.Host/Program.cs
builder.Services
    .AddPipeline()
    .RegisterTaskModule(builder.Configuration)
    .RegisterUserModule(builder.Configuration)
    .RegisterAppQueries();              // ← cross-module query handlers
```

# Allowed Dependencies
- App.Queries — for `RegisterAppQueries()` extension
- All module Application projects — for `Register{ModuleName}Module()` extensions

# Rules

MUST:
- `RegisterAppQueries()` called from App.Host
- Called after all module registrations — App.Queries depends on module entity types

MUST NOT:
- Call `RegisterAppQueries()` from inside any module registration method

# Anti-patterns
- Calling `RegisterAppQueries()` before module registrations — module handlers and entity types may not be available
- Scattering App.Queries registration across multiple extension methods

# Check list
- [ ] `RegisterAppQueries()` called from App.Host
- [ ] Called after all module registrations
- [ ] Not called from within any module registration method
