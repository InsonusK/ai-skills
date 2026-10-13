---
description: Wire centralized pipeline registration in the composition root
name: App.Host.csproj
element_kind: project
change_kind: extend
tags:
  - solution/pipeline-registration
  - element/app-host-csproj
---

# Goals
- Register the centralized `AddPipeline()` extension in the composition root
- Ensure `Program.cs` calls `AddPipeline()` exactly once

# Core Principles
- `Program.cs` only calls the high-level composition extension: `AddPipeline()`
- Pipeline behavior order is enforced inside `PipelineRegistration.AddPipeline()` — not in `Program.cs`

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
| /DependencyInjection/PipelineRegistration.cs | Centralized pipeline behavior registration |

# Implementation changes

Update `Program.cs` to call the pipeline composition extension:

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
- Never register `IPipelineBehavior<,>` directly in `Program.cs`
- Never call `AddPipeline()` more than once

## SHOULD
- Avoid registering behaviors directly in `Program.cs`
- Avoid calling `AddPipeline()` multiple times

# Check list
- [ ] `AddPipeline()` called from `Program.cs`
- [ ] No direct `IPipelineBehavior<,>` registration in `Program.cs`
