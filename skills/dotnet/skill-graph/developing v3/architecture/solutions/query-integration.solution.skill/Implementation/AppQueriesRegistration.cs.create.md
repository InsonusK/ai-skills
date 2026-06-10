---
description: App.Queries assembly scan registration
project_name: App.Queries
name: AppQueriesRegistration.cs
change_kind: create
---

# Goals
- Register all cross-module query handlers via assembly scan
- Called from App.Host — App.Queries does not self-register

# Core Principles
- `AddMediatR` scans the entire App.Queries assembly — discovers all `IRequestHandler` implementations automatically
- No separate registration step per handler — assembly scan covers them all

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| App.Queries DI registration | `AppQueriesRegistration` | `AppQueriesRegistration` | `AppQueriesRegistration.cs` | `AppQueriesRegistration.cs` |

# Implementation changes

```csharp
// App.Queries/AppQueriesRegistration.cs
using Microsoft.Extensions.DependencyInjection;

namespace App.Queries;

public static class AppQueriesRegistration
{
    public static IServiceCollection RegisterAppQueries(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(AppQueriesRegistration).Assembly));

        return services;
    }
}
```

# Rules

MUST:
- Register handlers via `AddMediatR` assembly scan
- Called from App.Host — not from any module registration
- Extension method named `RegisterAppQueries`

MUST NOT:
- Register individual handlers manually
- Register pipeline behaviors — behaviors are registered in App.Host
