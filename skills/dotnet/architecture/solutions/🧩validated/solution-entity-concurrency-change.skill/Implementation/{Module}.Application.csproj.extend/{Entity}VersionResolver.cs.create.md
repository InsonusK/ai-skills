---
description: Reads the current concurrency version for a single versioned entity
project_name: "{Module}.Application"
name: "{Entity}VersionResolver.cs"
element_kind: class
change_kind: create
---

# Goals
- Provide the entity-specific `IEntityVersionResolver` implementation for `{Entity}`
- Use the module's existing `{Entity}ByIdSpec` so the generic and per-entity spec concepts are not duplicated

# Core Principles
- Inherits no base class — implements `IEntityVersionResolver` directly
- Uses `IReadRepository<{Entity}>` from Shared
- `VersionedEntityName` constant mirrors `{Entity}Config.VersionedEntityName`

# Structure

## Project Structure
```
/{Module}.Application
  /Concurrency
    {Entity}VersionResolver.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Entity version resolver | `{Entity}VersionResolver` | `{Entity}VersionResolver` | `{Entity}VersionResolver.cs` | `{Entity}VersionResolver.cs` |

# Implementation changes

```csharp
// {Module}.Application/Concurrency/{Entity}VersionResolver.cs
using Shared.Concurrency;
using Shared.Repositories;
using {Module}.Application.Specifications;

namespace {Module}.Application.Concurrency;

public class {Entity}VersionResolver : IEntityVersionResolver
{
    private readonly IReadRepository<{Entity}> _repository;

    public {Entity}VersionResolver(IReadRepository<{Entity}> repository)
    {
        _repository = repository;
    }

    public const string VersionedEntityName = "{EntityName}";

    public async Task<int> GetCurrentVersionForAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.FirstOrDefaultAsync(
            new {Entity}ByIdSpec(id),
            cancellationToken);

        if (entity is null)
            return 0;

        return (int)entity.Version;
    }
}
```

> **Note:** `{EntityName}` is the same stable business string declared in `{Entity}Config.VersionedEntityName`. It is used by `EntityVersionResolverFactory` in App.Infrastructure to route requests to this resolver.

# Rule changes

## MUST
- Implement `IEntityVersionResolver` from Shared
- Declare `public const string VersionedEntityName` matching `{Entity}Config.VersionedEntityName`
- Use `IReadRepository<{Entity}>`
- Use `{Entity}ByIdSpec`
- Return `0` when entity is not found
- Return current `Version` cast to `int` when found

## MUST NOT
- Reference EF Core or DbContext directly
- Throw when entity is missing

# Anti-patterns
- Duplicating the `{Entity}ByIdSpec` query inline in the resolver — reuse the spec
- Hardcoding entity name strings that differ from the config constant

# Check list
- [ ] `{Entity}VersionResolver` defined in `{Module}.Application/Concurrency/{Entity}VersionResolver.cs`
- [ ] Implements `IEntityVersionResolver`
- [ ] `VersionedEntityName` matches `{Entity}Config.VersionedEntityName`
- [ ] Uses `IReadRepository<{Entity}>` and `{Entity}ByIdSpec`
- [ ] Returns `0` for missing entity

# Unittest TestCases
- [ ] WHEN component is requested THEN it provide the entity-specific IEntityVersionResolver implementation for {Entity}
- [ ] WHEN applied THEN Use the module's existing {Entity}ByIdSpec so the generic and per-entity spec concepts are not duplicated
- [ ] WHEN applied THEN Inherits no base class — implements IEntityVersionResolver directly
- [ ] WHEN applied THEN Uses IReadRepository<{Entity}> from Shared
- [ ] WHEN applied THEN VersionedEntityName constant mirrors {Entity}Config.VersionedEntityName
- [ ] WHEN verified THEN {Entity}VersionResolver defined in {Module}.Application/Concurrency/{Entity}VersionResolver.cs
- [ ] WHEN verified THEN Implements IEntityVersionResolver
- [ ] WHEN verified THEN VersionedEntityName matches {Entity}Config.VersionedEntityName
- [ ] WHEN verified THEN Uses IReadRepository<{Entity}> and {Entity}ByIdSpec
- [ ] WHEN naming '{Entity} version resolver' THEN pattern matches convention
