---
description: Redirect Email's constructor to call EmailRule.Check instead of its own local predicate
project_name: "{Module}.Domain"
name: Email
element_kind: class
change_kind: extend
tags:
  - solution/domain-rule
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
        EmailRule.Check(value);
        Value = value;
    }
}
```

`IsValidFormat` is removed from `Email` — the condition now lives only in `EmailRule`.

# Rule changes

## MUST
- Remove `Email`'s private `IsValidFormat` predicate when applying this extension.
  - Risk: keeping both means the condition can be edited in one place and not the other.
  - Fix: delete the local predicate; `Email` only calls `EmailRule.Check`.
