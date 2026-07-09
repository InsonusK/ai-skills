---
name: class-app-queries-registration
description: App.Queries assembly scan registration
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]]"
---

# Goal
- Register all cross-module query handlers via assembly scan
- Called from App.Host — App.Queries does not self-register

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create|AppQueriesRegistration.cs]]

# Core Principles
- Apply ONE plateau template per class
- `AddMediatR` scans the entire App.Queries assembly — discovers all `IRequestHandler` implementations automatically
- No separate registration step per handler — assembly scan covers them all

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create|AppQueriesRegistration.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| App.Queries DI registration | `AppQueriesRegistration` | `AppQueriesRegistration` | `AppQueriesRegistration.cs` | `AppQueriesRegistration.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create|AppQueriesRegistration.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-app-queries-registration
//Plateau: default
//Version: 20260628
```

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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create|AppQueriesRegistration.cs]]

# Rules
MUST:
	- Register handlers via `AddMediatR` assembly scan
	- Called from App.Host — not from any module registration
	- Extension method named `RegisterAppQueries`
MUST NOT:
	- Register individual handlers manually
	- Register pipeline behaviors — behaviors are registered in App.Host

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create|AppQueriesRegistration.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Register all cross-module query handlers via assembly scan
- [ ] WHEN applied THEN Called from App.Host — App.Queries does not self-register
- [ ] WHEN applied THEN AddMediatR scans the entire App.Queries assembly — discovers all IRequestHandler implementations automatically
- [ ] WHEN applied THEN No separate registration step per handler — assembly scan covers them all
- [ ] WHEN naming 'App.Queries DI registration' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/solution-query-integration.skill|solution-query-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create|AppQueriesRegistration.cs]]
