---
uid: bb207021-4cea-4004-a37b-12d042489744
order: 5
name: domain-rule
description: Defines the Domain Rule pattern — stateless deterministic predicates that encode reusable business conditions as static extension methods
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - ddd
  - rules
triggers:
  - create domain rule
  - encode business predicate
  - extract business condition
  - reusable domain validation
creates:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Rules/PrimitiveRule.class.skill|PrimitiveRule.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Rules/ValueObjectRule.class.skill|ValueObjectRule.class.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Rules/ContextualRule.class.skill|ContextualRule.class.skill]]"
extends:
  - "[[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/{Module}.Domain.csproj.skill|{Module}.Domain.csproj.skill]]"
depends_on:
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/02-solution-layer-structure.solution.skill|02-solution-layer-structure.solution.skill]]"
  - "[[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-value-object.solution.skill|04-value-object.solution.skill]]"
---
# Goal
- Define a single reusable pattern for encoding business predicates that can be used across Value Objects, Entities, and Domain Services
- Prevent business condition duplication across controllers, validators, services, and entities
- Separate the predicate (does this satisfy the condition?) from the enforcement (throw if not)
- Ensure primitive and VO overloads of the same rule share one implementation — no duplication

# Core Principles
- Rule defines business meaning — not transport behavior, not framework behavior
- Rule returns `bool` — the caller decides whether to throw, not the rule
- Rules are stateless, deterministic, and side-effect free
- Rules are implemented as static extension methods — never instantiated
- Primitive rule is the single source of truth — VO overloads delegate to primitive overload
- Three rule shapes exist: primitive rules, VO-scoped rules, contextual (multi-value) rules
- Rules define predicates — Entities define consistency — Value Objects define correctness

# Depend on solutions
- [[02-solution-layer-structure.solution.skill]] — rules live in {Module}.Domain, placement defined by layer structure
- [[skills/dotnet/skill-graph/developing v2/architecture/solutions/04-value-object.solution.skill|03-value-object.solution.skill]] — VO-scoped rules extend Value Object types defined by this solution

# Implementation

## {Module}.Domain (.csproj) (extended)

### Project extension

#### Goal
- Store all domain rule types for this bounded context

#### Core Principles
- Rules define predicates — Entities define consistency — Value Objects define correctness

#### Structure

##### Project Structure
```
/{Module}.Domain
  /Rules
    IntRules.cs
    StringRules.cs
    AgeRules.cs
    CanDriveCarRule.cs
```

##### Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Rules | All domain rule static classes for this module | |

#### Rules
MUST:
- All domain rules live in /{Module}.Domain/Rules

---

### Class extension

#### PrimitiveRule

##### Goal
- Encode a business predicate on a primitive type as a single source of truth
- Serve as the implementation delegate for all VO overloads of the same rule

##### Core Principal
- Static class with static extension methods on primitive types (`int`, `string`, `decimal`)
- Single implementation — all VO overloads delegate here, never duplicate
- Returns `bool` — never throws
- Defines business meaning — not transport behavior, not framework behavior
- Are Stateless, deterministic, and side-effect free

##### Implementation changes
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

##### Rule changes
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

---

#### ValueObjectRule

##### Goal
- Extend a Value Object type with named business predicates
- Delegate to the corresponding primitive rule — never duplicate logic

##### Core Principal
- Static class with extension methods on the VO type
- Always delegates to primitive overload — never reimplements logic
- Named `{VOType}Rules` — e.g. `AgeRules`, `MoneyRules`
- Defines business meaning — not transport behavior, not framework behavior
- Are Stateless, deterministic, and side-effect free
- Returns `bool` — never throws
##### Implementation changes
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

##### Rule changes
MUST:
- Be a `static class`
- Methods delegate to primitive rule where a primitive equivalent exists
- Return `bool` — never throw
- Named `{VOType}Rules`

MUST NOT:
- Reimplement logic that already exists in a primitive rule
- Throw exceptions — return bool only

---

#### ContextualRule

##### Goal
- Encode a business predicate that depends on multiple values simultaneously
- Provide both primitive and VO overloads — VO overload delegates to primitive overload

##### Core Principal
- Implemented as tuple extension methods — `(Type1, Type2).IsSatisfied()`
- Primitive tuple overload holds the logic
- VO tuple overload delegates to primitive tuple overload
- Named `{Condition}Rule` — e.g. `CanDriveCarRule`, `HasPermissionRule`
- Defines business meaning — not transport behavior, not framework behavior
- Are Stateless, deterministic, and side-effect free
- Returns `bool` — never throws

##### Implementation changes
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

##### Rule changes
MUST:
- Primitive tuple overload holds the logic — single source of truth
- VO tuple overload delegates to primitive overload
- Return `bool` — never throw
- Named `{Condition}Rule`

MUST NOT:
- VO overload reimplement logic from primitive overload
- Throw exceptions internally

---

#### [[skills/dotnet/skill-graph/developing v2/developing/Module Layer/Module.Domain csproj/classes/Entity.class.skill|Entity.class.skill]] (extended)

##### Goal
- Enforce entity invariants and prevent invalid state by using domain rules inside entity behavior methods
- Keep entity validation logic DRY by delegating to reusable domain rules instead of inline conditions

##### Core Principles
- Entity defines consistency — it decides when and how to enforce invariants
- Entity methods call domain rules to validate state transitions before applying changes
- Rule returns `bool` — entity decides whether to throw `DomainException` or reject the change
- Multiple related conditions are composed from individual rules — not reimplemented inline

##### Implementation changes
Entity behavior methods must use domain rules to guard state changes:

```csharp
public class Order
{
    public int Id { get; internal set; }
    public string Comment { get; internal set; }
    public uint Version { get; internal set; }

    public void UpdateComment(string comment)
    {
        if (!comment.IsNotEmpty())
            throw new DomainException("Comment must not be empty.");

        if (!comment.IsMaxLength(500))
            throw new DomainException("Comment must not exceed 500 characters.");

        Comment = comment;
    }
}
```

Entity can compose multiple rules for complex invariants:

```csharp
public class Driver
{
    public int Id { get; internal set; }
    public Age Age { get; internal set; }
    public Country Country { get; internal set; }

    public void AssignLicense()
    {
        if (!(Age, Country).IsSatisfied())
            throw new DomainException("Driver does not meet licensing requirements for this country.");

        // ... assign license
    }
}
```

##### Rule changes
MUST:
- Call domain rules inside entity methods before mutating state
- Throw `DomainException` when a rule returns `false` — the entity enforces, the rule only predicates
- Use the most specific rule available (primitive, VO, or contextual) for the condition being checked

MUST NOT:
- Reimplement rule logic inline inside entity methods — always delegate to existing rules
- Mutate state before validating with rules
- Allow invalid state to persist silently

---

# Rules

MUST:
- All rules implemented as static extension methods
- Rules return `bool` — caller decides whether to throw
- Rules are stateless and deterministic
- Primitive rule is single source of truth — VO rules delegate to it
- All rules live in /{Module}.Domain/Rules
- Named `{Type}Rules` for primitive/VO rules, `{Condition}Rule` for contextual rules

SHOULD:
- Rules be synchronous
- Rules avoid allocations

MUST NOT:
- Rule throw exceptions internally
- Rule depend on EF Core, FluentValidation, ASP.NET, HttpContext, or any infrastructure
- Rule mutate any object
- Rule duplicate logic that already exists in another rule
- Rule be instantiated with `new` — always static

# Anti-patterns
- Rule throws `DomainException` itself — rule returns `bool`, the VO or entity caller throws
- `new CanDriveCarRule().IsSatisfied()` — rules are static, never instantiated
- VO rule reimplements primitive rule logic — always delegate to primitive overload
- Same business condition checked in controller, validator, entity, and service separately — define once as rule
- Rule depends on DbContext or HttpContext — pure domain predicates only
- Rule has instance state — all rules must be stateless

# Check list
- [ ] Rule is a static class with static extension methods
- [ ] Rule returns `bool` — never throws
- [ ] Rule is stateless and deterministic
- [ ] Primitive rule exists as source of truth where applicable
- [ ] VO rule delegates to primitive rule — no logic duplication
- [ ] ContextualRule has primitive tuple overload as source of truth
- [ ] Named correctly: `{Type}Rules` or `{Condition}Rule`
- [ ] Lives in /{Module}.Domain/Rules
- [ ] No infrastructure dependencies

# Unittest TestCases
- [ ] When value satisfies rule Then returns true
- [ ] When value violates rule Then returns false
- [ ] When boundary value at minimum Then returns expected result
- [ ] When boundary value at maximum Then returns expected result
- [ ] When VO overload called Then delegates to primitive overload — same result
- [ ] When contextual VO overload called Then same result as primitive tuple overload
- [ ] Rule is pure — same input always produces same output
- [ ] Rule has no side effects — calling it twice produces no observable difference
