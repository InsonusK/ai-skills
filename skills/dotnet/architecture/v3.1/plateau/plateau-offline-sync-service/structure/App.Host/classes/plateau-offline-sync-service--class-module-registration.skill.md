---
name: plateau-offline-sync-service--class-module-registration
description: Class ModuleRegistration in the plateau-offline-sync-service plateau — the one AddModules() extension where every module's Register{ModuleName}Module is called
whenToUse: when adding a new module to the composition root, or checking where module registration calls belong
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
---

# Goal
- Provide a single `AddModules()` extension where every module is registered, keeping `Program.cs` stable as modules are added.

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `static class ModuleRegistration`, one `AddModules(this IServiceCollection, IConfiguration) : IServiceCollection`.
- Calls each module's `Register{ModuleName}Module(configuration)` — added here, never in `Program.cs`.
- Registers no pipeline behavior and no infrastructure service.
- Lives under `App.Host/DependencyInjection`.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Module registration | `ModuleRegistration` | `ModuleRegistration` | `ModuleRegistration.cs` | `ModuleRegistration.cs` |

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-module-registration
// Plateau: core
// Version: 20260902000000
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class ModuleRegistration
{
    public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration)
    {
        services.Register{ModuleName}Module(configuration);
        // register additional modules here

        return services;
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs.create]]

# Rules
MUST:
- Class `ModuleRegistration`, method `AddModules`, accepting `IServiceCollection` + `IConfiguration`, under `App.Host/DependencyInjection`.
- Call every module's `Register{ModuleName}Module()` here — never in `Program.cs`, never from within another module.
- Never register a pipeline behavior or an infrastructure service.
- Never apply several plateau templates per class.

# Check list
- [ ] `AddModules(IServiceCollection, IConfiguration)` under `App.Host/DependencyInjection`.
- [ ] Every registered module is called here; `Program.cs` calls only `AddModules()`.

# Unittest TestCases
- [ ] WHEN `AddModules()` runs THEN each module's handlers/validators resolve from the provider.
