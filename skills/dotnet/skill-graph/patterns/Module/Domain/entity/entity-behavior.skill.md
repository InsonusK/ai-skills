---
uid: 8b9910e6-8c0e-473a-b5d4-daa9459b85b0
status: draft
name: behavior
description: Rules of entity encapsulation behavior pattern
domain: skill
type: pattern
tags:
  - entity
  - behavior
triggers:
  - implement entity befaviour
---
# Goal
Define how [[entity-pattern.skill|Entity]] implement there behavior
# Core Principles
- Entity realize invariant enforcement
- Domain entity realize domain behavior
- Before implement entity behavior define business logic

# Define before implementation
- How should entity work
- Where state is valid

# Structure / Contracts
## Configure access to behavior
## Implement invariant enforcement
- All setters and methods must prevent to invalid state off entity
- If property has public setter, it must prevent invalid state
```CSharp
public class SomeDomainEntity
{
	private uint _someField
    public uint SomeField { 
	    get => _someField;
		internal set {
		    if (string.IsNullOrWhiteSpace(value))
		        throw new DomainException("Invalid name");
		    _someField = value
		} 
	}
}
```
- If property has private setter and has public method which change it state. Method must prevent invalid state
```CSharp
public class SomeDomainEntity
{
	public uint SomeField { get; internal set; }
	internal uint SomeMethodChangeEntityField(uint newValue){
	    if (string.IsNullOrWhiteSpace(newValue))
			throw new DomainException("Invalid value");
		SomeField = newValue
    }
}
```
## Split behavior to service
Large behavior logic could be extract to separate service in [[domain-csproj]]
- service class
```CSharp
public class SomeDomainEntity
{
	public uint SomeField { get; internal set; }
}

public class SomeDomainBusinessLogicService{
	public void MakeSomeChanges(SomeDomainEntity entity, uint newValue){
		if (string.IsNullOrWhiteSpace(newValue))
			throw new DomainException("Invalid value");
		entity.SomeField = newValue
	}
}
```
- extension method
```CSharp
public class SomeDomainEntity
{
	public uint SomeField { get; internal set; }
}

public static class SomeDomainBusinessLogicService{
	public static void MakeSomeChanges(this SomeDomainEntity entity, uint newValue){
		if (string.IsNullOrWhiteSpace(newValue))
			throw new DomainException("Invalid value");
		entity.SomeField = newValue
	}
}
```
# Rules
MUST
- Entity prevent invalid state 
- Every setter or methods which is not implement state validation must be internal
MUST NOT:
- Property has many points of changes, each point implement  invariant enforcement

# Anti-patterns
- Entity has several point of property change, each point has own state validation
- Property without state validation has public access

# Check list
- [ ] Entity prevent invalid state
- [ ] Entity realize domain behavior
- [ ] unit test usecases implemented and passed
	- [ ] When change entity to invalid state Then raise DomainException 
	- [ ] When call entity behavior Then get expected result