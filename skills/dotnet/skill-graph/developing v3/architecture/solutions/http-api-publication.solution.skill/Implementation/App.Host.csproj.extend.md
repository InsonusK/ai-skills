---
description: Wire API registration into the composition root
name: App.Host.csproj
change_kind: extend
---

# Goals
- Register controllers from all module Api assemblies
- Register Minimal API endpoint groups
- Configure ASP.NET Core JSON and ProblemDetails middleware

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    RepositoryRegistration.cs    ← repository-integration
    PipelineRegistration.cs      ← validation-behavior
    ApiRegistration.cs
  Program.cs
```

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /DependencyInjection/ApiRegistration.cs | Controller registration and middleware configuration | |

## NuGet Packages
| Package | Purpose |
| --- | --- |
| `Microsoft.AspNetCore` | `AddControllers`, `MapControllers`, `AddProblemDetails` |

## Allowed Dependencies
- All `{Module}.Api` projects — added as application parts

# Rules

MUST:
- `UseExceptionHandler()` registered before `MapControllers()` — unhandled exceptions produce `ProblemDetails`
- All module Api assemblies added as application parts
- All Minimal API endpoint groups mapped explicitly
- `AddProblemDetails()` registered in DI

MUST NOT:
- Register controllers manually one by one — use `AddApplicationPart` with assembly references

# Anti-patterns
- Missing `UseExceptionHandler()` before `MapControllers()`
- Forgetting to map Minimal API endpoint groups

# Check list
- [ ] `ApiRegistration.cs` exists in `/DependencyInjection`
- [ ] All module Api assemblies added as application parts
- [ ] `UseExceptionHandler()` before `MapControllers()`
- [ ] `AddProblemDetails()` registered
