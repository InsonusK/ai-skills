---
description: Add InfrastructureRegistration.cs and wire AddInfrastructure() into Program.cs
name: App.Host.csproj
element_kind: project
change_kind: extend
tags:
  - solution/infrastructure-project
  - element/app-host-csproj
---

# Goals
- Give the composition root a dedicated, centralized extension point for infrastructure registration, parallel to `AddModules()`/`AddPipeline()`

# Implementation changes

**AS IS** (the state after `solution-sln-structure`):
```
/App.Host
  /DependencyInjection
    ModuleRegistration.cs
    PipelineRegistration.cs
  Program.cs
  App.Host.csproj
```
```csharp
// App.Host/Program.cs
builder.Services
    .AddModules(builder.Configuration)
    .AddPipeline();
```
Allowed Dependencies: `{ModuleName}.Api`/`{ModuleName}.Application` (all modules), `BuildingBlocks`. No `App.Infrastructure` reference.

**TO BE** (after this solution):
```
/App.Host
  /DependencyInjection
    ModuleRegistration.cs
    PipelineRegistration.cs
    InfrastructureRegistration.cs
  Program.cs
  App.Host.csproj
```
```csharp
// App.Host/DependencyInjection/InfrastructureRegistration.cs
public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Concern-specific solutions (e.g. solution-repository-integration) extend this method.
        return services;
    }
}
```
```csharp
// App.Host/Program.cs
builder.Services
    .AddModules(builder.Configuration)
    .AddPipeline()
    .AddInfrastructure(builder.Configuration);
```
Allowed Dependencies: adds `App.Infrastructure`.

# Rule changes

## MUST
- `InfrastructureRegistration.cs` exist under `App.Host/DependencyInjection`, exposing `AddInfrastructure()`
- `AddInfrastructure()` called exactly once from `Program.cs`
- Never concern-specific registration code (a `DbContext`, a cache client, ...) added directly inside `AddInfrastructure()` by this solution — that belongs to whichever solution extends this method for its own concern
- Never `AddInfrastructure()` called more than once, or from anywhere other than `Program.cs`

