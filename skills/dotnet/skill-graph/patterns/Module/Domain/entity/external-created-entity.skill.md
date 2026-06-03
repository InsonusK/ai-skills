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
Define how work with external created entities

# Core Principles
- If entity created by async request from external system, It must support working with guid
- Follow [[async-external-creation.skill]]
# Structure / Contracts
## Setup external id
- [[entity-pattern.skill|Entity]] must implement Guid property
```CSharp
public class SomeExternalCreatedEntity
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }
}
```
- Inside [[domain-configuration-pattern.skill]] must implement unique index on field Guid
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
- Implemented [[guid resolving pipeline]]
# Rules
MUST:
- Guid immutable after creation
SHOULD NOT:
- Use Guid for domain logic

# Anti-patterns
- Change guid after creation
- Use Guid in domain logic
# Check list
- [ ] Guid has been added
- [ ] Unique index has been configured
- [ ] [[concurency control pattern.skill]] implemented
- [ ] unit test usecases implemented and passed
	- [ ] Prevent duplication guid
		- When try add second entity with same Guid 
		- Then
			- raise DbUpdateException 
			- with ex.InnerException is PostgresException postgresException
			- and postgresException.SqlState == "23505" 
			- and postgresException.ConstraintName == SomeExternalCreatedEntityConfig .UX_Guid
