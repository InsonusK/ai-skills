---
description: Centralized module registration extension
project_name: App.Host
name: ModuleRegistration.cs
change_kind: create
---

# Goals
- Provide a single extension method where every module is registered
- Keep `Program.cs` stable when new modules are added

# Core Principles
- One `AddModules` extension per App.Host
- Each module's `Register{ModuleName}Module(configuration)` is called inside this method
- New modules are added here — never directly in `Program.cs`

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    ModuleRegistration.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Module registration | `ModuleRegistration` | `ModuleRegistration` | `ModuleRegistration.cs` | `ModuleRegistration.cs` |

# Implementation changes

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
