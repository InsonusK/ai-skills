---
name: plateau-service-with-api--class-api-registration
description: Class ApiRegistration in the service-with-api plateau
whenToUse: when wiring controller discovery, ProblemDetails, or per-module Swagger documents into the composition root
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
- Give App.Host one call site pair, `AddApi()`/`UseApi()`, for everything the HTTP layer needs registered

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs.create]]

# Core Principles
- `AddApi()`/`UseApi()` are the only call sites — `Program.cs` never calls `AddControllers()`/`AddSwaggerGen()` directly
- One `SwaggerDoc`/`SwaggerEndpoint`/`DocInclusionPredicate` arm per module that publishes HTTP
- Never assumes `GrpcRegistration`'s `AddGrpcApi()`/`UseGrpcApi()` were called

# Implementation
```csharp
//Skill: class-api-registration
//Plateau: service-with-api
//Version: 20260825120000

public static class ApiRegistration
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(TaskModuleApiSwaggerRegistration.DocumentName, new OpenApiInfo
            {
                Title = TaskModuleApiSwaggerRegistration.Title,
                Version = TaskModuleApiSwaggerRegistration.Version
            });
            options.DocInclusionPredicate((docName, apiDesc) => docName switch
            {
                TaskModuleApiSwaggerRegistration.DocumentName => TaskModuleApiSwaggerRegistration.MatchesRoute(apiDesc.RelativePath is null ? null : "/" + apiDesc.RelativePath),
                _ => false
            });
        });
        return services;
    }

    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.MapControllers();
        app.MapTaskImportEndpoints();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
            options.SwaggerEndpoint($"/swagger/{TaskModuleApiSwaggerRegistration.DocumentName}/swagger.json", TaskModuleApiSwaggerRegistration.Title));

        return app;
    }
}
```

One `SwaggerDoc`/`SwaggerEndpoint`/`DocInclusionPredicate` arm and one `MapXxxEndpoints()` call added per module that publishes HTTP — see [[../../../../../solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs.create]] for the full multi-module worked example.

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs.create]]

# Rules
MUST:
- `AddControllers()`/`AddSwaggerGen()`/`AddProblemDetails()` called only from `AddApi()`
- `UseExceptionHandler()` called before `MapControllers()`
MUST NOT:
- Register one solution-wide `v1` Swagger document

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs.create]]

# Check list
- [ ] `AddApi()`/`UseApi()` are the only call sites for the HTTP layer
- [ ] `UseExceptionHandler()` precedes `MapControllers()`

__Applied solutions:__
- [[../../../../../solutions/solution-http-api-publication.skill/solution-http-api-publication.skill.md|solution-http-api-publication]] - [[../../../../../solutions/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs.create]]
