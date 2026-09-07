---
description: Configure the logging provider and level filtering in the composition root
name: App.Host.csproj
element_kind: project
change_kind: extend
tags:
  - solution/app-logging
  - element/app-host-csproj
---

# Goals
- Own the logging provider choice and level configuration for the whole service.
- Expose one `AddAppLogging()` extension `Program.cs` calls.

# Core Principles
- `Program.cs` calls only the high-level `AddAppLogging(configuration)` extension.
- Provider and levels are set here and in `appsettings.json` — nowhere else.

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    LoggingRegistration.cs
  Program.cs
  appsettings.json
  appsettings.Development.json
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /DependencyInjection/LoggingRegistration.cs | `AddAppLogging()` — provider + level configuration |

# Implementation changes

`Program.cs`:
```csharp
builder.Services.AddAppLogging(builder.Configuration);
```

`appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

`appsettings.Development.json` may lower specific categories to `Debug`.

# NuGet Packages
| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
| Microsoft.Extensions.Logging | central | `ILoggingBuilder`, `ILogger<T>` |
| Microsoft.Extensions.Logging.Console | central | default console provider |

Versions live in `Directory.Packages.props`; references here are versionless.

# What Does NOT Belong Here
- Log call sites — those are in the class that logs.
- `LogEvents` catalogue — lives in `Shared`.

# Allowed Dependencies
- App.Host.DependencyInjection
- Microsoft.Extensions.Logging(.Console)

# Rules

## MUST
- Call `AddAppLogging()` exactly once, in `Program.cs`.
  - Risk: called twice, providers and filters are registered twice and every line is written twice.
  - Fix: one call in the composition root; nowhere else adds logging.
- Read levels from `IConfiguration` (`Logging` section), not hard-coded in `LoggingRegistration`.
  - Risk: changing a level needs a rebuild and redeploy instead of a config change.
  - Fix: `builder.AddConfiguration(configuration.GetSection("Logging"))`.

# Check list
- [ ] `AddAppLogging(builder.Configuration)` in `Program.cs`, once.
- [ ] `appsettings.json` has a `Logging:LogLevel` section.
- [ ] No `AddLogging`/`AddConsole` call outside `LoggingRegistration`.
