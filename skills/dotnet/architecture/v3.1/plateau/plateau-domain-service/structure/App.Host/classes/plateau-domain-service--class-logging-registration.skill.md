---
name: plateau-domain-service--class-logging-registration
description: Class LoggingRegistration in the plateau-domain-service plateau — the one AddAppLogging() extension where the logging provider and level filters are configured
whenToUse: when changing the logging provider or level configuration, or adding a file/OTLP sink
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]]"
---

# Goal
- Provide `AddAppLogging(this IServiceCollection, IConfiguration)` — the single logging composition point — and localise the provider choice so a file/OTLP sink is a one-line change here.

__Applied solutions:__
- [[../../../../../solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]] - [[../../../../../solutions/solution-app-logging.skill/Implementation/App.Host.csproj.extend/LoggingRegistration.cs.create.md|LoggingRegistration.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `static class`, one `AddAppLogging(this IServiceCollection, IConfiguration) : IServiceCollection`.
- `ClearProviders()` first, then bind `configuration.GetSection("Logging")`, then add the console provider.
- This is the only method that names a logging provider; call sites use `ILogger<T>` and `LogEvents`.
- Lives under `App.Host/DependencyInjection`; called once from `Program.cs`.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Logging registration | `LoggingRegistration` | `LoggingRegistration` | `LoggingRegistration.cs` | `LoggingRegistration.cs` |

# Implementation
```csharp
// Skill: plateau-domain-service--class-logging-registration
// Plateau: core
// Version: 20260902000000
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Host.DependencyInjection;

public static class LoggingRegistration
{
    public static IServiceCollection AddAppLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddSimpleConsole(o =>
            {
                o.IncludeScopes = true;
                o.SingleLine = true;
                o.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
            });
            // Add a file / OTLP sink here later — no call-site changes.
        });

        return services;
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]] - [[../../../../../solutions/solution-app-logging.skill/Implementation/App.Host.csproj.extend/LoggingRegistration.cs.create.md|LoggingRegistration.cs.create]]

# Rules
MUST:
- `ClearProviders()` before adding a provider; bind levels from `configuration.GetSection("Logging")`, never hard-code `SetMinimumLevel`.
- Keep this the only method that names a logging provider.
- Live under `App.Host/DependencyInjection`; be called once from `Program.cs`.
- Never apply several plateau templates per class.

# Check list
- [ ] `AddAppLogging` clears providers, binds the `Logging` config section, adds the console provider.
- [ ] The "add a sink here" comment marks the single extension point.
- [ ] No other file references a logging provider type.

# Unittest TestCases
- [ ] WHEN `AddAppLogging` runs THEN exactly one logging provider is registered.
- [ ] WHEN `appsettings` sets a category level THEN the resolved `ILogger` honours it.
