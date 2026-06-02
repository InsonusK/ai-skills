---
uid: 0d4baf72-1689-49d3-81e8-5ee93b22bb28
status: draft
name: entity-concurency-pattern
description: Describe how work entity concurency
domain: skill
type: pattern
tags:
  - entity
  - editable
  - concurency
  - rowversion
triggers:
  - develop editable entity
---
# Goal
Define how work version concurency in domain

## Core Principles
- If entity is editable it must have concurency field to prevent changes by old data

## Structure / Contracts
### Setup concurrency field
Inside [[entity-pattern.skill|Entity]]
```C#
public class SomeEditableDomainEntity
{
    public uint Version { get; set; }
}
```
Inside [[domain-configuration-pattern.skill]]
```C#
public class SomeEditableDomainEntityConfig 
	: IEntityTypeConfiguration<SomeEditableDomainEntity>
{
    public void Configure(
	    EntityTypeBuilder<SomeEditableDomainEntity> entityBuilder)
    {
        entityBuilder
          .Property(e => e.Version)
          .HasColumnName("xmin") 
          .IsConcurrencyToken()
          .ValueGeneratedOnAddOrUpdate();
    }
}
```
### Setup Concurrency validation
- Realize [[concurency control pattern.skill]]
## Rules
MUST:
- Entity must have RowVersion field
- Field must be setuped in EF Configuration
- [[entity-concurency-pattern.skill]] must be applyed

## Anti-patterns
- Use timestamp instead RowVersion
