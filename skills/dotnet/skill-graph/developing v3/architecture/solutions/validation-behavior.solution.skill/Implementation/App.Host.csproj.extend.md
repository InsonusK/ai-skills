---
description: Add ValidationBehavior to the centralized pipeline registration
name: App.Host.csproj
element_kind: project
change_kind: extend
---

# Goals
- Register `ValidationBehavior` in the centralized pipeline registration
- Ensure `AddPipeline()` from [[pipeline-registration.solution.skill]] includes `ValidationBehavior`

# Core Principles
- Pipeline behaviors registered as open generics with `Transient` lifetime
- Pipeline registration order is enforced in `PipelineRegistration` — behaviors execute in registration order
- `ValidationBehavior` registered first in `AddPipeline()`

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    PipelineRegistration.cs      ← extended with ValidationBehavior
  Program.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /DependencyInjection/PipelineRegistration.cs | Extended with ValidationBehavior registration |

# Implementation changes

`AddPipeline()` is provided by [[pipeline-registration.solution.skill]]. This solution extends it to register `ValidationBehavior`:

```csharp
// App.Host/DependencyInjection/PipelineRegistration.cs
services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));
```

`Program.cs` calls `AddPipeline()` as defined by [[pipeline-registration.solution.skill]]:

```csharp
// App.Host/Program.cs
builder.Services
    .AddPipeline();
```

# Allowed Dependencies
- BuildingBlocks

# Rules

MUST:
- `ValidationBehavior` registered as the first `IPipelineBehavior` entry in `AddPipeline()`
- Pipeline behaviors registered as open generics with `AddTransient`
- Pipeline registration order defined in `PipelineRegistration.cs` — not in any module

MUST NOT:
- Register behaviors inside module registration methods
- Define pipeline order in multiple files

# Anti-patterns
- Defining pipeline order in multiple places
- Scattering pipeline registration across multiple extension methods

# Check list
- [ ] `ValidationBehavior` registered first in `PipelineRegistration.cs`
- [ ] `AddPipeline()` called from `Program.cs`
- [ ] Pipeline behaviors registered as open generics with `AddTransient`
