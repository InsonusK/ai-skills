---
uid: 63ee3f0b-7b1e-4a5d-b0b1-5b0ef7a8aa00
name: domain-configuration-pattern
description: rules for implementing EF Core entity type configurations in the Domain project
domain: skill
type: template
version: 20260609
tags:
  - dotnet
  - domain
  - ddd
  - ef-core
  - configuration
  - persistence-mapping
  - skill/template/class
triggers:
  - entity framework configuration
  - entity type configuration
  - entity mapping
  - database schema definition
  - index configuration
  - persistence mapping
aliases:
  - EF Configuration
  - EntityTypeConfiguration
  - Configuration
---
# Goal
Define how to write EF Core entity type configurations. One configuration class owns all persistence concerns for one entity — table name, indexes, relations, concurrency tokens, and value object mappings. The domain entity itself has zero EF attributes. Without this, mapping logic scatters and constraint names become magic strings impossible to reference in error handling or tests.

# Core Principles
- One `IEntityTypeConfiguration<T>` per entity — no exceptions
- All index names are `public static string` constants on the config class — never magic strings
- Domain entity must have zero EF attributes (`[Column]`, `[Index]`, etc.)
- Configuration is the only place that knows about column names, table names, and constraints
- Configuration registered via assembly scan — never manually per entity
- Cross-module foreign keys are configured in [[skills/dotnet/skill-graph/developing/App/Infrastructure csproj/class/app-infreastructure-ef-configuration.skill.md|App.Infrastructure EF configuration]] only

# Governed by
- [[skills/dotnet/skill-graph/developing/Architecture/solution/cross-module-communication.solution.skill|cross-module-relation-ef-configuratio.solution.skill]] - define that cross module relation define in [[skills/dotnet/skill-graph/developing/App/Infrastructure csproj/app-infrastructure.csproj.skill|app-infrastructure.csproj.skill]]
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/entity-concurrency-pattern.skill|entity-concurrency-pattern.skill]] — define configuration Version field as `IsConcurrencyToken`
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/external-created-entity.skill|external-created-entity.skill]] — define requirement of unique index on `Guid` field

# Structure
## Place in csproj
Defined in [[skills/dotnet/skill-graph/developing/Module/Domain csproj/module-domain-csproj.skill#Structure|module-domain-csproj.skill]]
```
/{ModuleName}.Domain
	/Configurations
		TaskConfig.cs
    	OrderConfig.cs
	{ModuleName}.Domain.csproj
```

## Naming convention
- class name
	- rule: EntityName + Config
	- pattern: {EntityName}Config
	- example: TodoTaskConfig
- file name:
	- rule: EntityName + .Config.cs
	- pattern: {EntityName}.Config.cs
	- example: TodoTask.Config.cs

## Implementation

### Base shape
```CSharp
public class TodoTaskConfig : IEntityTypeConfiguration<TodoTask>
{
    public static string TableName = nameof(TodoTask);

    public void Configure(EntityTypeBuilder<TodoTask> entityBuilder)
    {
        // indexes, relations, mappings go here
    }
}
```

### Index definition
Index names are static constants on the config class — referenced in tests and error handling.
```CSharp
public class TodoTaskConfig : IEntityTypeConfiguration<TodoTask>
{
    public static string TableName = nameof(TodoTask);
    public static string UX_Guid = $"UX_{TableName}_Guid";

    public void Configure(EntityTypeBuilder<TodoTask> entityBuilder)
    {
        entityBuilder
            .HasIndex(e => e.Guid)
            .IsUnique()
            .HasDatabaseName(UX_Guid);
    }
}
```

### Relation definition
```CSharp
entityBuilder
    .HasMany(e => e.SubTasks)
    .WithOne(e => e.Task)
    .HasForeignKey(e => e.TaskId)
    .IsRequired()
    .OnDelete(DeleteBehavior.Cascade);
```

### Multi-property Value Object mapping (OwnsOne)
Required for any entity that holds a multi-property VO — see [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/value-object.skill.skill]].
```CSharp
entityBuilder.OwnsOne(e => e.Cash, money =>
{
    money.Property(m => m.Amount).HasColumnName("Cash_Amount");
    money.Property(m => m.Currency).HasColumnName("Cash_Currency").HasMaxLength(3);
});
```

### Concurrency token (RowVersion)
Required for all mutable entities — see [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/entity-concurrency-pattern.skill]].
```CSharp
entityBuilder
    .Property(e => e.Version)
    .HasColumnName("xmin")
    .IsConcurrencyToken()
    .ValueGeneratedOnAddOrUpdate();
```

### Registration in DbContext
All configurations are registered via assembly scan — never manually per entity.
```CSharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(TodoTaskConfig).Assembly);
}
```

# Rules

MUST:
- one config class per entity
- all index names defined as `public static string` constants on the config class
- `OwnsOne` configured for every multi-property Value Object property
- `IsConcurrencyToken()` configured for every mutable entity `Version` field
- registered via `ApplyConfigurationsFromAssembly` 
MUST NOT:
- use EF data annotations on domain entity (`[Column]`, `[Index]`, `[ForeignKey]`, etc.)
- define table names, column names, or constraint names as inline strings
- put mapping logic in `DbContext.OnModelCreating` directly
- Configure cross-module foreign keys here — that belongs in App.Infrastructu

# Anti-patterns
- Mapping multi-property VO properties individually without `OwnsOne` — EF will fail to map or create a shadow table
- Hardcoded index name strings — breaks error handling that matches constraint names
- Skipping concurrency token config on mutable entity — concurrency control silently does nothing
- Using `[ConcurrencyCheck]` attribute on entity instead of fluent config

# Checklist
- [ ]  One config class exists per entity
- [ ]  Table-level index names defined as `public static string` constants
- [ ]  All unique indexes configured with `HasDatabaseName(ConstantName)`
- [ ]  All relations configured (`HasMany`/`HasOne`, `HasForeignKey`, `OnDelete`)
- [ ]  `OwnsOne` configured for every multi-property VO property
- [ ]  `Version` property configured as `IsConcurrencyToken()` if entity is mutable
- [ ]  No EF attributes on the domain entity class

# Unittest TestCases
- [ ]  When insert entity with duplicate `Guid` Then throws `DbUpdateException` with correct constraint name
- [ ]  When two contexts update same mutable entity concurrently Then second throws `DbUpdateConcurrencyException`
- [ ]  When insert entity with multi-property VO Then columns are persisted flat on entity table

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|entity.skill]] — defines which entity types require which configuration pieces
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/entity-concurrency-pattern.skill|entity-concurrency-pattern.skill]] — requires Version field registered as `IsConcurrencyToken`
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Solutions/external-created-entity.skill|external-created-entity.skill]] — requires unique index on `Guid` field
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/value-object.skill.skill|value-object-pattern.skill]] — multi-property VOs require `OwnsOne` mapping here