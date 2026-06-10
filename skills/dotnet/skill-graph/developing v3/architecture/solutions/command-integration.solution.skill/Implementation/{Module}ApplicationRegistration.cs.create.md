---
description: Module DI self-registration extension
project_name: "{Module}.Application"
name: "{Module}ApplicationRegistration.cs"
change_kind: create
---

# Goals
- Self-register all handlers and validators in this module's assembly via scan
- Give App.Host a single call surface for wiring up the module — no module internals exposed

# Core Principles
- One static extension method per module — `Register{ModuleName}Module`
- `AddMediatR` scans the Application assembly — all `IRequestHandler` implementations registered automatically
- `AddValidatorsFromAssembly` scans the Application assembly — all `AbstractValidator<T>` registered automatically
- Pipeline behaviors NOT registered here — that is App.Host's responsibility

# Structure

## Project Structure
```
/{Module}.Application
  {Module}ApplicationRegistration.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Module registration | `{ModuleName}ApplicationRegistration` | `TaskApplicationRegistration` | `{ModuleName}ApplicationRegistration.cs` | `TaskApplicationRegistration.cs` |

# Implementation changes

```csharp
// {Module}.Application/{Module}ApplicationRegistration.cs
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace {Module}.Application;

public static class {Module}ApplicationRegistration
{
    public static IServiceCollection Register{ModuleName}Module(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof({Module}ApplicationRegistration).Assembly));

        services.AddValidatorsFromAssembly(
            typeof({Module}ApplicationRegistration).Assembly);

        return services;
    }
}
```

# Rules

MUST:
- Method named `Register{ModuleName}Module`
- Accept `IServiceCollection` and `IConfiguration`
- Register handlers via `AddMediatR` assembly scan
- Register validators via `AddValidatorsFromAssembly`

MUST NOT:
- Register pipeline behaviors — belongs in App.Host
- Register `DbContext` or infrastructure services — belongs in App.Infrastructure registration
- Reference another module's Application assembly
