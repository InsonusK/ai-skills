---
description: Assign server timestamps in AppDbContext before saving
project_name: App.Infrastructure
name: AppDbContext.cs
element_kind: class
change_kind: extend
tags:
  - solution/entity-edit-timestamp
  - element/appdbcontext-cs
---

# Goals
- Set `ServerCreatedDateTime` and `ServerUpdatedDateTime` automatically before EF Core writes changes to the database.

# Core Principles
- The same logic runs for both sync and async save paths.
- Only entries that implement the timestamp interfaces are affected.

# Structure

## Project Structure
```
/App.Infrastructure
  /Persistence
    AppDbContext.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| DbContext | `AppDbContext` | `AppDbContext` | `AppDbContext.cs` | `AppDbContext.cs` |

# Implementation changes

Extend the existing `AppDbContext` with `OnBeforeSaving` and override both save methods:

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
# Rule changes

## MUST
- Override `SaveChanges()` and `SaveChangesAsync(CancellationToken)`.
- Call `OnBeforeSaving()` before the base save call.
- Set `ServerCreatedDateTime` only for `Added` `ICreationInfoModel` entries.
- Set `ServerUpdatedDateTime` for `Added` and `Modified` `IUpdateInfoModel` entries.
- `Internal Immutable` entities implement none of the timestamp interfaces.
- Timestamp properties on entities are `DateTimeOffset` with `internal set`.
- Create handlers for mutable entities set both `UserCreatedDateTime` and `UserUpdatedDateTime` to `ActionTimeStamp`.
- Update handlers set only `UserUpdatedDateTime` to `ActionTimeStamp`.
- Create handlers for `External Immutable` entities set only `UserCreatedDateTime` to `ActionTimeStamp`.
- EF configuration maps timestamp properties as required `DateTimeOffset` columns.
- Handlers never assign server timestamps.
- `AppDbContext` never assigns user timestamps.
- `OnBeforeSaving()` uses `DateTimeOffset.UtcNow` as the server time source.

## MUST NOT
- Set user timestamps in `OnBeforeSaving()`.
- Use `DateTime` or `DateTime.Now`.
- Add timestamp fields to `Internal Immutable` entities.
- Add update timestamp fields to `External Immutable` entities.
- Validate `ActionTimeStamp` inside handlers.
- Set server timestamps in handlers.
- Allow `ActionTimeStamp` to be in the future.
- Use EF attributes on entities for timestamp mapping.

# Anti-patterns
- Overriding only `SaveChangesAsync`.
- Assigning server timestamps in handlers or behaviors.
- Iterating over all entries instead of filtering by interface.

# Check list
- [ ] `SaveChanges()` overridden.
- [ ] `SaveChangesAsync()` overridden.
- [ ] `OnBeforeSaving()` called in both overrides.
- [ ] `ServerCreatedDateTime` set for added `ICreationInfoModel` entries.
- [ ] `ServerUpdatedDateTime` set for added/modified `IUpdateInfoModel` entries.
- [ ] `DateTimeOffset.UtcNow` used.

# Unittest TestCases
- [ ] WHEN `SaveChangesAsync` is called on a new mutable entity THEN `ServerCreatedDateTime` and `ServerUpdatedDateTime` are set.
- [ ] WHEN `SaveChangesAsync` is called on a modified mutable entity THEN `ServerUpdatedDateTime` changes and `ServerCreatedDateTime` does not.
- [ ] WHEN `SaveChanges` (sync) is called THEN `OnBeforeSaving` runs.
- [ ] WHEN `External Immutable` entity is inserted THEN only server creation timestamp is set.
