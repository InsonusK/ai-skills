---
description: Override SaveChanges to assign server timestamps before persisting
name: App.Infrastructure.csproj
element_kind: project
change_kind: extend
---

# Goals
- Ensure server timestamps are assigned automatically for all timestamped entities before they are persisted.
- Keep timestamp persistence logic in the single persistence boundary.

# Core Principles
- `AppDbContext` owns server time.
- Both synchronous and asynchronous `SaveChanges` paths call the same pre-save logic.
- `OnBeforeSaving` inspects `ChangeTracker` entries that implement the timestamp interfaces.

# Structure

## Project Structure
```
/App.Infrastructure
  /Persistence
    AppDbContext.cs
```

## Directory and class skills
| Directory \ file | Description |
| ----------------- | ----------- |
| /Persistence/AppDbContext.cs | DbContext that assigns server timestamps before saving |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Microsoft.EntityFrameworkCore` | latest stable | Provides `DbContext`, `ChangeTracker`, and `EntityState`. |

# Allowed Dependencies
- Shared
- {Module}.Domain (all modules)

# Rules

MUST:
- Override `SaveChanges()` and `SaveChangesAsync(CancellationToken)`.
- Call `OnBeforeSaving()` before delegating to the base method.
- Set `ServerCreatedDateTime` for `Added` entries implementing `ICreationInfoModel`.
- Set `ServerUpdatedDateTime` for `Added` or `Modified` entries implementing `IUpdateInfoModel`.
- Use `DateTimeOffset.UtcNow`.

MUST NOT:
- Set user timestamps.
- Assign server timestamps anywhere outside `AppDbContext`.

# Anti-patterns
- Overriding only `SaveChangesAsync` and forgetting the synchronous path.
- Using `DateTime.Now` or `DateTime.UtcNow` instead of `DateTimeOffset.UtcNow`.

# Check list
- [ ] `SaveChanges()` overridden and calls `OnBeforeSaving()`.
- [ ] `SaveChangesAsync()` overridden and calls `OnBeforeSaving()`.
- [ ] `OnBeforeSaving()` sets `ServerCreatedDateTime` for added `ICreationInfoModel` entries.
- [ ] `OnBeforeSaving()` sets `ServerUpdatedDateTime` for added/modified `IUpdateInfoModel` entries.
