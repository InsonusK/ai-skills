---
description: Register IEntityVersionResolverFactory and module IEntityVersionResolver implementations in App.Host
name: App.Host.csproj
element_kind: project
change_kind: extend
tags:
  - solution/entity-concurrency-change
  - element/app-host-csproj
---

# Goals
- Register `IEntityVersionResolverFactory` and all module `IEntityVersionResolver` implementations

# Core Principles
- `EntityVersionResolverFactory` registered as `Scoped` — it creates `Scoped` resolvers that depend on `IReadRepository<T>`
- `EntityVersionResolverFactory` receives module Domain assemblies (validation) and module Application assemblies (resolver discovery) from App.Host

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    EntityVersionResolverRegistration.cs    ← created to register the factory and resolvers
```

## Directory and class skills
| Directory \ file | Description |
| ----------------- | ----------- |
| /DependencyInjection/EntityVersionResolverRegistration.cs | Register IEntityVersionResolverFactory and module resolvers |

# Allowed Dependencies
- Shared
- App.Infrastructure
- All module Application and Api projects

# Rules

## MUST
- `IEntityVersionResolverFactory` registered as `Scoped`
- `EntityVersionResolverFactory` receives all module Domain assemblies that contain versioned entities
- `EntityVersionResolverFactory` receives all module Application assemblies that contain `IEntityVersionResolver` implementations
- Register every module `IEntityVersionResolver` implementation as `Scoped`
- Pipeline behaviors registered via centralized `PipelineRegistration` in App.Host

## MUST NOT
- `IEntityVersionResolverFactory` registered as `Singleton`
- Change the signature of `RepositoryRegistration.AddRepositories`

# Anti-patterns
- Passing non-Domain or non-Application assemblies to `EntityVersionResolverFactory` — scans unrelated types
- Registering the factory as `Singleton` — creates captive dependencies on `Scoped` repositories

# Check list
- [ ] `IEntityVersionResolverFactory` registered as `Scoped`
- [ ] `EntityVersionResolverFactory` receives module Domain assemblies
- [ ] `EntityVersionResolverFactory` receives module Application assemblies
- [ ] All module `IEntityVersionResolver` implementations registered as `Scoped`

# Unittest TestCases
- [ ] WHEN applied THEN Register IEntityVersionResolverFactory and module IEntityVersionResolver implementations
