---
name: plateau-statefull-service--class-app-queries-registration
description: Class AppQueriesRegistration in the statefull-service plateau
whenToUse: when wiring cross-module query handlers into the composition root
domain: skill
type: template
plateau: statefull-service
version: 20260824100000
tags:
  - skill/template/class
  - plateau/statefull-service
created_by:
  - "[[../../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]]"
---

# Goal
- Register every cross-module query handler in `App.Queries` via one assembly scan

# Core Principles
- One static extension method, called last from `ModuleRegistration.AddModules()` — after every module registration, since App.Queries handlers reference module entity types

# Implementation
```csharp
//Skill: class-app-queries-registration
//Plateau: statefull-service
//Version: 20260824100000

public static class AppQueriesRegistration
{
    public static IServiceCollection RegisterAppQueries(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AppQueriesRegistration).Assembly));
        return services;
    }
}
```

# Rules
MUST:
- Register handlers via `AddMediatR` assembly scan for the App.Queries assembly
- Be called from `ModuleRegistration.AddModules()`, after every module registration
MUST NOT:
- Be called from inside any module registration method

# Check list
- [ ] `RegisterAppQueries()` registers handlers via assembly scan
- [ ] Called last, after every module registration

__Applied solutions:__
- [[../../../../../solutions/solution-query-integration.skill/solution-query-integration.skill.md|solution-query-integration]] - [[../../../../../solutions/solution-query-integration.skill/Implementation/App.Queries.csproj.extend/AppQueriesRegistration.cs.create.md|AppQueriesRegistration.cs.create]]
