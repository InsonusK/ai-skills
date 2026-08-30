---
tags:
  - solution/entity-classification
  - element/entityname-config-cs
---

# {EntityName}Config.cs - extend

Extend the entity configuration to match the selected classification. Only the mappings required by the classification are added.

## Internal Immutable

No changes beyond the standard Id mapping.

**Dependencies**: do not implement `solution-entity-concurrency-change.skill` or `solution-external-created-entity.skill`.

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

**Dependencies**: implement `solution-external-created-entity.skill`; do not implement `solution-entity-concurrency-change.skill`.

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

**Dependencies**: implement `solution-entity-concurrency-change.skill`; do not implement `solution-external-created-entity.skill`.

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

**Dependencies**: implement both `solution-entity-concurrency-change.skill` and `solution-external-created-entity.skill`.

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

# Rule changes

## MUST
- Classify every domain entity into exactly one of the four types before writing its code, configuration, or API contract.
- Document the classification decision for every entity in a discoverable location (e.g., entity config XML comment, module ADR, or team wiki).

## SHOULD
- Name the classification in entity configuration comments or a dedicated `ENTITY_CLASSIFICATION.md` per module.
