---
uid: 7457c571-268f-4968-a90d-2ee3a0d58314
status: draft
name: domain-rule-pattern
description: rules for designing reusable domain rules and semantic predicates
domain: skill
type: pattern
tags:
  - dotnet
  - domain
  - validation
  - ddd
  - rules
triggers:
  - domain rule design
  - value object validation
  - entity invariant design
  - business rule extraction
aliases:
  - Domain Validation Rule
  - ValueObject Validator Rule
---
# Goal
Define a unified pattern for implementing reusable domain rules.

**Rules represent:**

- semantic predicates
- business conditions
- reusable invariant checks

**Rules are framework-independent and reusable across:**
- ValueObjects
- Entities
- DomainServices
- Application validation adapters
- Specifications
- Authorization policies
# Core principle
- Rule defines business meaning.  
- Rule does NOT define transport validation or framework behavior.

## Structure / Contracts
__Project structure:__
- /Domain
	- /Rules
		- IsPositiveRule.cs
		- IsAdultRule.cs
		- CanDriveCarRule.cs
## [[value-object-pattern.skill|Value Object]] Rule
Rules for [[value-object-pattern.skill|Value Object]] implements as Extension classes
__Example:__
```CSharp
public static class SomeValueObjectRule{
	public static bool IsOld(this SomeValueObject age){
		return age > 90;
	}
}
```
## Contextual Rules
Rules depending on multiple values.
__Example__
```CSharp
public static class CanDriveCarRule 
{
    public bool IsSatisfied(this (Age Age, Country Country) value)
    {
        return value.Country.Code switch
        {
            "US" => value.Age.Value >= 16,
            "NL" => value.Age.Value >= 18,
            _ => false
        };
    }
}
```

# Rules
__MUST__
- contain only business logic
- be deterministic
- avoid side effects
- avoid infrastructure access
- avoid framework dependency
__MUST NOT__
- access database
- use FluentValidation
- use HttpContext
- use external services directly
- mutate objects

# 7. Rule usage inside ValueObjects
ValueObjects may use Rules to enforce invariants.

__Example__
```
public record Age
{
    public int Value { get; }

    public Age(int value)
    {
        if (!new IsPositiveRule().IsSatisfied(value))
            throw new DomainException("Age must be positive");

        Value = value;
    }
}
```

# 8. Rule usage inside Entities
Entities may use Rules internally for invariant protection.
__Example__
```CSharp
public class User
{
    public Age Age { get; private set; }

    public void SetAge(Age age)
    {
        if (!new IsAdultRule().IsSatisfied(age))
            throw new DomainException("User must be adult");

        Age = age;
    }
}
```

# 9. Rule usage inside Domain Services
Rules may be reused by Domain Services.
__Example__
```CSharp
public class DriverDomainService
{
    private readonly CanDriveCarRule _rule = new();

    public bool CanDrive(Age age, Country country)
    {
        return _rule.IsSatisfied((age, country));
    }
}
```

# 10. Rule organization
__Structure__

# 11. Naming rules
__Rule class naming:__

```
{Condition}Rule
```

__Examples:__
- IsPositiveRule
- IsAdultRule
- CanDriveCarRule
- HasPermissionRule

# 12. Rule responsibility boundaries
__Rules define:__
- business truth
- semantic validity
- invariant predicates

__Rules do NOT define:__
- API validation
- DTO validation
- serialization validation
- HTTP validation
- UI validation

# 13. Performance rules
__Rules SHOULD:__
- avoid allocations when possible
- avoid async execution
- avoid IO

__Rules SHOULD be:__
- lightweight
- synchronous
- reusable
# 14. Anti-patterns
__Forbidden__
- database queries inside rules
- dependency on EF Core
- dependency on FluentValidation
- dependency on ASP.NET
- mutable rule state
- throwing exceptions for normal validation flow

# 15. Key architectural principle
```
Rules define business predicates.
Entities define business consistency.
ValueObjects define immutable correctness.
```
# 16. Design outcome
This pattern provides:
- reusable semantic validation
- deterministic business predicates
- framework-independent domain logic
- AI-friendly domain structure
- composable invariant system