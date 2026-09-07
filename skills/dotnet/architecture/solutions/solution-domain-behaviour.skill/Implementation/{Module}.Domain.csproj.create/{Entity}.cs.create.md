---
description: Create the module's first entity — Id plus state transitions guarded by a locally-owned condition
project_name: "{Module}.Domain"
name: "{Entity}.cs"
element_kind: class
change_kind: create
tags:
  - solution/domain-behaviour
  - element/entity-cs
---

# Goals
- Create the first entity in `{Module}.Domain/Entities`: an `Id`, and state transitions expressed as named methods that validate before mutating.
- Enforce entity invariants so invalid state is unreachable.
- Extract bulky logic to `{Module}.Domain/Services` while keeping the entity the gatekeeper of its state.

# Core Principles
- The entity has no public setters for guarded state — mutation is through named methods (or `internal set` written only by those methods).
- Every behavior method validates via a condition it owns (inline or a `private static` helper on the same class) before assigning anything, throwing `Shared.Exceptions.DomainException` on failure.
- Property types are `Soft{ValueObject}` or primitives until `solution-value-objects` (VP3) adds the strict `{ValueObject}`.

# Naming convention

| use case | class name | file name |
| -------- | ---------- | --------- |
| Entity | `{Entity}` | `{Entity}.cs` |
| Behavior method | `{Verb}{Noun}` or `{Verb}` (e.g. `UpdateComment`) | (on the entity) |

# Implementation changes

The entity exposes named, validating behavior methods — never a public setter for guarded state:

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
- Never mutate state before validating
- Never allow invalid state to persist silently
- Never let a service extension expose a second public way to change a property that is already changed by an entity method
- Never let a service extension bypass entity methods and write directly to properties

## SHOULD
- Keep entity methods small and delegate complex calculations to service extensions

# Unittest TestCases
- [ ] WHEN applied THEN Enforce entity invariants and prevent invalid state on every state-changing method
- [ ] WHEN applied THEN Extract bulky logic to {Module}.Domain/Services while keeping the entity as the gatekeeper of state
- [ ] WHEN applied THEN Entity methods validate via a locally-owned condition before applying changes
- [ ] WHEN applied THEN DomainException is thrown when the condition fails
- [ ] WHEN naming 'Entity behavior method' THEN pattern matches convention
