---
name: class-entity-config
description: Class {Entity}Config in the v1 plateau
whenToUse: when mapping a new entity to the database, or adding a concurrency token, a Guid unique index, or timestamp columns to an existing mapping
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]]"
  - "[[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]]"
  - "[[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]]"
---

# Goal
- Own every persistence concern for one entity — table/index/constraint names, relations, Value Object mappings, concurrency token, unique `Guid` index, timestamp columns — so the entity class itself stays free of EF attributes

# Core Principles
- One `IEntityTypeConfiguration<T>` per entity, no exceptions; `TableName`/index/constraint names are `public const string` constants, never magic strings
- Only the fields the entity's classification actually has get mapped here — an Internal Immutable entity's config has no `Version`/`Guid`/update-timestamp mapping at all

# Implementation
```csharp
//Skill: class-entity-config
//Plateau: v1
//Version: 20260825140000

public sealed class OrderConfig : IEntityTypeConfiguration<Order>
{
    public const string TableName = "Orders";
    public const string UX_Guid = "UX_Order_Guid"; // only when the entity implements IHasGuid

    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(o => o.Id);

        builder.OwnsOne(o => o.Total); // multi-property Value Object

        // solution-entity-concurrency-change: only on IVersioned entities
        builder.Property(o => o.Version)
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate()
            .HasColumnName("xmin")
            .HasColumnType("xid");

        // solution-entity-edit-timestamp: only the columns the entity's timestamp interfaces require
        builder.Property(o => o.UserCreatedDateTime).IsRequired();
        builder.Property(o => o.ServerCreatedDateTime).IsRequired();
        builder.Property(o => o.UserUpdatedDateTime).IsRequired();
        builder.Property(o => o.ServerUpdatedDateTime).IsRequired();
    }
}

public sealed class AttachmentConfig : IEntityTypeConfiguration<Attachment>
{
    public const string TableName = "Attachments";
    public const string UX_Guid = "UX_Attachment_Guid";

    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(a => a.Id);

        // solution-external-created-entity: unique index is the last line of defence against duplicate creation
        builder.HasIndex(a => a.Guid).IsUnique().HasDatabaseName(UX_Guid);

        builder.Property(a => a.UserCreatedDateTime).IsRequired();
        builder.Property(a => a.ServerCreatedDateTime).IsRequired();
        // no UpdatedDateTime columns — External Immutable, never updated
    }
}
```

# Rules
MUST:
- Live in `{Module}.Domain/Configurations`, one class per entity
- Map `Version` with `IsConcurrencyToken()` + `ValueGeneratedOnAddOrUpdate()` only for `IVersioned` entities
- Map a unique index on `Guid` (named via a `public const string UX_Guid`) only for `IHasGuid` entities
- Map timestamp columns matching exactly the entity's implemented timestamp interfaces — no more, no fewer
- Use `OwnsOne` for every multi-property Value Object
MUST NOT:
- Configure cross-module foreign keys here — belongs in `App.Infrastructure/Persistence/Configurations`
- Map `Version`/`Guid`/update-timestamp columns for an entity classification that forbids them

# Check list
- [ ] One config class per entity, `TableName`/index names as `public const string`
- [ ] `Version` mapped with `IsConcurrencyToken()`/`ValueGeneratedOnAddOrUpdate()` only when `IVersioned`
- [ ] Unique `Guid` index only when `IHasGuid`
- [ ] Timestamp columns match the entity's implemented timestamp interfaces exactly
- [ ] Every multi-property Value Object mapped with `OwnsOne`

__Applied solutions:__
- [[../../../../../solutions/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[../../../../../solutions/solution-domain-configuration.skill/Implementation/{Module}.Domain.csproj.extend/{Entity}Config.cs.create.md|{Entity}Config.cs.create]]
- [[../../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../../solutions/solution-entity-concurrency-change.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[../../../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] - [[../../../../../solutions/solution-external-created-entity.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
- [[../../../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] - [[../../../../../solutions/solution-entity-edit-timestamp.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend.md|{EntityName}Config.cs.extend]]
