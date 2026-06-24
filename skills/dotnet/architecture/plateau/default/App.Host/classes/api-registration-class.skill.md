---
uid: da3458f1-893d-4af5-97da-91d490ac2142
name: api-registration-class
description: Controller and middleware registration
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication-solution.skill]]"
---

# Goal
- Register controllers from all module Api assemblies via `AddControllers` with assembly parts
- Configure `ProblemDetails` as the standard error response format

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| API DI registration | `ApiRegistration` | `ApiRegistration` | `ApiRegistration.cs` | `ApiRegistration.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs.create]]

# Implementation
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
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs.create]]

# Rules
MUST:
	- `UseExceptionHandler()` registered before `MapControllers()`
	- All module Api assemblies added as application parts
	- All Minimal API endpoint groups mapped explicitly
	- `AddProblemDetails()` registered in DI
MUST NOT:
	- Register controllers manually one by one

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs.create]]

# Anti-patterns
- Missing `UseExceptionHandler()` before `MapControllers()`
- Forgetting to map Minimal API endpoint groups

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs.create]]

# Check list
- [ ] `ApiRegistration.cs` exists in `/DependencyInjection`
- [ ] All module Api assemblies added as application parts
- [ ] `UseExceptionHandler()` before `MapControllers()`
- [ ] `AddProblemDetails()` registered

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Register controllers from all module Api assemblies via AddControllers with assembly parts
- [ ] WHEN applied THEN Configure ProblemDetails as the standard error response format
- [ ] WHEN verified THEN ApiRegistration.cs exists in /DependencyInjection
- [ ] WHEN verified THEN All module Api assemblies added as application parts
- [ ] WHEN verified THEN UseExceptionHandler() before MapControllers()
- [ ] WHEN verified THEN AddProblemDetails() registered
- [ ] WHEN naming 'API DI registration' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/http-api-publication-solution.skill.md|http-api-publication]] - [[skills/dotnet/architecture/solutions/🧩validated/http-api-publication-solution.skill/Implementation/App.Host.csproj.extend/ApiRegistration.cs.create.md|ApiRegistration.cs.create]]
