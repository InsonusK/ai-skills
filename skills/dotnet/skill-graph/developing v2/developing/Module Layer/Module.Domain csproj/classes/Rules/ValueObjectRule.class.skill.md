---
uid:
name: value-object-rule-class
description: Business predicate extension methods on Value Object types that delegate to primitive rules.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/class
  - dotnet
  - domain
  - rule
  - value-object
triggers:
  - create value object rule
  - implement VO domain rule
  - encode VO predicate
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill]]"
extended_by:
---

# Goal
- Extend a Value Object type with named business predicates
- Delegate to the corresponding primitive rule — never duplicate logic

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#ValueObjectRule]]

# Core Principles
- Static class with extension methods on the VO type
- Always delegates to primitive overload — never reimplements logic
- Named `{VOType}Rules`

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#ValueObjectRule]]

# Structure

## Place in csproj
Defined in [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/{Module}.Domain.csproj.skill]]
```
/{ModuleName}.Domain
  /Rules
    {VOType}Rules.cs
```

## Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| VO rule | {VOType}Rules | AgeRules | {VOType}Rules.cs | AgeRules.cs |

## Implementation
```csharp
public static class AgeRules
{
    public static bool IsPositive(this Age age)
        => age.Value.IsPositive(); // delegates to IntRules

    public static bool IsAdult(this Age age)
        => age.Value >= 18;

    public static bool IsOld(this Age age)
        => age.Value > 90;
}
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#ValueObjectRule]]

# Rules

MUST:
- Be a `static class`
- Methods delegate to primitive rule where a primitive equivalent exists
- Return `bool` — never throw
- Named `{VOType}Rules`

MUST NOT:
- Reimplement logic that already exists in a primitive rule
- Throw exceptions — return bool only

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#ValueObjectRule]]

# Anti-patterns
- VO rule reimplements primitive rule logic — always delegate to primitive overload
- Rule throws exceptions internally

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#ValueObjectRule]]

# Check list
- [ ] Rule is a static class with static extension methods
- [ ] Rule returns `bool` — never throws
- [ ] VO rule delegates to primitive rule — no logic duplication
- [ ] Named correctly: `{VOType}Rules`
- [ ] Lives in /{Module}.Domain/Rules
- [ ] No infrastructure dependencies

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#ValueObjectRule]]

# Unittest TestCases
- [ ] When VO overload called Then delegates to primitive overload — same result
- [ ] Rule is pure — same input always produces same output
- [ ] Rule has no side effects

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/05-domain-rule.solution.skill#ValueObjectRule]]
