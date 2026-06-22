---
uid: 51c69d92-d0ea-4848-9820-f245f43ccb08
name: moduleapplicationregistration-class
description: Register IGuidResolver in module DI
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration.solution.skill]]"
---

# Goal
- Register each `GuidResolver` in the module's DI registration
- Self-register all handlers and validators in this module's assembly via scan
- Give App.Host a single call surface for wiring up the module — no module internals exposed

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.extend.md|{Module}ApplicationRegistration.cs.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs.create]]

# Core Principals
- One static extension method per module — `Register{ModuleName}Module`
- `AddMediatR` scans the Application assembly — all `IRequestHandler` implementations registered automatically
- `AddValidatorsFromAssembly` scans the Application assembly — all `AbstractValidator<T>` registered automatically
- Pipeline behaviors NOT registered here — that is App.Host's responsibility

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.extend.md|{Module}ApplicationRegistration.cs.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Module registration | `{ModuleName}ApplicationRegistration` | `TaskApplicationRegistration` | `{ModuleName}ApplicationRegistration.cs` | `TaskApplicationRegistration.cs` |

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.extend.md|{Module}ApplicationRegistration.cs.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs.create]]

# Implementation
Module registration extended with `IGuidResolver` registrations:

```csharp
// {Module}.Application/{Module}ApplicationRegistration.cs
public static class {Module}ApplicationRegistration
{
    public static IServiceCollection Register{Module}Module(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof({Module}ApplicationRegistration).Assembly));

        services.AddValidatorsFromAssembly(
            typeof({Module}ApplicationRegistration).Assembly);

        // one registration per external-created entity type in this module
        services.AddScoped<
            IGuidResolver<Result<Create{Entity}Result>>,
            Create{Entity}GuidResolver>();

        return services;
    }
}
```

```csharp
// {Module}.Application/{Module}ApplicationRegistration.cs
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace {Module}.Application;

public static class {Module}ApplicationRegistration
{
    public static IServiceCollection Register{ModuleName}Module(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof({Module}ApplicationRegistration).Assembly));

        services.AddValidatorsFromAssembly(
            typeof({Module}ApplicationRegistration).Assembly);

        return services;
    }
}
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.extend.md|{Module}ApplicationRegistration.cs.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs.create]]

# Rules
MUST:
	- Each `IGuidResolver<TResponse>` registered explicitly as `Scoped` — not auto-scanned
	- One registration per external-created entity type
	- `TResponse` matches the command handler response type exactly
	- Method named `Register{ModuleName}Module`
	- Accept `IServiceCollection` and `IConfiguration`
	- Register handlers via `AddMediatR` assembly scan
	- Register validators via `AddValidatorsFromAssembly`
MUST NOT:
	- `IGuidResolver` registrations omitted — `GuidResolvingBehavior` will throw at runtime if resolver not found
	- `IGuidResolver` registered as open generic — breaks DI resolution per command result type
	- Register pipeline behaviors — belongs in App.Host
	- Register `DbContext` or infrastructure services — belongs in App.Infrastructure registration
	- Reference another module's Application assembly

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.extend.md|{Module}ApplicationRegistration.cs.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs.create]]

# Anti-patterns
- `IGuidResolver` registered as open generic — breaks DI resolution per command result type
- Resolver registered with mismatched `TResponse` — handler and resolver return different types

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.extend.md|{Module}ApplicationRegistration.cs.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs.create]]

# Check list
- [ ] `IGuidResolver<Result<Create{Entity}Result>>` registered as `Scoped`
- [ ] One registration per external-created entity type
- [ ] `TResponse` matches command handler response type

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.extend.md|{Module}ApplicationRegistration.cs.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Register each GuidResolver in the module's DI registration
- [ ] WHEN verified THEN IGuidResolver<Result<Create{Entity}Result>> registered as Scoped
- [ ] WHEN verified THEN One registration per external-created entity type
- [ ] WHEN verified THEN TResponse matches command handler response type
- [ ] WHEN applied THEN Self-register all handlers and validators in this module's assembly via scan
- [ ] WHEN applied THEN Give App.Host a single call surface for wiring up the module — no module internals exposed
- [ ] WHEN applied THEN One static extension method per module — Register{ModuleName}Module
- [ ] WHEN applied THEN AddMediatR scans the Application assembly — all IRequestHandler implementations registered automatically
- [ ] WHEN applied THEN AddValidatorsFromAssembly scans the Application assembly — all AbstractValidator<T> registered automatically
- [ ] WHEN applied THEN Pipeline behaviors NOT registered here — that is App.Host's responsibility
- [ ] WHEN naming 'Module registration' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.extend.md|{Module}ApplicationRegistration.cs.extend]]
- [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/skill-graph/developing v3/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Application.csproj.extend/{Module}ApplicationRegistration.cs.create.md|{Module}ApplicationRegistration.cs.create]]
