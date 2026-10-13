---
description: Host the shared EventId catalogue for searched-for log lines
name: Shared.csproj
element_kind: project
change_kind: extend
tags:
  - solution/app-logging
  - element/shared-csproj
---

# Goals
- Give the milestone and critical log lines stable `EventId`s that survive a provider change.

# Structure

## Project Structure
```
/Shared
  /Logging
    LogEvents.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Logging/LogEvents.cs | Stable `EventId` constants |

# What Does NOT Belong Here
- The logging provider configuration — that is `App.Host/DependencyInjection/LoggingRegistration.cs`.
- Log call sites.

# Allowed Dependencies
- Microsoft.Extensions.Logging.Abstractions (for `EventId`)

# Rules

## MUST
- Keep `LogEvents` a `static class` of `EventId` constants only — no logging logic.
  - Risk: behaviour in `Shared` couples every project to a logging implementation detail.
  - Fix: constants only; the `ILogger<T>` call stays at the call site.

# Check list
- [ ] `Shared/Logging/LogEvents.cs` exists and contains only `EventId` fields.
