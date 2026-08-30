---
description: Add the complete ordered pipeline behavior registrations to the centralized pipeline registration
name: App.Host.csproj
element_kind: project
change_kind: extend
tags:
  - solution/pipeline-registration-order
  - element/app-host-csproj
---

# Goals
- Extend the centralized `AddPipeline()` extension so it registers all pipeline behaviors in the canonical execution order
- Ensure `Program.cs` continues to call `AddPipeline()` exactly once

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs
  Program.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /DependencyInjection/PipelineRegistration.cs | Centralized pipeline behavior registration with ordered behaviors |

# Implementation changes

Update `Program.cs` to continue calling the pipeline composition extension once:

```csharp
// App.Host/Program.cs
builder.Services
    .AddPipeline();
```

# Allowed Dependencies
- App.Host.DependencyInjection

# Rules

## MUST
- `AddPipeline()` called in `Program.cs`
- `AddPipeline()` called exactly once
- Pipeline behaviors registered in App.Host — never inside a module's registration method

## MUST NOT
- Register `IPipelineBehavior<,>` directly in `Program.cs`
- Call `AddPipeline()` more than once

# Anti-patterns
- Registering behaviors directly in `Program.cs`
- Calling `AddPipeline()` multiple times

# Check list
- [ ] `AddPipeline()` called from `Program.cs`
- [ ] No direct `IPipelineBehavior<,>` registration in `Program.cs`
