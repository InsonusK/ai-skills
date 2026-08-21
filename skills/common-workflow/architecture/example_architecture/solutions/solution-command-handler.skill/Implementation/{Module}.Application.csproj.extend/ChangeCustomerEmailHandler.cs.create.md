---
description: Handler that loads Customer and calls its guarded ChangeEmail method
project_name: "{Module}.Application"
name: ChangeCustomerEmailHandler
element_kind: class
change_kind: create
tags:
  - solution/command-handler
  - element/changecustomeremailhandler-cs
---

# Implementation changes
```csharp
// {Module}.Application/Commands/ChangeCustomerEmailHandler.cs
public class ChangeCustomerEmailHandler
{
    public void Handle(ChangeCustomerEmailCommand command)
    {
        var customer = Load(command.CustomerId);
        var email = new Email(command.NewEmail);
        customer.ChangeEmail(email);
    }
}
```

# Rule changes

## MUST
- Construct `Email` before calling `ChangeEmail` — never pass `command.NewEmail` as a raw string into the domain layer.
  - Risk: skipping `Email`'s constructor lets an unvalidated string reach `Customer`.
  - Fix: always build the value object first, as shown above.
