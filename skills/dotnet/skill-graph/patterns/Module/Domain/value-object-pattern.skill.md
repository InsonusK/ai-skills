---
uid: 9d4d8583-80ca-4e87-891d-a709cc9ade17
status: draft
name: value-object-pattern
description: rules for designing and implementing domain value objects
domain: skill
type: pattern
tags:
  - dotnet
  - domain
  - ddd
  - value-object
triggers:
  - value object design
  - domain modeling
  - immutable types
aliases:
  - Value Object
  - ValueObjects
---
# Goal
Define how to model **Value Objects** in domain layer.

# Rules
## 1. Value Object = immutable concept
- No identity
- Equality by value
- Always immutable

## 2. Structure
Preferred implementation:
```C#
public record TaskName
{
    public string Value { get; }

    public TaskName(string value)
    {
        Value = value;
    }
}
```
or simplified:
```C#
public record TaskName(string Value);
```
## 3. Validation allowed in constructor
```C#
public record TaskName
{
    public string Value { get; }

    public TaskName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Task name is required");

        Value = value;
    }
}
```
## 4. Rules
- no setters
- no EF tracking logic
- no services injection
- no behavior depending on infrastructure
- small and focused

# Anti-patterns
- mutable VO  
- VO with DbContext usage  
- VO with external dependencies
# Check list
{{ Check list what agent must done and which artifacts create while using skill. Check list is using to validate that agent follow the skill }}
