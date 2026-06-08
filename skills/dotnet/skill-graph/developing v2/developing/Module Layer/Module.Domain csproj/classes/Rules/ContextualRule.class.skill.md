---
uid:
name: contextual-rule-class
description: Multi-value business predicate implemented as tuple extension methods with primitive and VO overloads.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/class
  - dotnet
  - domain
  - rule
  - contextual
triggers:
  - create contextual rule
  - implement multi-value domain rule
  - encode business condition
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill]]"
extended_by:
---

# Goal
- Encode a business predicate that depends on multiple values simultaneously
- Provide both primitive and VO overloads — VO overload delegates to primitive overload

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#ContextualRule]]

# Core Principles
- Implemented as tuple extension methods — `(Type1, Type2).IsSatisfied()`
- Primitive tuple overload holds the logic
- VO tuple overload delegates to primitive tuple overload
- Named `{Condition}Rule`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#ContextualRule]]

# Structure

## Place in csproj
Defined in [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/{Module}.Domain.csproj.skill]]
```
/{ModuleName}.Domain
  /Rules
    {Condition}Rule.cs
```

## Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Contextual rule | {Condition}Rule | CanDriveCarRule | {Condition}Rule.cs | CanDriveCarRule.cs |

## Implementation
```csharp
public static class CanDriveCarRule
{
    // primitive overload — single source of truth
    public static bool IsSatisfied(this (int Age, string Country) value)
    {
        return value.Country switch
        {
            "US" => value.Age >= 16,
            "NL" => value.Age >= 18,
            _ => false
        };
    }

    // VO overload — delegates to primitive overload
    public static bool IsSatisfied(this (Age Age, Country Country) value)
        => (value.Age.Value, value.Country.Code).IsSatisfied();
}
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#ContextualRule]]

# Rules

MUST:
- Primitive tuple overload holds the logic — single source of truth
- VO tuple overload delegates to primitive overload
- Return `bool` — never throw
- Named `{Condition}Rule`

MUST NOT:
- VO overload reimplement logic from primitive overload
- Throw exceptions internally

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#ContextualRule]]

# Anti-patterns
- VO overload reimplements primitive overload logic
- Rule throws exceptions internally

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#ContextualRule]]

# Check list
- [ ] Rule is a static class with static extension methods
- [ ] Rule returns `bool` — never throws
- [ ] ContextualRule has primitive tuple overload as source of truth
- [ ] Named correctly: `{Condition}Rule`
- [ ] Lives in /{Module}.Domain/Rules
- [ ] No infrastructure dependencies

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#ContextualRule]]

# Unittest TestCases
- [ ] When value satisfies rule Then returns true
- [ ] When value violates rule Then returns false
- [ ] When contextual VO overload called Then same result as primitive tuple overload
- [ ] Rule is pure — same input always produces same output
- [ ] Rule has no side effects

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#ContextualRule]]
