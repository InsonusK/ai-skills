---
uid: 90a22d4c-6797-4ee2-bc99-84eb24852c5e
name: middleware-registration-solution
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
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill|solution-structure-solution.skill]]"
---

# Goal
- Define the centralized `MiddlewareRegistration` class in App.Host as the single place where all custom HTTP middleware are registered
- Ensure `UseMiddlewarePipeline()` is called from `Program.cs` in the correct middleware order
- Establish a clear extension point: each middleware solution extends `UseMiddlewarePipeline()` to insert its middleware

# Core Principles
- `MiddlewareRegistration.cs` is the single source of truth for custom HTTP middleware order
- Middleware registered in execution order
- New middleware is added by extending `UseMiddlewarePipeline()`, never by creating a second registration file
- `UseMiddlewarePipeline()` is called once from `Program.cs`, after routing and before endpoint mapping

# Requirements
- `Microsoft.AspNetCore.Http.Abstractions` NuGet package — provides `IApplicationBuilder` and `UseMiddleware<T>()`
- definition of `module project structure` — [[skills/dotnet/architecture/solutions/🧩validated/solution-structure-solution.skill/solution-structure-solution.skill|solution-structure-solution.skill]] defines App.Host project and `/DependencyInjection` folder

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/solutions/🗑️deprecated/middleware-registration-solution.skill/Implementation/App.Host.csproj.extend|App.Host.csproj]] - extend - Wire centralized HTTP middleware registration in the application pipeline
  - [[skills/dotnet/architecture/solutions/🗑️deprecated/middleware-registration-solution.skill/Implementation/App.Host.csproj.extend/MiddlewareRegistration.cs.create|MiddlewareRegistration.cs]] - create - Centralized HTTP middleware registration extension

# Rules

MUST:
- `MiddlewareRegistration.cs` defined in `App.Host/DependencyInjection/MiddlewareRegistration.cs`
- `UseMiddlewarePipeline()` called once from `Program.cs`
- All custom middleware registered inside `UseMiddlewarePipeline()` using `app.UseMiddleware<TMiddleware>()`
- Middleware registered in intended execution order
- `UseMiddlewarePipeline()` called after routing and before `MapControllers()` / `MapEndpoints()`

MUST NOT:
- Register custom middleware inside module registration methods
- Register custom middleware directly in `Program.cs`
- Change middleware order in multiple files
- Create multiple middleware registration extension methods

SHOULD:
- Keep `UseMiddlewarePipeline()` the only method that adds custom HTTP middleware registrations

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

# Unittest TestCases
- [ ] WHEN applied THEN `MiddlewareRegistration.cs` exists under `App.Host/DependencyInjection`
- [ ] WHEN applied THEN `UseMiddlewarePipeline()` is called from `Program.cs`
- [ ] WHEN applied THEN `UseMiddlewarePipeline()` returns `IApplicationBuilder`
- [ ] WHEN extended THEN middleware can be added inside `UseMiddlewarePipeline()` in execution order
