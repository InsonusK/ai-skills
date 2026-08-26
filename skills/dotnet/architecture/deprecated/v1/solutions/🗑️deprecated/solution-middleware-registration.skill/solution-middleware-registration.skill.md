---
name: solution-middleware-registration
description: Defines the centralized HTTP middleware registration in App.Host — the single MiddlewareRegistration class and UseMiddlewarePipeline extension method where all custom HTTP middleware are registered in the correct pipeline order
domain: skill
type: architecture
version: 20260612
tags:
  - skill/architecture/solution
  - stack/dotnet
  - framework/aspnet-core
  - middleware
  - http-pipeline
  - "#draft"
  - concern/architecture

triggers:
  - register http middleware
  - add middleware registration
  - centralized middleware
  - http pipeline middleware
creates:
  - App.Host.DependencyInjection.MiddlewareRegistration.cs
extends:
  - App.Host.csproj
depends_on:
  - "[[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure.skill]]"
---

# Goal
- Define the centralized `MiddlewareRegistration` class in App.Host as the single place where all custom HTTP middleware are registered
- Ensure `UseMiddlewarePipeline()` is called from `Program.cs` in the correct middleware order
- Establish a clear extension point: each middleware solution extends `UseMiddlewarePipeline()` to insert its middleware

# Capabilities
- Single centralized extension point for all custom HTTP middleware
- Guaranteed correct pipeline order by registering middleware in one file
- Clear extension mechanism for future middleware solutions
- Prevention of scattered middleware registrations across modules and `Program.cs`
- Simplified review of HTTP pipeline composition

# Core Principles
- `MiddlewareRegistration.cs` is the single source of truth for custom HTTP middleware order
- Middleware registered in execution order
- New middleware is added by extending `UseMiddlewarePipeline()`, never by creating a second registration file
- `UseMiddlewarePipeline()` is called once from `Program.cs`, after routing and before endpoint mapping

# Requirements
- `Microsoft.AspNetCore.Http.Abstractions` NuGet package — provides `IApplicationBuilder` and `UseMiddleware<T>()`
- definition of `module project structure` — [[skills/dotnet/architecture/deprecated/v1/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure.skill]] defines App.Host project and `/DependencyInjection` folder

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/deprecated/v1/solutions/🗑️deprecated/solution-middleware-registration.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - extend - Wire centralized HTTP middleware registration in the application pipeline
  - [[skills/dotnet/architecture/deprecated/v1/solutions/🗑️deprecated/solution-middleware-registration.skill/Implementation/App.Host.csproj.extend/MiddlewareRegistration.cs.create|MiddlewareRegistration.cs]] - create - Centralized HTTP middleware registration extension

# Rules

## MUST:
- [[skills/dotnet/architecture/deprecated/v1/solutions/🗑️deprecated/solution-middleware-registration.skill/Implementation/App.Host.csproj.extend#MUST|App.Host.csproj]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🗑️deprecated/solution-middleware-registration.skill/Implementation/App.Host.csproj.extend/MiddlewareRegistration.cs.create#MUST|MiddlewareRegistration.cs]]

## MUST NOT:
- [[skills/dotnet/architecture/deprecated/v1/solutions/🗑️deprecated/solution-middleware-registration.skill/Implementation/App.Host.csproj.extend#MUST NOT|App.Host.csproj]]
	- [[skills/dotnet/architecture/deprecated/v1/solutions/🗑️deprecated/solution-middleware-registration.skill/Implementation/App.Host.csproj.extend/MiddlewareRegistration.cs.create#MUST NOT|MiddlewareRegistration.cs]]

# Anti-patterns
- Middleware order scattered across multiple files
- `UseMiddlewarePipeline()` duplicated or replaced by module-specific registration methods
- Registering middleware directly in `Program.cs` instead of inside `MiddlewareRegistration`

# Check list
- [ ] `MiddlewareRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] `UseMiddlewarePipeline()` extension method defined in `MiddlewareRegistration.cs`
- [ ] `UseMiddlewarePipeline()` called from `Program.cs`
- [ ] `UseMiddlewarePipeline()` called after routing and before `MapControllers()` / `MapEndpoints()`
- [ ] No custom middleware registrations outside `MiddlewareRegistration.cs`
