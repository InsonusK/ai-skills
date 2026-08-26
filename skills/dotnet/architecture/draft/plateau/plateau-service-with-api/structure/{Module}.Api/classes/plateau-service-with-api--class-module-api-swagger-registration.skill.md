---
name: plateau-service-with-api--class-module-api-swagger-registration
description: Class {Module}ApiSwaggerRegistration in the service-with-api plateau
whenToUse: when a module's routes need their own Swagger document instead of being merged into one solution-wide document
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/class
  - plateau/service-with-api
created_by:
  - "[[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]]"
---

# Goal
- Give every module its own Swagger document and a route-matching predicate `App.Host` delegates to

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Module}ApiSwaggerRegistration.cs.create.md|{Module}ApiSwaggerRegistration.cs.create]]

# Implementation
```csharp
//Skill: class-module-api-swagger-registration
//Plateau: service-with-api
//Version: 20260825120000

public static class TaskModuleApiSwaggerRegistration
{
    public const string DocumentName = "task";
    public const string Title = "Task Module API";
    public const string Version = "v1";

    public static bool MatchesRoute(string? relativePath)
        => relativePath is not null && relativePath.StartsWith("/api/tasks", StringComparison.OrdinalIgnoreCase);
}
```

Consumed from `App.Host`'s `ApiRegistration` — see [[../../App.Host/classes/plateau-service-with-api--class-api-registration.skill.md|class-api-registration]].

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Module}ApiSwaggerRegistration.cs.create.md|{Module}ApiSwaggerRegistration.cs.create]]

# Rules
MUST:
- `MatchesRoute` cover every route this module publishes, and no other module's routes

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Module}ApiSwaggerRegistration.cs.create.md|{Module}ApiSwaggerRegistration.cs.create]]

# Check list
- [ ] `DocumentName`/`Title`/`Version`/`MatchesRoute` all present
- [ ] No overlap with another module's `MatchesRoute`

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/{Module}.Api.csproj.extend/{Module}ApiSwaggerRegistration.cs.create.md|{Module}ApiSwaggerRegistration.cs.create]]
