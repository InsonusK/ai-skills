---
description: The service's single DbContext, applying every module's entity configurations
project_name: App.Infrastructure
name: AppDbContext.cs
element_kind: class
change_kind: create
tags:
  - solution/repository-integration
  - element/appdbcontext-cs
---

# Goals
- Give the service exactly one `DbContext`, shared by every module's repositories
- Apply every module's `IEntityTypeConfiguration<T>` classes automatically, without listing entities by hand

# Core Principles
- `AppDbContext` is the only class that knows EF Core's model-building API — module Domain/Application never reference it
- `OnModelCreating` calls `ApplyConfigurationsFromAssembly` once per module Domain assembly — never registers a `DbSet<T>` or configuration by hand
- Cross-module foreign-key configurations (owned by App.Infrastructure per `solution-domain-configuration`) are applied from the same scan, since they live in App.Infrastructure's own assembly

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| The service's DbContext | AppDbContext | AppDbContext | AppDbContext.cs | AppDbContext.cs |

# Implementation changes

```csharp
// App.Infrastructure/Persistence/AppDbContext.cs
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // One per module, added as modules are registered:
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskModule.Domain.AssemblyMarker).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
```

Registered inside `InfrastructureRegistration.AddInfrastructure()` (see `App.Host.csproj.extend`):

```csharp
services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
```

# Rule changes

## MUST
- Be the only `DbContext` in the service
- Apply configurations via `ApplyConfigurationsFromAssembly` for App.Infrastructure's own assembly and every module's Domain assembly
- Be registered via `AddDbContext<AppDbContext>` inside `AddInfrastructure()`
- Never register a `DbSet<T>` property or an `IEntityTypeConfiguration<T>` by hand instead of via the assembly scan
- Never be referenced by any module `Application` or `Domain` project directly

# Check list
- [ ] `AppDbContext` is the only `DbContext` in the service
- [ ] `OnModelCreating` applies configurations via `ApplyConfigurationsFromAssembly` only — no manual `DbSet`/config registration
- [ ] Registered via `AddDbContext<AppDbContext>` inside `AddInfrastructure()`
