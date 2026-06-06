---
uid: 79cc5145-5598-4499-9648-47f328a77ad2
status: implemented
name: external-created-entity
description: rules for adding the Guid field and unique index to entities created by an external system
domain: skill
type: pattern
tags:
  - dotnet
  - domain
  - entity
  - guid
  - idempotency
  - external
  - created
  - skill/pattern/solution
triggers:
  - implement externally created entity
  - add Guid to entity
  - client-generated Guid
  - idempotent creation
---
# Goal
Define how to add a client-generated Guid to an entity that is created by an external system. The Guid is a correlation handle — it lets the external system retry creation safely. The unique index enforces idempotency at the database level. Without this, retried requests create duplicate entities.

# Core Principles
- External system generates the Guid before the entity exists — backend never generates it
- Guid is immutable after creation — set once, never changed
- Guid is a correlation handle only — never used in domain logic or relations
- Unique index on Guid is the database-level idempotency guard
- Internal `Id` is still the only identity used inside the domain after creation
- Follow [[skills/dotnet/skill-graph/developing/App/Infrastructure Layer/async-external-creation.skill|async-external-creation.skill]] solution

# Solution
## Setup Domain — Guid Field 
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|Entity]] must implement `Guid` property
```CSharp
public class SomeExternalCreatedEntity
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }
}
```
## EF Configuration — Add Unique Index
- Inside [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-configuration-pattern.skill|domain-configuration-pattern.skill]] must implement unique index on field Guid
- Index name is a constant so it can be referenced in error handling and tests.
```CSharp
public class SomeExternalCreatedEntityConfig 
	: IEntityTypeConfiguration<SomeExternalCreatedEntity>
{
	public static string TableName = nameof(SomeExternalCreatedEntityConfig);
    public static string UX_Guid = $"UX_{TableName}_Guid";
    
    public void Configure(
	    EntityTypeBuilder<SomeExternalCreatedEntity> entityBuilder)
    {
        entityBuilder
          .HasIndex(e => e.Guid)
          .IsUnique()
          .HasDatabaseName(UX_Guid);
    }
}
```
## Setup Concurrency validation
- Implemented [[skills/dotnet/skill-graph/developing/Module/Application Layer/guid-resolving-pipeline.skill|guid-resolving-pipeline]]

# Rules
MUST:
- `Guid Guid { get; internal set; }` present on entity
- Unique index configured with a named constant
- Guid immutable after creation — no setter or method changes it after `Create()`
- Use Guid ONLY as correlation handle for external system
- keep the domain decoupled from external identifiers
SHOULD NOT:
- Use Guid in domain logic, specifications, or relations
- Expose Guid as a public setter
- Generate Guid server-side for external creation flows
# Anti-patterns
- Change `Guid` after creation

# Check list
- [ ] `Guid Guid { get; internal set; }` added
- [ ] Unique index configured in EF configuration class
- [ ] Index name stored as `public static string` constant on config class
- [ ] Guid set only during entity creation — no subsequent mutation
- [ ] [[skills/dotnet/skill-graph/developing/Module/Application Layer/guid-resolving-pipeline.skill|guid-resolving-pipeline.skill]] implemented

# Unittest TestCases
- [ ] When entity inserted with duplicate `Guid` Then `DbUpdateException` thrown with `UX_Guid` constraint name
- [ ] When entity created Then `Guid` matches the value provided at creation

# Relations
- [[skills/dotnet/skill-graph/developing/App/Infrastructure Layer/async-external-creation.skill|async-external-creation.skill]] - architecture pattern for async creation
- [[skills/dotnet/skill-graph/developing/Module/Application Layer/guid-resolving-pipeline.skill|guid-resolving-pipeline]] -  pipeline behavior that checks Guid before handler runs
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|entity.skill]] — identifies External Immutable and External Mutable as requiring this pattern
- [[files/ef-configuration.skill|ef-configuration.skill]] — unique index goes in the entity's EF configuration class