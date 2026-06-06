---
uid: 9d4d8583-80ca-4e87-891d-a709cc9ade17
name: value-object-pattern
description: rules for designing and implementing domain value objects
domain: skill
type: pattern
tags:
  - dotnet
  - domain
  - ddd
  - value-object
  - skill/pattern/class
triggers:
  - value object design
  - domain modeling
  - immutable types
  - eliminate primitive obsession
  - domain type with invariants
aliases:
  - Value Object
  - ValueObjects
  - VO
---
# Goal
Eliminate primitive obsession by encoding domain semantics into types. A Value Object owns its invariants, normalization, and equality — preventing invalid state from ever entering the domain model. Replaces raw primitives (string, int, decimal) where those primitives carry business meaning (Age, Money, Email, Percentage).

# Core Principles
- Value Object is `sealed record` — structural equality by default
- Invariants enforced in constructor — invalid state cannot be constructed
- Value Objects are immutable — no public setters
- Value Object represents a single, meaningful concept with invariants
- Equality is structural, not referential
- Multi-property VOs require a private parameterless constructor for EF materialization
- Multi-property VOs require `OwnsOne` EF mapping — see [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-configuration-pattern.skill#Multi-property Value Object mapping (OwnsOne)|domain configuration multi property mapping]]

# Place in csproj
Defined in [[skills/dotnet/skill-graph/developing/Module/Domain csproj/module-domain-csproj.skill#Structure|module-domain-csproj.skill]]
```
/{ModuleName}.Domain
	/ValueObjects
		Age.cs
		Money.cs
	{ModuleName}.Domain.csproj
```

# Contracts

## Single-property Value Object
```CSharp
public sealed record Age
{
    public int Value { get; }

    public Age(int value)
    {
        if (value <= 0 || value > 120)
            throw new DomainException("Invalid Age");
        Value = value;
    }

    public static implicit operator int(Age obj) => obj.Value;
    public static implicit operator Age(int value) => new(value);
    public override string ToString() => Value.ToString();
}
```

## Multi-propery Value Object
```csharp
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new DomainException("Amount cannot be negative");
        if (string.IsNullOrEmpty(currency)) throw new DomainException("Currency required");

        Amount = amount;
        Currency = currency;
    }

    private Money() { } // EF Core materialization only

    public override string ToString() => $"{Amount} {Currency}";
}
```

# Rules
MUST:
- be `sealed record`
- be immutable
- Validate all invariants in constructor
- No public setters
- Multi-property VO has private parameterless constructor
SHOULD:
- Single-property VO has implicit conversion operators
- Use [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-rule-pattern.skill|domain-rule.skill]] for complex invariant predicates
- have implicit operator for single-property VOs
- encapsulate normalization and formatting
- represent semantically meaningful concepts
MUST NOT:
- Depend on infrastructure or application services
- Be mutable

# Anti-patterns
- Using primitives for domain concepts with meaning (string, int, decimal everywhere)
- Allowing invalid state to exist in Value Object
- Making Value Objects mutable
- Adding infrastructure or framework dependencies
- Creating Value Objects without real semantic purpose

# Check list
- [ ] Declared as `sealed record`
- [ ] Validates all invariants in constructor  
- [ ] No public setters (immutable)
- [ ] No infrastructure or service dependencies
- [ ] Equality based on value, not identity 
- [ ] ToString() implemented if used in logs/UI
- [ ] Linked to [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-rule-pattern.skill|domain-rule-pattern.skill]] for complex invariants
- [ ]  Multi-property VO has `private` parameterless constructor
- [ ]  Multi-property VO has `OwnsOne` EF configuration in entity mapping

# Unittest TestCases
- [ ] When create invalid ValueObject Then raise Exception
- [ ] When create valid ValueObject Then object created
- [ ] When value is below lower bound Then throws DomainException 
- [ ] When value is above upper bound Then throws DomainException 
- [ ] When value is boundary (min/max valid) Then object created 
- [ ] Two VOs with same value are equal 
- [ ] Implicit conversion round-trips losslessly (if operators defined)

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-rule-pattern.skill|domain-rule-pattern.skill]] - implementation of value object invariant validation rule