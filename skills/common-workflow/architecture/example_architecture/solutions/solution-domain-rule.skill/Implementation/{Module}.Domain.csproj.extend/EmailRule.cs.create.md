---
description: Centralized email-format condition, callable from both Email and the transport validator
project_name: "{Module}.Domain"
name: EmailRule
element_kind: class
change_kind: create
tags:
  - solution/domain-rule
  - element/emailrule-cs
---

# Implementation changes
```csharp
// {Module}.Domain/Rules/EmailRule.cs
public static class EmailRule
{
    public static bool IsValid(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Contains('@');

    public static void Check(string value)
    {
        if (!IsValid(value))
            throw new DomainException("{ModuleName}.Email.InvalidFormat", "Email must contain '@'.");
    }
}
```

# Rule changes

## MUST
- Keep `IsValid` free of any throw — only `Check` throws.
  - Risk: `IsValid` is meant to be called from FluentValidation's `.Must(...)`, which expects a `bool`; a throwing `IsValid` would surface as an unhandled exception during transport validation instead of a validation error.
  - Fix: `IsValid` returns `bool` only; `Check` wraps it with the throw for the `Email` constructor's use.
