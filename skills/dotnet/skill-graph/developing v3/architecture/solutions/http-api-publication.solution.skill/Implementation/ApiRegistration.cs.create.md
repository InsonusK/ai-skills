---
description: Controller and middleware registration
project_name: App.Host
name: ApiRegistration.cs
change_kind: create
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

##### Implementation changes

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

# Rules

MUST:
- `UseExceptionHandler()` registered before `MapControllers()`
- All module Api assemblies added as application parts
- All Minimal API endpoint groups mapped explicitly
- `AddProblemDetails()` registered in DI

MUST NOT:
- Register controllers manually one by one

# Anti-patterns
- Missing `UseExceptionHandler()` before `MapControllers()`
- Forgetting to map Minimal API endpoint groups

# Check list
- [ ] `ApiRegistration.cs` exists in `/DependencyInjection`
- [ ] All module Api assemblies added as application parts
- [ ] `UseExceptionHandler()` before `MapControllers()`
- [ ] `AddProblemDetails()` registered
