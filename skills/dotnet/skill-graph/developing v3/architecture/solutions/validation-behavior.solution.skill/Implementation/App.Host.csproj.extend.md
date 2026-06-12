---
description: Wire pipeline behavior registration in the composition root
name: App.Host.csproj
element_kind: project
change_kind: extend
---

# Goals
- Register MediatR pipeline behaviors in the single correct order
- Ensure `AddPipeline()` is called in `Program.cs`

# Core Principles
- Pipeline behaviors registered as open generics with `Transient` lifetime
- Pipeline registration order is enforced in `PipelineRegistration` — behaviors execute in registration order
- `Program.cs` only calls the high-level composition extension: `AddPipeline()`

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
| /DependencyInjection/PipelineRegistration.cs | Pipeline behavior registration in correct order |

# Implementation changes

Update `Program.cs` to call the pipeline composition extension:

```csharp
// App.Host/Program.cs
builder.Services
    .AddPipeline();
```

# Allowed Dependencies
- BuildingBlocks

# Rules

MUST:
- `AddPipeline()` called in `Program.cs`
- Pipeline behaviors registered as open generics with `AddTransient`
- Pipeline registration order defined in `PipelineRegistration.cs` — not in any module

MUST NOT:
- Register behaviors inside module registration methods
- Define pipeline order in multiple files

# Anti-patterns
- Defining pipeline order in multiple places
- Scattering pipeline registration across multiple extension methods

# Check list
- [ ] `AddPipeline()` called in `Program.cs`
- [ ] `PipelineRegistration.cs` exists under `/DependencyInjection`
