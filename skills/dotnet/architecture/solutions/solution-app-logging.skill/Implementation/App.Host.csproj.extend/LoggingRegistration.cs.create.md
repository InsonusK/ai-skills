---
description: The one place the logging provider and level filters are configured and a sink is swapped
name: LoggingRegistration.cs
element_kind: class
change_kind: create
tags:
  - solution/app-logging
  - element/logging-registration
---

# Goals
- Provide `AddAppLogging(this IServiceCollection, IConfiguration)` — the single logging composition point.
- Localise the provider choice so a file/OTLP sink is a one-line change here.

# Implementation

```csharp
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
            // Add a file / OTLP sink here later — no call site changes.
        });

        return services;
    }
}
```

# Allowed Dependencies
- Microsoft.Extensions.Logging
- Microsoft.Extensions.Logging.Console
- Microsoft.Extensions.Configuration.Abstractions

# Rules

## MUST
- `ClearProviders()` before adding the console provider.
  - Risk: the host's default providers stay registered and double-write every line.
  - Fix: clear first, then add exactly the providers this service wants.
- Bind levels from `configuration.GetSection("Logging")`.
  - Risk: hard-coded `SetMinimumLevel` ignores `appsettings` and cannot be changed per environment.
  - Fix: `AddConfiguration(configuration.GetSection("Logging"))`.
- Keep this the only method that names a logging provider.
  - Risk: a provider added elsewhere makes the effective sink set impossible to determine from one file.
  - Fix: every sink change happens inside this method.

## SHOULD
- Enable `IncludeScopes` so an ambient scope (module name, correlation id once tracing exists) prints with each line.

# Check list
- [ ] `AddAppLogging` clears providers, binds config, adds the console provider.
- [ ] The "add a sink here" comment marks the single extension point.
- [ ] No other file references a logging provider type.
