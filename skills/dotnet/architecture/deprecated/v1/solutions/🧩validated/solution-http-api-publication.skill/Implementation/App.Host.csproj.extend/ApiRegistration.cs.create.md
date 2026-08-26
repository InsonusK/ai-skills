---
description: Controller and middleware registration
project_name: App.Host
name: ApiRegistration.cs
element_kind: class
change_kind: create
tags:
  - solution/http-api-publication
  - element/apiregistration-cs
---

# Goals
- Register controllers from all module Api assemblies via `AddControllers` with assembly parts
- Configure `ProblemDetails` as the standard error response format

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| API DI registration | `ApiRegistration` | `ApiRegistration` | `ApiRegistration.cs` | `ApiRegistration.cs` |

# Implementation changes

```csharp
// App.Host/DependencyInjection/ApiRegistration.cs
using Microsoft.AspNetCore.Mvc;

namespace App.Host.DependencyInjection;

public static class ApiRegistration
{
    public static IServiceCollection AddApi(
        this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddApplicationPart(typeof(TaskController).Assembly)
            .AddApplicationPart(typeof(TimeLogController).Assembly)
            .AddApplicationPart(typeof(UserController).Assembly);

        services.AddProblemDetails();

        return services;
    }
}
```

#### Program.cs (extended)

##### Goal
- Wire API registration and map controllers and Minimal API endpoint groups in the request pipeline

####

# Implementation changes

```csharp
// App.Host/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApi()
    .AddPipeline()
    .AddRepositories()
    .RegisterTaskModule(builder.Configuration)
    .RegisterTimeLogModule(builder.Configuration)
    .RegisterUserModule(builder.Configuration)
    .RegisterAppQueries();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapControllers();

// Minimal API endpoint groups
app.MapWebhookEndpoints();

app.Run();
```

# Rule changes

## MUST
- `UseExceptionHandler()` registered before `MapControllers()`
- All module Api assemblies added as application parts
- All Minimal API endpoint groups mapped explicitly
- `AddProblemDetails()` registered in DI
- Every `ResultStatus` handler can return has an explicit `[ProducesResponseType]`
- `ResultStatus.Ok` → 200 OK
- `ResultStatus.Created` → 201 Created with `CreatedAtAction`
- `ResultStatus.NoContent` → 204 No Content
- `ResultStatus.Invalid` → 400 Bad Request with `ProblemDetails`
- `ResultStatus.NotFound` → 404 Not Found with `ProblemDetails`
- `ResultStatus.Conflict` → 409 Conflict with `ProblemDetails`
- `ResultStatus.Error` → 500 Internal Server Error with `ProblemDetails`
- Any other `ResultStatus` → throw `InvalidOperationException`
- Each module that publishes HTTP APIs exposes a public static `{Module}ApiSwaggerRegistration` class in `{Module}.Api`
- `{Module}ApiSwaggerRegistration` declares `DocumentName`, `Title`, `Version`, and `MatchesRoute(string? relativePath)`

## MUST NOT
- Register controllers manually one by one
- Use a single monolithic `v1` Swagger document that contains all module routes

# Anti-patterns
- Missing `UseExceptionHandler()` before `MapControllers()`
- Forgetting to map Minimal API endpoint groups

# Check list
- [ ] `ApiRegistration.cs` exists in `/DependencyInjection`
- [ ] All module Api assemblies added as application parts
- [ ] `UseExceptionHandler()` before `MapControllers()`
- [ ] `AddProblemDetails()` registered

# Unittest TestCases
- [ ] WHEN applied THEN Register controllers from all module Api assemblies via AddControllers with assembly parts
- [ ] WHEN applied THEN Configure ProblemDetails as the standard error response format
- [ ] WHEN verified THEN ApiRegistration.cs exists in /DependencyInjection
- [ ] WHEN verified THEN All module Api assemblies added as application parts
- [ ] WHEN verified THEN UseExceptionHandler() before MapControllers()
- [ ] WHEN verified THEN AddProblemDetails() registered
- [ ] WHEN naming 'API DI registration' THEN pattern matches convention
