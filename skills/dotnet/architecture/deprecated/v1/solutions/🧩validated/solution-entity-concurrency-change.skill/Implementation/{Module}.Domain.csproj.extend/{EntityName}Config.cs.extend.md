---
description: Map Version to xmin with IsConcurrencyToken and ValueGeneratedOnAddOrUpdate, and add VersionedEntityName constant
project_name: "{Module}.Domain"
name: "{EntityName}Config.cs"
element_kind: class
change_kind: extend
tags:
  - solution/entity-concurrency-change
  - element/entityname-config-cs
---

# Goals
- Configure `Version` as the EF concurrency token mapped to the PostgreSQL `xmin` system column
- Declare the stable business name (`VersionedEntityName`) used by the concurrency infrastructure

# Core Principles
- `xmin` is a PostgreSQL system column — automatically incremented on every row update
- `IsConcurrencyToken()` tells EF to include `Version` in `WHERE` clause on `UPDATE` — EF raises `DbUpdateConcurrencyException` if zero rows affected
- `ValueGeneratedOnAddOrUpdate()` tells EF the value comes from the database — never from application code
- `VersionedEntityName` is the single stable business name used by `EntityVersionResolverFactory`, `IHasVersions`, and `ETagEncoder` — changing it is a breaking API change

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| EF entity configuration | `{EntityName}Config` | `{EntityName}Config` | `{EntityName}Config.cs` | `{EntityName}Config.cs` |

# Implementation changes

Every mutable entity configuration must include the `Version` mapping and the `VersionedEntityName` constant:

```csharp
// {Module}.Domain/Configurations/{EntityName}Config.cs
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public const string TableName = nameof({EntityName});
    public const string VersionedEntityName = "{Entity}";

    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        // ... other configuration (indexes, relations)

        builder
            .Property(e => e.Version)
            .HasColumnName("xmin")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();
    }
}
```

# Rule changes

## MUST
- Every mutable entity configuration maps `Version` to `xmin` with `IsConcurrencyToken()` and `ValueGeneratedOnAddOrUpdate()`
- Every mutable entity configuration declares `public const string VersionedEntityName` with the stable business name
- `TableName` is `public const string`

## MUST NOT
- `HasDefaultValue` or `HasComputedColumnSql` used on `Version` — `xmin` is managed entirely by PostgreSQL
- `VersionedEntityName` be derived from `TableName` or `nameof({EntityName})` — it is an explicit business contract

# Anti-patterns
- `Version` mapped to a regular column without `IsConcurrencyToken()` — loses database-level protection
- Putting `VersionedEntityName` on the entity class — spreads configuration across the domain instead of keeping it in the config

# Check list
- [ ] `Version` mapped to `xmin`
- [ ] `.IsConcurrencyToken()` called
- [ ] `.ValueGeneratedOnAddOrUpdate()` called
- [ ] `VersionedEntityName` declared as `public const string`
- [ ] `TableName` declared as `public const string`

# Unittest TestCases
- [ ] WHEN applied THEN Configure Version as the EF concurrency token mapped to the PostgreSQL xmin system column
- [ ] WHEN applied THEN xmin is a PostgreSQL system column — automatically incremented on every row update
- [ ] WHEN applied THEN IsConcurrencyToken() tells EF to include Version in WHERE clause on UPDATE — EF raises DbUpdateConcurrencyException if zero rows affected
- [ ] WHEN applied THEN ValueGeneratedOnAddOrUpdate() tells EF the value comes from the database — never from application code
- [ ] WHEN applied THEN VersionedEntityName declared as public const string with the stable business name
- [ ] WHEN verified THEN Version mapped to xmin
- [ ] WHEN verified THEN .IsConcurrencyToken() called
- [ ] WHEN verified THEN .ValueGeneratedOnAddOrUpdate() called
- [ ] WHEN verified THEN VersionedEntityName declared as public const string
- [ ] WHEN verified THEN TableName declared as public const string
- [ ] WHEN naming 'EF entity configuration' THEN pattern matches convention
