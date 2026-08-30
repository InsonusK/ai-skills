---
name: plateau-stateless-non-interactive-service--class-module-registration
description: Class ModuleRegistration in the stateless-non-interactive-service plateau
whenToUse: when creating or editing ModuleRegistration, or wiring up a newly added module
domain: skill
type: template
plateau: stateless-non-interactive-service
version: 20260821120000
tags:
  - skill/template/class
  - plateau/stateless-non-interactive-service
created_by:
  - "[[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
---

# Goal
- Provide a single extension method where every module is registered
- Keep `Program.cs` stable when new modules are added

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- One `AddModules` extension per App.Host
- Each module's `Register{ModuleName}Module(configuration)` is called inside this method
- New modules are added here — never directly in `Program.cs`

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Module registration | `ModuleRegistration` | `ModuleRegistration` | `ModuleRegistration.cs` | `ModuleRegistration.cs` |

# Implementation
```csharp
//Skill: class-module-registration
//Plateau: stateless-non-interactive-service
//Version: 20260821120000

// App.Host/DependencyInjection/ModuleRegistration.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Host.DependencyInjection;

public static class ModuleRegistration
{
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration)
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
- Class named `ModuleRegistration`, method named `AddModules`
- Accept `IServiceCollection` and `IConfiguration`
- Live under `/App.Host/DependencyInjection`
- Call every registered module's registration extension
MUST NOT:
- Register pipeline behaviors — belongs in App.Host pipeline registration
- Register infrastructure services — belongs in App.Infrastructure registration

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs.create]]

# Check list
- [ ] `AddModules` calls every registered module's `Register{ModuleName}Module`
- [ ] New modules are added here, never directly in `Program.cs`

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs.create]]

# Unittest TestCases
- [ ] WHEN a module is registered THEN `AddModules` calls its `Register{ModuleName}Module(configuration)`

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs.create]]
