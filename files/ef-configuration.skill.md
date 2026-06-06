---
name: ef-configuration
description: rules for implementing EF Core entity type configurations in the Domain project
domain: skill
type: pattern
tags:
  - dotnet
  - domain
  - ef-core
  - configuration
  - persistence-mapping
triggers:
  - EF entity configuration
  - entity type configuration
  - index configuration
  - persistence mapping
---
# Goal
Define how to write EF Core entity type configurations. One configuration class owns all persistence concerns for one entity — table name, indexes, relations, concurrency tokens, and value object mappings. The domain entity itself has zero EF attributes. Without this, mapping logic scatters and constraint names become magic strings impossible to reference in error handling or tests.

# Core Principles
- One `IEntityTypeConfiguration<T>` per entity — no exceptions
- All index names are `public static string` constants on the config class
- Domain entity has zero EF data annotations
- Configuration registered via assembly scan — never manually per entity
- Cross-module foreign keys are configured in App.Infrastructure only

# File Location
```
/{ModuleName}.Domain
  /Configurations
    TaskConfig.cs
    OrderConfig.cs
```

## Base shape
```csharp
public class TaskConfig : IEntityTypeConfiguration<Task>
{
    public static string TableName = nameof(Task);

    public void Configure(EntityTypeBuilder<Task> builder)
    {
        // indexes, relations, mappings
    }
}
```

## Unique index (required for external-created-entity.skill)
```csharp
public class TaskConfig : IEntityTypeConfiguration<Task>
{
    public static string TableName = nameof(Task);
    public static string UX_Guid = $"UX_{TableName}_Guid";

    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder
            .HasIndex(e => e.Guid)
            .IsUnique()
            .HasDatabaseName(UX_Guid);
    }
}
```

## Concurrency token (required for entity-concurrency.skill)
```csharp
builder
    .Property(e => e.Version)
    .HasColumnName("xmin")
    .IsConcurrencyToken()
    .ValueGeneratedOnAddOrUpdate();
```

## Relation
```csharp
builder
    .HasMany(e => e.SubTasks)
    .WithOne(e => e.Task)
    .HasForeignKey(e => e.TaskId)
    .IsRequired()
    .OnDelete(DeleteBehavior.Cascade);
```

## Multi-property Value Object (required for value-object.skill)
```csharp
builder.OwnsOne(e => e.Cash, money =>
{
    money.Property(m => m.Amount).HasColumnName("Cash_Amount");
    money.Property(m => m.Currency).HasColumnName("Cash_Currency").HasMaxLength(3);
});
```

## Registration in DbContext
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(TaskConfig).Assembly);
}
```

# Rules
MUST:
- One config class per entity
- Index names as `public static string` constants
- `OwnsOne` for every multi-property Value Object property
- `IsConcurrencyToken()` for every mutable entity `Version` field
- Registered via `ApplyConfigurationsFromAssembly`
MUST NOT:
- Use EF data annotations on the domain entity
- Define constraint names as inline strings
- Configure cross-module foreign keys here — that belongs in App.Infrastructure

# Checklist
- [ ] One config class per entity in `/Configurations`
- [ ] Index names as `public static string` constants
- [ ] `OwnsOne` for all multi-property VO properties
- [ ] `Version` configured as `IsConcurrencyToken()` if entity is mutable
- [ ] No EF annotations on domain entity class

# Unittest TestCases
- [ ] When entity with duplicate Guid inserted Then DbUpdateException with correct constraint name
- [ ] When mutable entity updated concurrently Then DbUpdateConcurrencyException thrown
- [ ] When entity with multi-property VO saved Then columns flat on entity table

# Relations
- entity.skill — determines which configuration pieces are required per entity type
- entity-concurrency.skill — Version field configuration
- external-created-entity.skill — Guid unique index configuration
- value-object.skill — multi-property VOs require OwnsOne here
