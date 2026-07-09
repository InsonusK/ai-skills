---
name: class-appdbcontext
description: EF Core DbContext that assigns server timestamps before saving
domain: skill
type: template
version: 20260630010447
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]]"
---

# Goal
- Provide the concrete EF Core `DbContext` for the application.
- Register all module entity configurations via assembly scan.
- Assign authoritative server timestamps automatically before persisting timestamped entities.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.extend|AppDbContext.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create|UnitOfWork.cs]]

# Core Principles
- Apply ONE plateau template per class
- App.Infrastructure is the only project with a concrete `DbContext`.
- Server timestamps are authoritative and assigned at the persistence boundary.
- The same pre-save logic runs for both synchronous and asynchronous save paths.
- Only entries that implement the timestamp interfaces are affected.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.extend|AppDbContext.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create|UnitOfWork.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| DbContext | `AppDbContext` | `AppDbContext` | `AppDbContext.cs` | `AppDbContext.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.extend|AppDbContext.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create|UnitOfWork.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-appdbcontext
//Plateau: default
//Version: 20260630010447
```

```csharp
// App.Infrastructure/Persistence/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using Shared.Timestamps;

namespace App.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public override int SaveChanges()
    {
        OnBeforeSaving();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void OnBeforeSaving()
    {
        var currentTime = DateTimeOffset.UtcNow;

        var createdEntities = ChangeTracker.Entries<ICreationInfoModel>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToArray();

        foreach (var entity in createdEntities)
        {
            entity.ServerCreatedDateTime = currentTime;
        }

        var editedEntities = ChangeTracker.Entries<IUpdateInfoModel>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .ToArray();

        foreach (var entity in editedEntities)
        {
            entity.ServerUpdatedDateTime = currentTime;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        // ... module configuration registrations
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.extend|AppDbContext.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create|UnitOfWork.cs]]

# Rules
MUST:
	- Override `SaveChanges()` and `SaveChangesAsync(CancellationToken)`.
	- Call `OnBeforeSaving()` before the base save call.
	- Use `DateTimeOffset.UtcNow`.
	- Set `ServerCreatedDateTime` only for `Added` `ICreationInfoModel` entries.
	- Set `ServerUpdatedDateTime` for `Added` and `Modified` `IUpdateInfoModel` entries.
MUST NOT:
	- Set user timestamps in `OnBeforeSaving()`.
	- Use `DateTime` or `DateTime.Now`.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.extend|AppDbContext.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create|UnitOfWork.cs]]

# Anti-patterns
- Overriding only `SaveChangesAsync` and forgetting the synchronous path.
- Assigning server timestamps in handlers or behaviors.
- Iterating over all entries instead of filtering by interface.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.extend|AppDbContext.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create|UnitOfWork.cs]]

# Check list
- [ ] `SaveChanges()` overridden.
- [ ] `SaveChangesAsync()` overridden.
- [ ] `OnBeforeSaving()` called in both overrides.
- [ ] `ServerCreatedDateTime` set for added `ICreationInfoModel` entries.
- [ ] `ServerUpdatedDateTime` set for added/modified `IUpdateInfoModel` entries.
- [ ] `DateTimeOffset.UtcNow` used.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.extend|AppDbContext.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create|UnitOfWork.cs]]

# Unittest TestCases
- [ ] WHEN `SaveChangesAsync` is called on a new mutable entity THEN `ServerCreatedDateTime` and `ServerUpdatedDateTime` are set.
- [ ] WHEN `SaveChangesAsync` is called on a modified mutable entity THEN `ServerUpdatedDateTime` changes and `ServerCreatedDateTime` does not.
- [ ] WHEN `SaveChanges` (sync) is called THEN `OnBeforeSaving` runs.
- [ ] WHEN `External Immutable` entity is inserted THEN only server creation timestamp is set.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill|solution-entity-edit-timestamp]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.extend|AppDbContext.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend|App.Infrastructure.csproj]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/solution-unit-of-work.skill|solution-unit-of-work]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend/UnitOfWork.cs.create|UnitOfWork.cs]]
