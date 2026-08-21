---
description: Customer entity with a guarded ChangeEmail method
project_name: "{Module}.Domain"
name: Customer
element_kind: class
change_kind: create
tags:
  - solution/entity-invariant
  - element/customer-cs
---

# Implementation changes
```csharp
// {Module}.Domain/Entities/Customer.cs
public class Customer
{
    public int Id { get; internal set; }
    public Email Email { get; private set; }

    public void ChangeEmail(Email newEmail)
    {
        if (newEmail == Email)
            throw new DomainException("{ModuleName}.Customer.EmailUnchanged", "New email must differ from the current one.");

        Email = newEmail;
    }
}
```

# Rule changes

## MUST
- Accept `Email`, not `string`, as `ChangeEmail`'s parameter.
  - Risk: accepting a raw `string` reopens the door to an invalid email reaching `Customer`, bypassing `Email`'s own validation.
  - Fix: the caller constructs `Email` first; `ChangeEmail` only ever receives an already-valid value.
