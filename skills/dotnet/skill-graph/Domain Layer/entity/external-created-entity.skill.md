---
uid: 79cc5145-5598-4499-9648-47f328a77ad2
status: implemented
name: external-created-entity
description: Implementation of external guid and work with external created entities
domain: skill
type: pattern
tags:
  - external
  - created
triggers:
  - develop external created entity
---
# Goal
Define how work with external created entities to prevent id leaking, double creation and support async creation.

# Core Principles
- If entity created by async request from external system, It must support working with guid
- Follow [[skills/dotnet/skill-graph/Infrastructure Layer/async-external-creation.skill|async-external-creation.skill]]

# Structure / Contracts
## Setup external id
- [[skills/dotnet/skill-graph/Domain Layer/entity/entity-pattern.skill|Entity]] must implement Guid property
```CSharp
public class SomeExternalCreatedEntity
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }
}
```
- Inside [[skills/dotnet/skill-graph/Domain Layer/domain-configuration-pattern.skill]] must implement unique index on field Guid
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
- Implemented [[skills/dotnet/skill-graph/Application Layer/guid-resolving-pipeline.skill|guid-resolving-pipeline]]

# Rules
MUST:
- Guid immutable after creation
- Use Guid ONLY as correlation handle for external system
- keep the domain decoupled from external identifiers
SHOULD NOT:
- Use Guid for domain logic

# Anti-patterns
- Change guid after creation
- Use Guid in domain logic.

# Check list
- [ ] Guid has been added
- [ ] Unique index has been configured
- [ ] [[skills/dotnet/skill-graph/Domain Layer/entity/external-created-entity.skill|external-created-entity.skill]] implemented

# Unittest TestCases
- [ ] Prevent duplication guid
	- When try add second entity with same Guid 
	- Then
		- raise DbUpdateException 
		- with ex.InnerException is PostgresException postgresException
		- and postgresException.SqlState == "23505" 
		- and postgresException.ConstraintName == SomeExternalCreatedEntityConfig .UX_Guid

# Relations
- [[skills/dotnet/skill-graph/Infrastructure Layer/async-external-creation.skill|async-external-creation.skill]] - architecture pattern for async creation
- [[skills/dotnet/skill-graph/Application Layer/guid-resolving-pipeline.skill|guid-resolving-pipeline]] - define implementation of resolving entity id from guid