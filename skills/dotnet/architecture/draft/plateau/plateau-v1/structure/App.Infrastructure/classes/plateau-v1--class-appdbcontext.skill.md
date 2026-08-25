---
name: class-appdbcontext
description: Class AppDbContext in the v1 plateau
whenToUse: when reviewing how entity configurations get applied, or how server-assigned timestamps get set before a commit
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]]"
---

# Goal
- Be the service's single `DbContext`, applying every module's entity configurations automatically
- Assign server-authoritative creation/update timestamps at the single persistence boundary, before every commit

# Core Principles
- `OnModelCreating` calls `ApplyConfigurationsFromAssembly` once per module Domain assembly plus its own assembly — never registers a config by hand
- `OnBeforeSaving` runs inside both `SaveChanges`/`SaveChangesAsync` overrides, setting `ServerCreatedDateTime` on `Added` `ICreationInfoModel` entries and `ServerUpdatedDateTime` on `Added`/`Modified` `IUpdateInfoModel` entries

# Implementation
```csharp
//Skill: class-appdbcontext
//Plateau: v1
//Version: 20260825140000

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskModule.Domain.AssemblyMarker).Assembly); — one per module
        base.OnModelCreating(modelBuilder);
    }

    private void OnBeforeSaving()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<ICreationInfoModel>().Where(e => e.State == EntityState.Added))
            entry.Property(nameof(ICreationInfoModelReadOnly.ServerCreatedDateTime)).CurrentValue = now;

        foreach (var entry in ChangeTracker.Entries<IUpdateInfoModel>().Where(e => e.State is EntityState.Added or EntityState.Modified))
            entry.Property(nameof(IUpdateInfoModelReadOnly.ServerUpdatedDateTime)).CurrentValue = now;
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        OnBeforeSaving();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        OnBeforeSaving();
        return base.SaveChangesAsync(ct);
    }
}
```

# Rules
MUST:
- Be the only `DbContext` in the service
- Apply configurations only via `ApplyConfigurationsFromAssembly`
- Override both `SaveChanges` and `SaveChangesAsync` to call `OnBeforeSaving`
MUST NOT:
- Set a user-supplied timestamp — only server timestamps are set here
- Register a `DbSet<T>`/config by hand

# Check list
- [ ] `AppDbContext` is the only `DbContext`, applies configs via `ApplyConfigurationsFromAssembly`
- [ ] `OnBeforeSaving` sets `ServerCreatedDateTime` for `Added` `ICreationInfoModel` entries and `ServerUpdatedDateTime` for `Added`/`Modified` `IUpdateInfoModel` entries
- [ ] Both `SaveChanges` overrides call `OnBeforeSaving`

__Applied solutions:__
- [[../../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../../solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.create.md|AppDbContext.cs.create]]
- [[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[../../../../../solutions/solution-entity-edit-timestamp.skill/Implementation/App.Infrastructure.csproj.extend/AppDbContext.cs.extend.md|AppDbContext.cs.extend]]
