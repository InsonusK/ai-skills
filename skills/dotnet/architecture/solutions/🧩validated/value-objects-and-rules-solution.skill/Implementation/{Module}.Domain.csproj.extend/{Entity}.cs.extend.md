---
description: Extend entity to use Value Objects for properties with invariants and domain rules inside behavior methods
project_name: "{Module}.Domain"
name: "{Entity}"
element_kind: class
change_kind: extend
---

# Goals
- Encapsulate invariant state on Entity properties into dedicated Value Object types
- Keep Entity focused on identity, lifecycle, and aggregate consistency while delegating value-level validation to Value Objects
- Enforce entity invariants and prevent invalid state by using domain rules inside entity behavior methods
- Keep entity validation logic DRY by delegating to reusable domain rules instead of inline conditions

# Core Principals
- Entity properties that carry business meaning or invariant constraints use Value Objects instead of primitives
- Value Object immutability guarantees that once an Entity holds a value, that value cannot be mutated into an invalid state
- Equality of value properties on Entities is evaluated by Value Object structural equality
- Entity defines consistency — it decides when and how to enforce invariants
- Entity methods call domain rules to validate state transitions before applying changes
- Rule returns `bool` — entity decides whether to throw `DomainException` or reject the change
- Multiple related conditions are composed from individual rules — not reimplemented inline

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity class | {EntityName} | Order | {EntityName}.cs | Order.cs |

# Implementation changes

Entity must use Value Object types for properties that have invariant state or business semantics:

```csharp
public class Order
{
    public int Id { get; internal set; }
    public Money Total { get; internal set; }
    public Email CustomerEmail { get; internal set; }
}
```

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

Multi-property Value Objects persisted via EF Core `OwnsOne` must be configured in the entity's EF configuration (see [[skills/dotnet/architecture/solutions/🧩validated/domain-configuration-solution.skill/domain-configuration-solution.skill.md|domain-configuration-solution.skill]]):

```csharp
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.OwnsOne(o => o.Total);
        builder.OwnsOne(o => o.CustomerEmail);
    }
}
```

# Rule changes

MUST:
- Use Value Object on Entity property when the value has invariant state or carries business semantics
- Configure multi-property Value Objects with `OwnsOne` in the entity's EF configuration
- Call domain rules inside entity methods before mutating state
- Throw `DomainException` when a rule returns `false` — the entity enforces, the rule only predicates
- Use the most specific rule available (primitive, VO, or contextual) for the condition being checked

MUST NOT:
- Use primitive type on Entity property when the value carries business meaning or invariant constraints
- Reimplement rule logic inline inside entity methods — always delegate to existing rules
- Mutate state before validating with rules
- Allow invalid state to persist silently

# Unittest TestCases
- [ ] WHEN applied THEN Encapsulate invariant state on Entity properties into dedicated Value Object types
- [ ] WHEN applied THEN Keep Entity focused on identity, lifecycle, and aggregate consistency while delegating value-level validation to Value Objects
- [ ] WHEN applied THEN Enforce entity invariants and prevent invalid state by using domain rules inside entity behavior methods
- [ ] WHEN applied THEN Keep entity validation logic DRY by delegating to reusable domain rules instead of inline conditions
- [ ] WHEN applied THEN Entity properties that carry business meaning or invariant constraints use Value Objects instead of primitives
- [ ] WHEN applied THEN Value Object immutability guarantees that once an Entity holds a value, that value cannot be mutated into an invalid state
- [ ] WHEN applied THEN Equality of value properties on Entities is evaluated by Value Object structural equality
- [ ] WHEN applied THEN Entity defines consistency — it decides when and how to enforce invariants
- [ ] WHEN applied THEN Entity methods call domain rules to validate state transitions before applying changes
- [ ] WHEN applied THEN Rule returns bool — entity decides whether to throw DomainException or reject the change
- [ ] WHEN applied THEN Multiple related conditions are composed from individual rules — not reimplemented inline
- [ ] WHEN naming 'Entity class' THEN pattern matches convention
