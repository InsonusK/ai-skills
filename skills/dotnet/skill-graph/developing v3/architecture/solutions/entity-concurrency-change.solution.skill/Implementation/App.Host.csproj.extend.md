---
description: Register IEntityVersionResolver in App.Host
name: App.Host.csproj
element_kind: project
change_kind: extend
---

# Goals
- Register `IEntityVersionResolver` as `Singleton` with module Domain assemblies

# Core Principles
- `EntityVersionResolver` registered as `Singleton` — map is built once at startup, safe for singleton lifetime
- `EntityVersionResolver` receives module Domain assemblies from App.Host — the only project that references all modules

# Structure

## Project Structure
```
/App.Host
  /DependencyInjection
    EntityVersionResolverRegistration.cs    ← created to register EntityVersionResolver
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /DependencyInjection/EntityVersionResolverRegistration.cs | Register IEntityVersionResolver as Singleton with module Domain assemblies |

# Allowed Dependencies
- Shared
- App.Infrastructure
- All module Application and Api projects

# Rules

MUST:
- `EntityVersionResolver` registered as `Singleton`
- `EntityVersionResolver` receives all module Domain assemblies that contain versioned entities

MUST NOT:
- Change the signature of `RepositoryRegistration.AddRepositories`

# Anti-patterns
- Passing non-Domain assemblies to `EntityVersionResolver` — scans unrelated types

# Check list
- [ ] `EntityVersionResolver` registered as `Singleton`
- [ ] `EntityVersionResolver` receives module Domain assemblies

# Unittest TestCases
- [ ] WHEN applied THEN Register IEntityVersionResolver as Singleton with module Domain assemblies
