---
uid:
name: primitive-rule-class
description: Stateless deterministic business predicate on primitive types implemented as static extension methods.
domain: skill
type: template
version: 20260610
tags:
  - skill/template/class
  - dotnet
  - domain
  - rule
triggers:
  - create primitive rule
  - implement domain rule on primitive
  - encode business predicate
created_by: "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-domain-rule.solution.skill]]"
extended_by:
---

# Goal
- Encode a business predicate on a primitive type as a single source of truth
- Serve as the implementation delegate for all VO overloads of the same rule

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-domain-rule.solution.skill#PrimitiveRule]]

# Core Principles
- Rule defines business meaning — not transport behavior, not framework behavior
- Rule returns `bool` — the caller decides whether to throw
- Rules are stateless, deterministic, and side-effect free
- Primitive rule is the single source of truth — VO overloads delegate here, never duplicate

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-domain-rule.solution.skill#PrimitiveRule]]

# Structure

## Place in csproj
Defined in [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/{Module}.Domain.csproj.skill]]
```
/{ModuleName}.Domain
  /Rules
    {Type}Rules.cs
```

## Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Primitive rule | {Type}Rules | IntRules | {Type}Rules.cs | IntRules.cs |

## Implementation
```csharp
public static class IntRules
{
    public static bool IsPositive(this int value) => value > 0;
    public static bool IsInRange(this int value, int min, int max)
        => value >= min && value <= max;
}

public static class StringRules
{
    public static bool IsNotEmpty(this string value)
        => !string.IsNullOrWhiteSpace(value);
    public static bool IsMaxLength(this string value, int max)
        => value.Length <= max;
}
```

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-domain-rule.solution.skill#PrimitiveRule]]

# Rules

MUST:
- Be a `static class`
- Methods are `static` extension methods on primitive type
- Return `bool` — never throw
- Be stateless and side-effect free
- Named `{Type}Rules` — e.g. `IntRules`, `StringRules`, `DecimalRules`

MUST NOT:
- Depend on EF Core, FluentValidation, ASP.NET, or HttpContext
- Mutate any object
- Duplicate logic that already exists in another primitive rule

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-domain-rule.solution.skill#PrimitiveRule]]

# Anti-patterns
- Rule throws `DomainException` itself — rule returns `bool`, the VO or entity caller throws
- Same business condition checked in controller, validator, entity, and service separately — define once as rule
- Rule depends on DbContext or HttpContext — pure domain predicates only
- Rule has instance state — all rules must be stateless

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-domain-rule.solution.skill#PrimitiveRule]]

# Check list
- [ ] Rule is a static class with static extension methods
- [ ] Rule returns `bool` — never throws
- [ ] Rule is stateless and deterministic
- [ ] Named correctly: `{Type}Rules`
- [ ] Lives in /{Module}.Domain/Rules
- [ ] No infrastructure dependencies

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-domain-rule.solution.skill#PrimitiveRule]]

# Unittest TestCases
- [ ] When value satisfies rule Then returns true
- [ ] When value violates rule Then returns false
- [ ] When boundary value at minimum Then returns expected result
- [ ] When boundary value at maximum Then returns expected result
- [ ] Rule is pure — same input always produces same output
- [ ] Rule has no side effects — calling it twice produces no observable difference

__Applied solutions:__
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-domain-rule.solution.skill#PrimitiveRule]]
