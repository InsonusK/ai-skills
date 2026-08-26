---
description: Wire API registration into the composition root
name: App.Host.csproj
element_kind: project
change_kind: extend
tags:
  - solution/http-api-publication
  - element/app-host-csproj
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
    PipelineRegistration.cs      ← pipeline-registration
    ApiRegistration.cs
  Program.cs
```

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /DependencyInjection/ApiRegistration.cs | Controller registration and middleware configuration | |

#

# NuGet Packages
| Package | Purpose |
| --- | --- |
| `Microsoft.AspNetCore` | `AddControllers`, `MapControllers`, `AddProblemDetails` |

#

# Allowed Dependencies
- All `{Module}.Api` projects — added as application parts

# Rules

## MUST
- `UseExceptionHandler()` registered before `MapControllers()` — unhandled exceptions produce `ProblemDetails`
- All module Api assemblies added as application parts
- All Minimal API endpoint groups mapped explicitly
- `AddProblemDetails()` registered in DI
- `App.Host` registers one `SwaggerDoc` per module using the module's `{Module}ApiSwaggerRegistration` constants
- `App.Host` provides a single `DocInclusionPredicate` that delegates route matching to `{Module}ApiSwaggerRegistration.MatchesRoute`
- `App.Host` registers one `SwaggerEndpoint` per module in `UseSwaggerUI`

## MUST NOT
- Register controllers manually one by one — use `AddApplicationPart` with assembly references
- Declare Swagger document metadata (document name, title, version, route matching) in `App.Host`

# Anti-patterns
- Missing `UseExceptionHandler()` before `MapControllers()`
- Forgetting to map Minimal API endpoint groups

# Check list
- [ ] `ApiRegistration.cs` exists in `/DependencyInjection`
- [ ] All module Api assemblies added as application parts
- [ ] `UseExceptionHandler()` before `MapControllers()`
- [ ] `AddProblemDetails()` registered
