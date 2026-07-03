---
name: solution-middleware-registration
description: Defines the centralized HTTP middleware registration in App.Host — the single MiddlewareRegistration class and UseMiddlewarePipeline extension method where all custom HTTP middleware are registered in the correct pipeline order
domain: skill
type: architecture
version: 20260612
tags:
  - skill/architecture/solution
  - dotnet
  - aspnet-core
  - middleware
  - http-pipeline
  - "#draft"
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
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure.skill]]"
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
- definition of `module project structure` — [[skills/dotnet/architecture/solutions/🧩validated/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure.skill]] defines App.Host project and `/DependencyInjection` folder

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/solutions/🗑️deprecated/solution-middleware-registration.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - extend - Wire centralized HTTP middleware registration in the application pipeline
  - [[skills/dotnet/architecture/solutions/🗑️deprecated/solution-middleware-registration.skill/Implementation/App.Host.csproj.extend/MiddlewareRegistration.cs.create|MiddlewareRegistration.cs]] - create - Centralized HTTP middleware registration extension

# Rules

## MUST:
- [[./Implementation/App.Host.csproj.extend.md#MUST|App.Host.csproj.extend]]
	- [[./Implementation/App.Host.csproj.extend/MiddlewareRegistration.cs.create.md#MUST|MiddlewareRegistration.cs.create]]

## MUST NOT:
- [[./Implementation/App.Host.csproj.extend.md#MUST NOT|App.Host.csproj.extend]]
	- [[./Implementation/App.Host.csproj.extend/MiddlewareRegistration.cs.create.md#MUST NOT|MiddlewareRegistration.cs.create]]

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
