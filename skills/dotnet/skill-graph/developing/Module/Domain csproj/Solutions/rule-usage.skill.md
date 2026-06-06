---
uid: 2e7f8260-57a9-4e55-853e-7a7267251294
name: skill-name
description: Describe what skill define
domain: skill
type: pattern
tags:
  - dotnet
  - domain
  - entity
  - validation
  - value-object
  - rules
  - skill/pattern/solution
triggers:
  - use rule in value object
  - add validation to value object
  - use rule in entity
  - add validation to entity
---
# Goal
Define solution that [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-rule-pattern.skill|Domain Validation Rule]] are reused in [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/value-object-pattern.skill|Value Object]], [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|Entity]], [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-service.skill|Domain Service]] validation

# Core Principles
- Rule is single source of truth per predicate — VO and entity rules delegate to primitive rules
- Don't duplicate rule logic for primitives and value object
# Affected objects
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-rule-pattern.skill|Rules]] - reused in [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|Entity]] and [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/value-object-pattern.skill|Value Object]]
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|Entity]] - use [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-rule-pattern.skill|Rules]] to prevent invalid state
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/value-object-pattern.skill|Value Object]] - use [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-rule-pattern.skill|Rules]] to prevent invariants
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-service.skill|Domain Service]] - use [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-rule-pattern.skill|Rules]] to prevent invalid state
- [[shared-layer.skill]] - extraction point of common rules for several [[skills/dotnet/skill-graph/developing/Module/Domain csproj/module-domain-csproj.skill|module-domain-csproj.skill]] 

# Contracts
## Rule usage inside Value Object
[[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/value-object-pattern.skill|Value Object]] call rules in their constructor to enforce invariants.
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

## Rule usage inside Entity
[[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|Entity]] call rules inside setters or behavior methods to protect invariants.
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

## Rule usage inside Domain Service
[[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-service.skill|DomainService]] compose rules for multi-entity or contextual business decisions.
```CSharp
public static class DriverDomainService
{
    public static bool CanDrive(Age age, Country country)
        => (age, country).IsSatisfied();
}
```

# Rules
MUST:
- all reusable validation rules store in rules
- Primitive rules implemented in differen [[skills/dotnet/skill-graph/developing/Module/Domain csproj/module-domain-csproj.skill|module-domain-csproj.skill]] must be extracted into [[shared-layer.skill]]
MUST NOT:
- duplicate same rule in several places
# Anti-patterns
- Rule throw exception
- Business condition duplicated across controller, service, and entity

# Check list
- [ ] No duplication rules in [[skills/dotnet/skill-graph/developing/Module/Domain csproj/module-domain-csproj.skill|module-domain-csproj.skill]]
- [ ] Rules which is common for several [[skills/dotnet/skill-graph/developing/Module/Domain csproj/module-domain-csproj.skill|module-domain-csproj.skill]] extracted into [[shared-layer.skill]]
 
# Relations
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-rule-pattern.skill|domain-rule-pattern.skill]] - reusable validation rule
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/value-object-pattern.skill|value-object-pattern.skill]] - value objects which use rules for validation
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|entity.skill]] - entity which use rules for validation
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/domain-service.skill|domain-service.skill]] - services which use rules for validation
- [[shared-layer.skill]] - shared cs project with common rules