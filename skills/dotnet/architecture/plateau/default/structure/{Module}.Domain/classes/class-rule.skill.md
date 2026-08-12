---
name: class-rule
description: Create a domain rule — stateless deterministic predicate implemented as static extension methods
domain: skill
type: template
version: 20260628
plateau: default
tags:
  - skill/template/class
  - plateau/default
  - stack/dotnet
  - concern/architecture

created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]]"
---

# Goal
- Encode a reusable business predicate as a single source of truth
- Prevent duplication of business conditions across controllers, validators, services, and entities
- Separate the predicate from the enforcement mechanism

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{Rule}.cs.create|{Rule}.cs]]

# Core Principles
- Apply ONE plateau template per class
- Static class with static extension methods — never instantiated
- Returns `bool` — the caller decides whether to throw
- Stateless, deterministic, and side-effect free
- Primitive rule is the single source of truth — VO overloads delegate to it, never duplicate
- Value Object constructors validate values only by calling Rules

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{Rule}.cs.create|{Rule}.cs]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Primitive rule | {Type}Rules | IntRules | {Type}Rules.cs | IntRules.cs |
| VO-scoped rule | {VOType}Rules | AgeRules | {VOType}Rules.cs | AgeRules.cs |
| Contextual rule | {Condition}Rule | CanDriveCarRule | {Condition}Rule.cs | CanDriveCarRule.cs |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{Rule}.cs.create|{Rule}.cs]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-rule
//Plateau: default
//Version: 20260628
```

## PrimitiveRule
PrimitiveRule must be a static class with extension methods on the primitive type:

```csharp
// IntRules.cs
public static class IntRules
{
    public static bool IsPositive(this int value) => value > 0;
    public static bool IsInRange(this int value, int min, int max)
        => value >= min && value <= max;
}

// StringRules.cs
public static class StringRules
{
    public static bool IsNotEmpty(this string value)
        => !string.IsNullOrWhiteSpace(value);
    public static bool IsMaxLength(this string value, int max)
        => value.Length <= max;
}
```

## ValueObjectRule
ValueObjectRule must delegate to the primitive rule:

```csharp
// AgeRules.cs
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

## ContextualRule
ContextualRule must use tuple extension methods with primitive overload as source of truth:

```csharp
// CanDriveCarRule.cs
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
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{Rule}.cs.create|{Rule}.cs]]

# Rules
MUST:
	- Be a `static class`
	- Methods are `static` extension methods
	- Return `bool` — never throw
	- Be stateless and side-effect free
	- Named correctly: `{Type}Rules` for primitive/VO rules, `{Condition}Rule` for contextual rules
	- Primitive tuple overload holds the logic — single source of truth
	- VO tuple overload delegates to primitive overload
MUST NOT:
	- Depend on EF Core, FluentValidation, ASP.NET, or HttpContext
	- Mutate any object
	- Duplicate logic that already exists in another primitive rule
	- Throw exceptions internally
	- Reimplement logic that already exists in a primitive rule

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{Rule}.cs.create|{Rule}.cs]]

# Unittest TestCases
- [ ] WHEN applied THEN Encode a reusable business predicate as a single source of truth
- [ ] WHEN applied THEN Prevent duplication of business conditions across controllers, validators, services, and entities
- [ ] WHEN applied THEN Separate the predicate from the enforcement mechanism
- [ ] WHEN applied THEN Static class with static extension methods — never instantiated
- [ ] WHEN applied THEN Returns bool — the caller decides whether to throw
- [ ] WHEN applied THEN Stateless, deterministic, and side-effect free
- [ ] WHEN applied THEN Primitive rule is the single source of truth — VO overloads delegate to it, never duplicate
- [ ] WHEN naming 'Primitive rule' THEN pattern matches convention
- [ ] WHEN naming 'VO-scoped rule' THEN pattern matches convention
- [ ] WHEN naming 'Contextual rule' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-value-objects-and-rules.skill/Implementation/{Module}.Domain.csproj.extend/{Rule}.cs.create|{Rule}.cs]]
