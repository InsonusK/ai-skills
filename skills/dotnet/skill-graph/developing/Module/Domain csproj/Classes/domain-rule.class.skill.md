---
uid: 7457c571-268f-4968-a90d-2ee3a0d58314
name: domain-rule-pattern
description: rules for implementing reusable domain predicates as static extension methods
domain: skill
type: template
version: 20260609
tags:
  - dotnet
  - domain
  - validation
  - ddd
  - rules
  - predicates
  - skill/template/class
triggers:
  - domain rule design
  - business predicate
  - reusable invariant logic
aliases:
  - Domain Validation Rule
  - ValueObject Validator Rule
  - Rules
  - Domain rule
---
# Goal
Define a unified pattern for implementing reusable, framework-independent domain rules. A Rule encodes a single business predicate — a semantic condition that can be reused across Value Objects, Entities, Domain Services, and application adapters. Without this pattern, business conditions scatter into validators, controllers, and services — making the domain logic untestable and non-reusable.

# Core principle
- Rule defines business meaning, not transport or framework behavior
- Rule returns `bool` — the caller decides whether to throw
- Rules are stateless, deterministic, and side-effect free
- Rules have no infrastructure or framework dependencies

# Structure
## Place in csproj
Defined in [[skills/dotnet/skill-graph/developing/Module/Domain csproj/module-domain-csproj.skill#Structure|module-domain-csproj.skill]]
```
/{ModuleName}.Domain
	/Rules
		IntRules.cs
		AgeRules.cs
		CanDriveCarRule.cs
	{ModuleName}.Domain.csproj
```

## Naming convention
Rule class naming: `{Condition}Rule` for contextual rules, `{Type}Rules` for VO extension classes.

| Type               | Example                                |
| ------------------ | -------------------------------------- |
| VO extension class | `AgeRules`, `MoneyRules`               |
| Contextual rule    | `CanDriveCarRule`, `HasPermissionRule` |
| Primitive rule     | `IntRules`, `StringRules`              |

## Implementation	  
### Value Object Rule
Rules scoped to a single [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/value-object.skill.skill|Value Object]] are implemented as static extension methods on that VO type.
```CSharp
public static class AgeRules
{
    public static bool IsAdult(this Age age) => age.Value >= 18;
    public static bool IsOld(this Age age) => age.Value > 90;
}
```
### Primitive rule — single source of truth
Primitive rule holds the logic. [[#Value Object Rule|VO rules]] delegate to it — no duplication.
```csharp
public static class IntRules
{
    public static bool IsPositive(this int value) => value > 0;
}

public static class AgeRules
{
    public static bool IsPositive(this Age age) => age.Value.IsPositive();
}
```
### Contextual Rules — multiple values
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

### Duplicated rules
Same rule for different type of [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/value-object.skill.skill|Value Object]] and primitives must have only one validation implementation
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


# Rules
MUST:
- be stateless and deterministic
- return `bool` — never throw exceptions internally
- Be implemented as static extension methods
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
- [ ] No infrastructure dependency

# Unittest TestCases
- [ ]  When value satisfies rule Then returns true
- [ ]  When value violates rule Then returns false
- [ ]  When boundary value (min/max) Then returns expected result
- [ ]  Rule is pure — same input always returns same output

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/value-object.skill.skill|value-object-pattern.skill]] — VOs use Rules for invariant validation in constructor
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|entity-pattern.skill]] — Entities use Rules for invariant protection in setters and methods
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-service.skill|domain-service.skill]] — Domain Services compose Rules for multi-value business decisions