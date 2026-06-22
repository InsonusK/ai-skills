# {EntityName}Config.cs - extend

Extend the entity configuration to match the selected classification. Only the mappings required by the classification are added.

## Internal Immutable

No changes beyond the standard Id mapping.

**Dependencies**: do not implement `entity-concurrency-change.solution.skill` or `external-created-entity.solution.skill`.

```csharp
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.HasKey(x => x.Id);

        // other immutable property mappings
    }
}
```

## External Immutable

Add the `Guid` mapping with a unique index. There is no `Version` concurrency token.

**Dependencies**: implement `external-created-entity.solution.skill`; do not implement `entity-concurrency-change.solution.skill`.

```csharp
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public const string UX_Guid = "UX_{EntityName}_Guid";

    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Guid).IsRequired();
        builder.HasIndex(x => x.Guid)
            .HasDatabaseName(UX_Guid)
            .IsUnique();

        // other immutable property mappings
    }
}
```

## Internal Mutable

Add the `Version` concurrency token mapped to PostgreSQL `xmin`. There is no `Guid`.

**Dependencies**: implement `entity-concurrency-change.solution.skill`; do not implement `external-created-entity.solution.skill`.

```csharp
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public const string VersionedEntityName = "{EntityName}";

    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version)
            .IsRowVersion()
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        // other property mappings
    }
}
```

## External Mutable

Add both the unique `Guid` index and the `Version` concurrency token mapped to `xmin`.

**Dependencies**: implement both `entity-concurrency-change.solution.skill` and `external-created-entity.solution.skill`.

```csharp
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public const string UX_Guid = "UX_{EntityName}_Guid";
    public const string VersionedEntityName = "{EntityName}";

    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Guid).IsRequired();
        builder.HasIndex(x => x.Guid)
            .HasDatabaseName(UX_Guid)
            .IsUnique();

        builder.Property(x => x.Version)
            .IsRowVersion()
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        // other property mappings
    }
}
```
