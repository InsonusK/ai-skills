---
description: AddApi() — the single App.Host extension point wiring controller discovery, ProblemDetails, and a per-module Swagger document per module's ApiSwaggerRegistration
project_name: "App.Host"
name: "ApiRegistration.cs"
element_kind: class
change_kind: create
tags:
  - solution/http-api-publication
  - element/api-registration-cs
---

# Goals
- Give App.Host one call site, `AddApi()`, for everything the HTTP API layer needs registered — mirroring `AddModules()`/`AddPipeline()`'s centralized-extension-point discipline
- Wire one Swagger document per module instead of one solution-wide `v1` document, so a large solution's Swagger UI stays navigable

# Core Principles
- `AddApi()` is the only call site — `Program.cs` never calls `AddControllers()`/`AddSwaggerGen()` directly
- Every module's `{Module}ApiSwaggerRegistration` is imported once here and contributes one `SwaggerDoc` + one `SwaggerEndpoint`, matched by its own `MatchesRoute`

# Implementation changes

```csharp
// App.Host/DependencyInjection/ApiRegistration.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using TagModule.Api;
using TaskModule.Api;
using TimeLogModule.Api;

namespace App.Host.DependencyInjection;

public static class ApiRegistration
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(TagModuleApiSwaggerRegistration.DocumentName, new OpenApiInfo
            {
                Title = TagModuleApiSwaggerRegistration.Title,
                Version = TagModuleApiSwaggerRegistration.Version
            });
            options.SwaggerDoc(TaskModuleApiSwaggerRegistration.DocumentName, new OpenApiInfo
            {
                Title = TaskModuleApiSwaggerRegistration.Title,
                Version = TaskModuleApiSwaggerRegistration.Version
            });
            options.SwaggerDoc(TimeLogModuleApiSwaggerRegistration.DocumentName, new OpenApiInfo
            {
                Title = TimeLogModuleApiSwaggerRegistration.Title,
                Version = TimeLogModuleApiSwaggerRegistration.Version
            });

            options.DocInclusionPredicate((docName, apiDesc) => docName switch
            {
                TagModuleApiSwaggerRegistration.DocumentName => TagModuleApiSwaggerRegistration.MatchesRoute(apiDesc.RelativePath is null ? null : "/" + apiDesc.RelativePath),
                TaskModuleApiSwaggerRegistration.DocumentName => TaskModuleApiSwaggerRegistration.MatchesRoute(apiDesc.RelativePath is null ? null : "/" + apiDesc.RelativePath),
                TimeLogModuleApiSwaggerRegistration.DocumentName => TimeLogModuleApiSwaggerRegistration.MatchesRoute(apiDesc.RelativePath is null ? null : "/" + apiDesc.RelativePath),
                _ => false
            });
        });

        return services;
    }

    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.MapControllers();
        app.MapTaskImportEndpoints(); // one MapXxxEndpoints() call per module's Minimal API registration

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{TagModuleApiSwaggerRegistration.DocumentName}/swagger.json", TagModuleApiSwaggerRegistration.Title);
            options.SwaggerEndpoint($"/swagger/{TaskModuleApiSwaggerRegistration.DocumentName}/swagger.json", TaskModuleApiSwaggerRegistration.Title);
            options.SwaggerEndpoint($"/swagger/{TimeLogModuleApiSwaggerRegistration.DocumentName}/swagger.json", TimeLogModuleApiSwaggerRegistration.Title);
        });

        return app;
    }
}
```

`Program.cs` calls only the two extension methods this file defines:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterSampleModule).Assembly));
builder.Services.AddModules(builder.Configuration);
builder.Services.AddPipeline();
builder.Services.AddApi();

var app = builder.Build();
app.UseApi();
app.Run();
```

# Rule changes

## MUST
- `AddControllers()`/`AddSwaggerGen()`/`AddProblemDetails()` called only from `AddApi()`
- One `SwaggerDoc` + one `SwaggerEndpoint` + one `DocInclusionPredicate` arm per module that publishes an API
- `UseExceptionHandler()` called before `MapControllers()` in `UseApi()`
- Never register a single solution-wide `v1` Swagger document covering every module's routes
- Never let one module's routes appear in another module's `DocInclusionPredicate` arm

# Check list
- [ ] `AddApi()`/`UseApi()` are the only two call sites `Program.cs` uses for the API layer
- [ ] Every module publishing an API contributes exactly one `SwaggerDoc`/`SwaggerEndpoint`/`DocInclusionPredicate` arm
- [ ] `UseExceptionHandler()` precedes `MapControllers()`
