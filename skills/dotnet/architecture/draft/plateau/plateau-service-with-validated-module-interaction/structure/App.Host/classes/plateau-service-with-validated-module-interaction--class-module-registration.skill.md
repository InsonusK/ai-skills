---
name: class-module-registration
description: Class ModuleRegistration in the service-with-validated-module-interaction plateau
whenToUse: when creating or editing ModuleRegistration, or wiring up a newly added module
domain: skill
type: template
plateau: service-with-validated-module-interaction
version: 20260822140000
tags:
  - skill/template/class
  - plateau/service-with-validated-module-interaction
created_by:
  - "[[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
---

# Goal
- Provide a single extension method where every module is registered
- Keep `Program.cs` stable when new modules are added

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs.create]]
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

# Core Principles
- Apply ONE plateau template per class
- One `AddModules` extension per App.Host
- Each module's `Register{ModuleName}Module(configuration)` is called inside this method — that method itself is defined by `solution-command-integration`'s `{Module}ApplicationRegistration.cs` (see `class-module-application-registration`), not by this file
- New modules are added here — never directly in `Program.cs`

__Applied solutions:__
- [[../../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../../solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs.create]]
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj.extend]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Module registration | `ModuleRegistration` | `ModuleRegistration` | `ModuleRegistration.cs` | `ModuleRegistration.cs` |

# Implementation
```csharp
//Skill: class-module-registration
//Plateau: service-with-validated-module-interaction
//Version: 20260822140000

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
