---
description: Extend entity with behavior methods that validate via a locally-owned condition before mutating state
project_name: "{Module}.Domain"
name: "{EntityName}"
element_kind: class
change_kind: extend
tags:
  - solution/domain-behaviour
  - element/entityname-cs
---

# Goals
- Enforce entity invariants and prevent invalid state on every state-changing method
- Extract bulky logic to `{Module}.Domain/Services` while keeping the entity as the gatekeeper of state

# Core Principles
- Every behavior method validates via a condition it owns — either inline or a `private static` helper on the same class — before assigning anything
- Bulky or multi-step behavior can be delegated to a static service extension, but the entity still owns validation for its own guarded setters

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity behavior method | {Verb}{Noun} or {Verb} | UpdateComment | {EntityName}.cs | Order.cs |

# Implementation changes

Entity behavior methods validate locally before mutating:

```csharp
public class Order
{
    public int Id { get; internal set; }
    public string Comment { get; internal set; }
    public uint Version { get; internal set; }

    public void UpdateComment(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            throw new DomainException("{ModuleName}.Order.CommentRequired", "Comment must not be empty.");

        if (comment.Length > 500)
            throw new DomainException("{ModuleName}.Order.CommentTooLong", "Comment must not exceed 500 characters.");

        Comment = comment;
    }
}
```

Entity composes several conditions for a complex invariant, using a `private static` helper for readability:

```csharp
public class Driver
{
    public int Id { get; internal set; }
    public Age Age { get; internal set; }
    public Country Country { get; internal set; }

    public void AssignLicense()
    {
        if (!MeetsLicensingRequirements(Age, Country))
            throw new DomainException("{ModuleName}.Driver.NotEligibleForLicense", "Driver does not meet licensing requirements for this country.");

        // ... assign license
    }

    private static bool MeetsLicensingRequirements(Age age, Country country) => country.Code switch
    {
        "US" => age.Value >= 16,
        "NL" => age.Value >= 18,
        _ => false
    };
}
```

When behavior becomes too large for the entity, delegate to a Domain Service Extension defined in `{Behavior}Service.cs` (this solution). The entity exposes a guarded internal method for the service to call:

```csharp
public class Order
{
    public int Id { get; internal set; }
    public decimal Total { get; private set; }

    internal void SetTotal(decimal total)
    {
        if (total < 0)
            throw new DomainException("{ModuleName}.Order.TotalMustBePositive", "Total must be positive.");

        Total = total;
    }
}
```

# Rule changes

## MUST
- Validate via a locally-owned condition (inline or a `private static` helper) inside entity methods before mutating state
- Throw `DomainException` when that condition fails
- A single entity property must not have multiple uncoordinated public mutation points

## MUST NOT
- Mutate state before validating
- Allow invalid state to persist silently
- Let a service extension expose a second public way to change a property that is already changed by an entity method
- Let a service extension bypass entity methods and write directly to properties

## SHOULD
- Keep entity methods small and delegate complex calculations to service extensions

# Unittest TestCases
- [ ] WHEN applied THEN Enforce entity invariants and prevent invalid state on every state-changing method
- [ ] WHEN applied THEN Extract bulky logic to {Module}.Domain/Services while keeping the entity as the gatekeeper of state
- [ ] WHEN applied THEN Entity methods validate via a locally-owned condition before applying changes
- [ ] WHEN applied THEN DomainException is thrown when the condition fails
- [ ] WHEN naming 'Entity behavior method' THEN pattern matches convention
