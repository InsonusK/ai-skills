---
description: Self-validating email value object
project_name: "{Module}.Domain"
name: Email
element_kind: class
change_kind: create
tags:
  - solution/value-object
  - element/email-cs
---

# Implementation changes
```csharp
// {Module}.Domain/ValueObjects/Email.cs
public sealed record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (!IsValidFormat(value))
            throw new DomainException("{ModuleName}.Email.InvalidFormat", "Email must contain '@'.");

        Value = value;
    }

    private static bool IsValidFormat(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Contains('@');
}
```

# Rule changes

## MUST
- Validate `value` before assigning `Value`.
  - Risk: assigning first and validating after leaves a window where an invalid `Email` briefly exists.
  - Fix: run `IsValidFormat` before the assignment, as shown above.
