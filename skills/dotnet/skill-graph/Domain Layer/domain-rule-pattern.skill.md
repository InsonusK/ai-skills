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
  - Rules
---
# Goal
Define a unified pattern for implementing reusable, framework-independent domain rules. A Rule encodes a single business predicate — a semantic condition that can be reused across Value Objects, Entities, Domain Services, and application adapters. Without this pattern, business conditions scatter into validators, controllers, and services — making the domain logic untestable and non-reusable.

# Core principle
- Rule defines business meaning, not transport or framework behavior
- Rule returns `bool` — the caller decides whether to throw
- Rules are stateless, deterministic, and side-effect free
- Rules define business predicates. Entities define consistency. Value Objects define correctness.
- Don't duplicate rule logic for primitives and value object

# Structure / Contracts
## Project structure
```
/Domain
	/Rules
		IsPositiveRule.cs
		IsAdultRule.cs
		CanDriveCarRule.cs
```
		  
## [[skills/dotnet/skill-graph/Domain Layer/value-object-pattern.skill|Value Object]] Rule
Rules scoped to a single Value Object are implemented as static extension methods on that VO type.
```CSharp
public static class AgeRules
{
    public static bool IsAdult(this Age age) => age.Value >= 18;
    public static bool IsOld(this Age age) => age.Value > 90;
}
```

## Contextual Rules
Rules depending on multiple values use a tuple extension method.
```CSharp
public static class CanDriveCarRule
{
    public static bool IsSatisfied(this (Age Age, Country Country) value)
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

## Duplicated rules
Same rule for different type of ValueObjects and primitives must have only one validation implementation
```CSharp
// Primitive rule is the single source of truth
public static class IntRules
{
    public static bool IsPositive(this int value) => value > 0;
}

// VO rules delegate to the primitive — no logic duplication
public static class AgeRules
{
    public static bool IsPositive(this Age age) => age.Value.IsPositive();
    public static bool IsPositive(this DogAge age) => age.Value.IsPositive();
}

// Contextual rule: primitive overload holds the logic
public static class CanDriveCarRule
{
    public static bool IsSatisfied(this (int Age, string Country) value)
    {
        return value.Country switch
        {
            "US" => value.Age >= 16,
            "NL" => value.Age >= 18,
            _ => false
        };
    }

    // VO overload delegates to primitive overload
    public static bool IsSatisfied(this (Age Age, Country Country) value)
        => (value.Age.Value, value.Country.Code).IsSatisfied();
}
```

## Rule usage inside [[skills/dotnet/skill-graph/Domain Layer/value-object-pattern.skill|Value Object]]
Value Objects call rules in their constructor to enforce invariants.
```CSharp
public sealed record Age
{
    public int Value { get; }

    public Age(int value)
    {
        if (!value.IsPositive())
            throw new DomainException("Age must be positive");

        Value = value;
    }
}

public static class IsPositiveAgeRule
{
    public static bool IsPositive(this int value) => value > 0;
    public static bool IsPositive(this Age age) => age.Value.IsPositive();
}
```

## Rule usage inside [[skills/dotnet/skill-graph/Domain Layer/entity/entity-pattern.skill|Entities]]
Entities call rules inside setters or behavior methods to protect invariants.
```CSharp
public class User
{
    public Age Age { get; internal set; }

    internal void SetAge(Age age)
    {
        if (!age.IsAdult())
            throw new DomainException("User must be adult");

        Age = age;
    }
}
```

## Rule usage inside [[skills/dotnet/skill-graph/Domain Layer/domain-service.skill|Domain Service]]
Domain Services compose rules for multi-entity or contextual business decisions.
```CSharp
public static class DriverDomainService
{
    public static bool CanDrive(Age age, Country country)
        => (age, country).IsSatisfied();
}
```

# Naming
Rule class naming: `{Condition}Rule` for contextual rules, `{Type}Rules` for VO extension classes.

|Type|Example|
|---|---|
|VO extension class|`AgeRules`, `MoneyRules`|
|Contextual rule|`CanDriveCarRule`, `HasPermissionRule`|
|Primitive rule|`IntRules`, `StringRules`|

# Rules
MUST:
- contain only business logic
- be deterministic
- be stateless
- return `bool` — never throw exceptions internally
- avoid side effects
- avoid infrastructure access
- avoid framework dependency
SHOULD:
- be synchronous
- avoid allocations
MUST NOT:
- depend on EF Core, FluentValidation, ASP.NET, or HttpContext
- use external services
- mutate objects

# Anti-patterns
- Rule throws `DomainException` itself — rule returns `bool`, caller throws
- Rule instantiated with `new Rule().IsSatisfied()` — use static extension methods
- Business condition duplicated across controller, service, and entity
- Rule depends on infrastructure (DbContext, HttpContext, IOptions)
- Rule has mutable state

# Checklist
- [ ] Rule contains only business logic (no infrastructure, no framework)
- [ ] Rule is deterministic and side-effect free
- [ ] Rule returns `bool` — does not throw
- [ ] Rule is named `{Condition}Rule` or `{Type}Rules`
- [ ] Rule placed in `/Domain/Rules/`
- [ ] Rule implemented as static extension method
- [ ] Unit tests cover all boundary conditions
- [ ] Rule doesn't duplicate each other

# Unittest TestCases
- [ ]  When value satisfies rule Then returns true
- [ ]  When value violates rule Then returns false
- [ ]  When boundary value (min/max) Then returns expected result
- [ ]  Rule is pure — same input always returns same output

# Relations
- [[skills/dotnet/skill-graph/Domain Layer/value-object-pattern.skill|value-object-pattern.skill]] — VOs use Rules for invariant validation in constructor
- [[skills/dotnet/skill-graph/Domain Layer/entity/entity-pattern.skill|entity-pattern.skill]] — Entities use Rules for invariant protection in setters and methods
- [[skills/dotnet/skill-graph/Domain Layer/domain-service.skill|domain-service.skill]] — Domain Services compose Rules for multi-value business decisions