---
uid: 8b9910e6-8c0e-473a-b5d4-daa9459b85b0
name: entity-behavior
description: rules for implementing domain behavior and invariant enforcement on entities
domain: skill
type: template
tags:
  - dotnet
  - domain
  - entity
  - behavior
  - invariants
  - skill/pattern/solution
triggers:
  - implement entity behavior
  - entity invariant enforcement
  - entity domain method
---
# Goal
Define how [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|entity-pattern.skill]] implement there behavior and enforces invariants. Without this pattern, invariant enforcement scatters across handlers, services, and controllers — the entity can be put into an invalid state from any caller.

# Core Principles
- Entity is the single point of truth for its own state validity
- Every method or setter that changes state must validate before changing
- Invalid state must never be reachable — throw `DomainException` if attempted
- Large or multi-entity behavior is extracted to [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-service.skill|domain-service.skill]]
- Rules logic is extracted to [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-rule.class.skill|domain-rule-pattern.skill]] to avoid duplication

# Patterns
## Setter with validation
Use when a property has a controlled write point.
```CSharp
public class SomeDomainEntity
{
	private uint _someField
    public uint SomeField { 
	    get => _someField;
		public set {
		    if (value == 0)
		        throw new DomainException("Invalid value");
		    _someField = value;
		} 
	}
}
```

## Behavior method
Use when state change has business meaning beyond setting a field.
```CSharp
public class SomeDomainEntity
{
	public uint SomeField { get; internal set; }
	public uint SomeMethodChangeEntityField(uint newValue){
	    if (value == 0)
			throw new DomainException("Invalid value");
		SomeField = newValue;
		return SomeField;
    }
}
```

## Extract to domain service
Large behavior logic could be extract to separate service in [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-service.skill|domain-service.skill]]
### To public domain service
```CSharp
public class SomeDomainEntity
{
	public uint SomeField { get; internal set; }
}

public class SomeDomainBusinessLogicService{
	public void MakeSomeChanges(SomeDomainEntity entity, uint newValue){
		if (value == 0)
			throw new DomainException("Invalid value");
		entity.SomeField = newValue;
	}
}
```
### To public static domain service
```CSharp
public class SomeDomainEntity
{
	public uint SomeField { get; internal set; }
}

public static class SomeDomainBusinessLogicService{
	public static void MakeSomeChanges(this SomeDomainEntity entity, uint newValue){
		if (string.IsNullOrWhiteSpace(newValue))
			throw new DomainException("Invalid value");
		entity.SomeField = newValue;
	}
}
```

# Rules
MUST
- Every property mutation validates state before assigning
- Public property setters, must prevent invalid state
- Public methods whose change properties, must prevent invalid state
- `DomainException` thrown when invariant is violated — never silently ignored
MUST NOT:
- Have multiple uncoordinated public mutation points for the same field
- Duplicate invariant logic across separate setters or methods
- Contain business workflow orchestration — that belongs in Application

# Anti-patterns
- Entity has several point of property change, each point has own state validation
- Property without state validation has public access

# Check list
- [ ] Entity prevent invalid state
- [ ] Every mutation validates before assigning
- [ ] `DomainException` thrown on invariant violation
- [ ] Complex logic extracted to domain-service.skill
- [ ] Unit test usecases implemented and passed

# Unittest TestCases
- [ ] When valid value set Then state changes correctly
- [ ] When invalid value set Then `DomainException` thrown
- [ ] When behavior method called with invalid args Then DomainException thrown

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|entity.skill]] — entity structure this behavior sits on
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-rule.class.skill|domain-rule-pattern.skill]] — reusable predicates used inside behavior methods
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-service.skill|domain-service.skill]] — extraction point for large or multi-entity behavior
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-event-pattern.skill|domain-event-pattern.skill]] — behavior methods raise domain events
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|entity-pattern.skill]] - base skill for entity implementation