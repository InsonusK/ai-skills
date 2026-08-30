---
description: Per-module Swagger document metadata — DocumentName/Title/Version and a route-matching predicate, so this module gets its own definition in the Swagger UI dropdown instead of being merged into one solution-wide document
project_name: "{Module}.Api"
name: "{Module}ApiSwaggerRegistration.cs"
element_kind: class
change_kind: create
tags:
  - solution/http-api-publication
  - element/module-api-swagger-registration-cs
---

# Goals
- Let every module's routes appear as their own Swagger definition, so a solution with many modules doesn't collapse into one unreadable `v1` document
- Give `App.Host` a route-matching predicate it can delegate to, instead of hardcoding each module's route prefix in the composition root

# Implementation changes

```csharp
// {Module}.Api/TaskModuleApiSwaggerRegistration.cs
namespace {Module}.Api;

public static class TaskModuleApiSwaggerRegistration
{
    public const string DocumentName = "task";
    public const string Title = "Task Module API";
    public const string Version = "v1";

    public static bool MatchesRoute(string? relativePath)
        => relativePath is not null && relativePath.StartsWith("/api/tasks", StringComparison.OrdinalIgnoreCase);
}
```

Consumed from `App.Host`'s `ApiRegistration` — see [[skills/dotnet/architecture/v3/solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs]] for the `AddSwaggerGen`/`UseSwaggerUI` wiring that reads `DocumentName`/`Title`/`Version`/`MatchesRoute` from every module's registration class.

# Rule changes

## MUST
- Declare `DocumentName`, `Title`, `Version` as `public const string`
- `MatchesRoute` match every route this module's Controllers and Minimal API endpoints actually expose, and nothing from another module

## MUST NOT
- Let two modules' `MatchesRoute` overlap on the same route prefix

# Check list
- [ ] `DocumentName`/`Title`/`Version`/`MatchesRoute` all present
- [ ] `MatchesRoute` covers every route this module publishes, and no other module's routes
