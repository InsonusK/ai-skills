---
name: class-api-registration
description: Controller and middleware registration
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]]"
---

# Goal
- Register controllers from all module Api assemblies via `AddControllers` with assembly parts
- Configure `ProblemDetails` as the standard error response format

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create|ApiRegistration.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| API DI registration | `ApiRegistration` | `ApiRegistration` | `ApiRegistration.cs` | `ApiRegistration.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create|ApiRegistration.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-api-registration
//Plateau: default
//Version: 20260628
```

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

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create|ApiRegistration.cs]]

# Rules
MUST:
	- `UseExceptionHandler()` registered before `MapControllers()`
	- All module Api assemblies added as application parts
	- All Minimal API endpoint groups mapped explicitly
	- `AddProblemDetails()` registered in DI
MUST NOT:
	- Register controllers manually one by one

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create|ApiRegistration.cs]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- Missing `UseExceptionHandler()` before `MapControllers()`
- Forgetting to map Minimal API endpoint groups

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create|ApiRegistration.cs]]

# Check list
- [ ] `ApiRegistration.cs` exists in `/DependencyInjection`
- [ ] All module Api assemblies added as application parts
- [ ] `UseExceptionHandler()` before `MapControllers()`
- [ ] `AddProblemDetails()` registered

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create|ApiRegistration.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Register controllers from all module Api assemblies via AddControllers with assembly parts
- [ ] WHEN applied THEN Configure ProblemDetails as the standard error response format
- [ ] WHEN verified THEN ApiRegistration.cs exists in /DependencyInjection
- [ ] WHEN verified THEN All module Api assemblies added as application parts
- [ ] WHEN verified THEN UseExceptionHandler() before MapControllers()
- [ ] WHEN verified THEN AddProblemDetails() registered
- [ ] WHEN naming 'API DI registration' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/solution-http-api-publication.skill|solution-http-api-publication]] - [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-http-api-publication.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create|ApiRegistration.cs]]
