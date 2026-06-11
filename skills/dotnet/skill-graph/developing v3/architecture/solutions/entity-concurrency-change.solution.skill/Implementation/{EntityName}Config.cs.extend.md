---
description: Map Version to xmin with IsConcurrencyToken and ValueGeneratedOnAddOrUpdate
name: "{EntityName}Config.cs"
change_kind: extend
---

# Goals
- Configure `Version` as the EF concurrency token mapped to the PostgreSQL `xmin` system column

# Core Principles
- `xmin` is a PostgreSQL system column — automatically incremented on every row update
- `IsConcurrencyToken()` tells EF to include `Version` in `WHERE` clause on `UPDATE` — EF raises `DbUpdateConcurrencyException` if zero rows affected
- `ValueGeneratedOnAddOrUpdate()` tells EF the value comes from the database — never from application code

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| EF entity configuration | `{EntityName}Config` | `{EntityName}Config` | `{EntityName}Config.cs` | `{EntityName}Config.cs` |

# Implementation changes

Every mutable entity configuration must include the `Version` mapping:

```csharp
// {Module}.Domain/Configurations/{EntityName}Config.cs
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public static string TableName = nameof({EntityName});

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

# Rules

MUST:
- Every mutable entity configuration maps `Version` to `xmin` with `IsConcurrencyToken()` and `ValueGeneratedOnAddOrUpdate()`

MUST NOT:
- `HasDefaultValue` or `HasComputedColumnSql` used on `Version` — `xmin` is managed entirely by PostgreSQL

# Anti-patterns
- `Version` mapped to a regular column without `IsConcurrencyToken()` — loses database-level protection

# Check list
- [ ] `Version` mapped to `xmin`
- [ ] `.IsConcurrencyToken()` called
- [ ] `.ValueGeneratedOnAddOrUpdate()` called
