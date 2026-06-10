---
description: Extend entity with behavior methods that enforce invariants through domain rules and integrate with domain services
project_name: "{Module}.Domain"
name: "{EntityName}"
change_kind: extend
---

# Goals
- Enforce entity invariants and prevent invalid state by using domain rules inside entity behavior methods
- Keep entity validation logic DRY by delegating to reusable domain rules
- Extract bulky logic to `{Module}.Domain/Services` while keeping the entity as the gatekeeper of state

# Core Principals
- Entity defines consistency — it decides when and how to enforce invariants
- Entity methods call domain rules to validate state transitions before applying changes
- Rule returns `bool` — entity decides whether to throw `DomainException`
- Bulky or multi-step behavior can be delegated to a static service extension, but the entity still owns validation

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity behavior method | {Verb}{Noun} or {Verb} | UpdateComment | {EntityName}.cs | Order.cs |

# Implementation changes

Entity behavior methods must validate state through domain rules before mutating:

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

Entity can compose contextual rules for complex invariants:

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

When behavior becomes too large for the entity, delegate to a Domain Service Extension defined in `{Behavior}Service.cs` (this solution).

Entity may expose guarded internal methods for use by domain service extensions:

```csharp
public class Order
{
    public int Id { get; internal set; }
    public decimal Total { get; private set; }

    internal void SetTotal(decimal total)
    {
        if (!total.IsPositive())
            throw new DomainException("Total must be positive.");

        Total = total;
    }
}
```

# Rule changes

MUST:
- Call domain rules inside entity methods before mutating state
- Throw `DomainException` when a rule returns `false`
- Use the most specific rule available (primitive, VO, or contextual)
- Keep the entity as the single gatekeeper for each property mutation

MUST NOT:
- Reimplement rule logic inline inside entity methods
- Mutate state before validating with rules
- Allow invalid state to persist silently
- Let a service extension expose a second public way to change a property that is already changed by an entity method
