---
description: Configure timestamp columns for entities based on classification
project_name: "{Module}.Domain"
name: "{EntityName}Config.cs"
element_kind: class
change_kind: extend
tags:
  - solution/entity-edit-timestamp
  - element/entityname-config-cs
---

# Goals
- Map all timestamp properties to the database as required `DateTimeOffset` columns.
- Keep persistence concerns in the configuration class.

# Core Principles
- Timestamp columns are required because they are always assigned before the row is persisted.
- PostgreSQL maps `DateTimeOffset` to `timestamp with time zone` by default.
- No EF attributes on the entity — all mapping lives here.

# Structure

## Project Structure
```
/{Module}.Domain
  /Configurations
    {EntityName}Config.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| EF entity configuration | `{EntityName}Config` | `{EntityName}Config` | `{EntityName}Config.cs` | `{EntityName}Config.cs` |

# Implementation changes

## External Immutable

```csharp
// {Module}.Domain/Configurations/{EntityName}Config.cs
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public const string TableName = nameof({EntityName});
    public const string UX_Guid = $"UX_{TableName}_Guid";

    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Guid).IsRequired();
        builder.HasIndex(x => x.Guid)
            .HasDatabaseName(UX_Guid)
            .IsUnique();

        builder.Property(x => x.ServerCreatedDateTime).IsRequired();
        builder.Property(x => x.UserCreatedDateTime).IsRequired();
    }
}
```

## Internal Mutable

```csharp
// {Module}.Domain/Configurations/{EntityName}Config.cs
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public const string TableName = nameof({EntityName});
    public const string VersionedEntityName = "{Entity}";

    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version)
            .HasColumnName("xmin")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(x => x.ServerCreatedDateTime).IsRequired();
        builder.Property(x => x.UserCreatedDateTime).IsRequired();
        builder.Property(x => x.ServerUpdatedDateTime).IsRequired();
        builder.Property(x => x.UserUpdatedDateTime).IsRequired();
    }
}
```

## External Mutable

```csharp
// {Module}.Domain/Configurations/{EntityName}Config.cs
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public const string TableName = nameof({EntityName});
    public const string UX_Guid = $"UX_{TableName}_Guid";
    public const string VersionedEntityName = "{Entity}";

    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Guid).IsRequired();
        builder.HasIndex(x => x.Guid)
            .HasDatabaseName(UX_Guid)
            .IsUnique();

        builder.Property(x => x.Version)
            .HasColumnName("xmin")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(x => x.ServerCreatedDateTime).IsRequired();
        builder.Property(x => x.UserCreatedDateTime).IsRequired();
        builder.Property(x => x.ServerUpdatedDateTime).IsRequired();
        builder.Property(x => x.UserUpdatedDateTime).IsRequired();
    }
}
```

# Rule changes

## MUST
- Map every timestamp property that exists on the entity as `.IsRequired()`.
- Use `DateTimeOffset` properties on the entity.
- Keep timestamp mapping in the configuration class.

## MUST NOT
- Map timestamp columns on `Internal Immutable` entities.
- Map `ServerUpdatedDateTime` or `UserUpdatedDateTime` on `External Immutable` entities.
- Use EF attributes on the entity.

# Anti-patterns
- Mapping timestamp columns only in some entities inconsistently.
- Allowing nullable timestamp columns.
- Using `DateTime` column types.

# Check list
- [ ] All timestamp properties that exist on the entity are mapped.
- [ ] Mapped properties are `.IsRequired()`.
- [ ] No EF attributes on the entity class.
- [ ] External Immutable config does not include update timestamps.

# Unittest TestCases
- [ ] WHEN config inspected for `External Immutable` THEN only creation timestamps are mapped.
- [ ] WHEN config inspected for mutable entity THEN creation and update timestamps are mapped.
- [ ] WHEN migration generated THEN timestamp columns are non-nullable `DateTimeOffset`.
- [ ] WHEN `Internal Immutable` config inspected THEN no timestamp columns are mapped.
