---
name: class-module-registration
description: Centralized module registration extension
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure.skill]]"
---

# Goal
- Provide a single extension method where every module is registered
- Keep `Program.cs` stable when new modules are added

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs.create]]

# Core Principals
- One `AddModules` extension per App.Host
- Each module's `Register{ModuleName}Module(configuration)` is called inside this method
- New modules are added here — never directly in `Program.cs`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Module registration | `ModuleRegistration` | `ModuleRegistration` | `ModuleRegistration.cs` | `ModuleRegistration.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs.create]]

# Implementation
```csharp
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
- [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs.create]]

# Rules
MUST:
	- Class named `ModuleRegistration`
	- Method named `AddModules`
	- Accept `IServiceCollection` and `IConfiguration`
	- Live under `/App.Host/DependencyInjection`
	- Call every registered module's registration extension
MUST NOT:
	- Register pipeline behaviors — belongs in App.Host pipeline registration
	- Register infrastructure services — belongs in App.Infrastructure registration

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs.create]]

# Unittest TestCases
- [ ] WHEN component is requested THEN it provide a single extension method where every module is registered
- [ ] WHEN applied THEN Keep Program.cs stable when new modules are added
- [ ] WHEN applied THEN One AddModules extension per App.Host
- [ ] WHEN applied THEN Each module's Register{ModuleName}Module(configuration) is called inside this method
- [ ] WHEN applied THEN New modules are added here — never directly in Program.cs
- [ ] WHEN naming 'Module registration' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/solution-solution-structure.skill.md|solution-solution-structure]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-solution-structure.skill/Implementation/App.Host.csproj.create/ModuleRegistration.cs.create.md|ModuleRegistration.cs.create]]
