---
name: plateau-domain-service--class-app-dbcontext
description: Class AppDbContext in the plateau-domain-service plateau — the service's single DbContext, applying every module's entity configurations and setting server timestamps / concurrency tokens before save
whenToUse: when editing the DbContext — adding a module's config-assembly scan, or the OnBeforeSaving server-timestamp / concurrency-token logic
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]]"
---

# Goal
- Give the service exactly one `DbContext`, shared by every module's repositories; apply every module's `IEntityTypeConfiguration` automatically; assign server timestamps (and, standing in for `xmin`, bump the concurrency token) in `OnBeforeSaving`.

__Applied solutions:__
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.create.md|AppDbContext.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `sealed`, in `/App.Infrastructure/Persistence`. The only class that knows EF's model-building API — module `Domain`/`Application` never reference it.
- `OnModelCreating` calls `ApplyConfigurationsFromAssembly` once for its own assembly (cross-module FKs) plus once per module Domain assembly — never registers a `DbSet<T>` or a config by hand.
- **VP7:** overrides `SaveChanges` / `SaveChangesAsync` to call `OnBeforeSaving`, which sets `ServerCreatedDateTime` for `Added` `ICreationInfoModel` entries and `ServerUpdatedDateTime` for `Added`/`Modified` `IUpdateInfoModel` entries, from `DateTimeOffset.UtcNow`. Never touches user timestamps.
- Registered via `AddDbContext<AppDbContext>` inside `AddInfrastructure()`; the provider (Npgsql / in-memory) is chosen there.

# Implementation
```csharp
// Skill: plateau-domain-service--class-app-dbcontext
// Plateau: domain-service
// Version: 20260902000000
using Microsoft.EntityFrameworkCore;
using Shared.Concurrency;
using Shared.Timestamps;

namespace App.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public override int SaveChanges() { OnBeforeSaving(); return base.SaveChanges(); }
    public override Task<int> SaveChangesAsync(CancellationToken ct = default) { OnBeforeSaving(); return base.SaveChangesAsync(ct); }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        mb.ApplyConfigurationsFromAssembly(typeof({Module}.Domain.Configurations.{Entity}Config).Assembly);
        base.OnModelCreating(mb);
    }

    private void OnBeforeSaving()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var e in ChangeTracker.Entries<ICreationInfoModel>().Where(e => e.State == EntityState.Added))
            e.Entity.ServerCreatedDateTime = now;
        foreach (var e in ChangeTracker.Entries<IUpdateInfoModel>().Where(e => e.State is EntityState.Added or EntityState.Modified))
            e.Entity.ServerUpdatedDateTime = now;
        // Production maps Version to xmin and drops this loop.
        foreach (var e in ChangeTracker.Entries<IVersioned>().Where(e => e.State is EntityState.Added or EntityState.Modified))
        { var p = e.Property(nameof(IVersioned.Version)); p.CurrentValue = (uint)(p.CurrentValue ?? 0u) + 1u; }
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[../../../../../solutions/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.extend.md|AppDbContext.cs.extend]]

# Rules
MUST:
- Be the only `DbContext`; apply configs only via `ApplyConfigurationsFromAssembly` (own assembly + each module Domain assembly).
- Override both `SaveChanges` and `SaveChangesAsync`, calling `OnBeforeSaving` before the base.
- Set only server timestamps in `OnBeforeSaving`, from `DateTimeOffset.UtcNow`; never set user timestamps here.
- Be registered via `AddDbContext<AppDbContext>` inside `AddInfrastructure()`; never be referenced by a module project.
- Never apply several plateau templates per class.

# Check list
- [ ] The only `DbContext`; configs applied only by assembly scan.
- [ ] Both save methods overridden; `OnBeforeSaving` runs before the base.
- [ ] `ServerCreatedDateTime`/`ServerUpdatedDateTime` set from `UtcNow`; user times untouched.

# Unittest TestCases
- [ ] WHEN a new `IUpdateInfoModel` entity is saved THEN both server timestamps are set.
- [ ] WHEN an existing entity is modified THEN `ServerUpdatedDateTime` changes and `ServerCreatedDateTime` does not.
