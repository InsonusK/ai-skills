---
description: Replace solution-domain-behaviour's local condition inside a behavior method with a call to the centralized Check()
project_name: "{Module}.Domain"
name: "{EntityName}"
element_kind: class
change_kind: extend
tags:
  - solution/domain-rules
  - element/entityname-cs
---

# Goals
- Remove a duplicate copy of a condition already centralized in `{Module}.Domain.Rules` because a Value Object or validator needs the same one

# Implementation changes

Before (per `solution-domain-behaviour`, composed locally with a `private static` helper):

```csharp
public class Driver
{
    public int Id { get; internal set; }
    public Age Age { get; internal set; }
    public Country Country { get; internal set; }

    public void AssignLicense()
    {
        if (!MeetsLicensingRequirements(Age, Country))
            throw new DomainException("TaskModule.Driver.NotEligibleForLicense", "Driver does not meet licensing requirements for this country.");
    }

    private static bool MeetsLicensingRequirements(Age age, Country country) => country.Code switch
    {
        "US" => age.Value >= 16,
        "NL" => age.Value >= 18,
        _ => false
    };
}
```

After (redirected to a centralized rule — the same condition is now reusable by, for example, an eligibility check elsewhere in the module):

```csharp
public class Driver
{
    public int Id { get; internal set; }
    public Age Age { get; internal set; }
    public Country Country { get; internal set; }

    public void AssignLicense()
    {
        var result = (Age, Country).Check();
        var blocking = result.Errors.FirstOrDefault(e => e.Severity == Severity.Error);
        if (blocking is not null)
            throw new DomainException(blocking.ErrorCode, blocking.ErrorMessage);
    }
}
```

The local `private static` helper is deleted — the condition now lives once, in `{Module}.Domain.Rules`.

# Rule changes

## MUST
- Call the centralized `Check()` instead of a local condition, once redirected
- Delete the local `private static` helper this file used to define

## MUST NOT
- Keep the local helper alongside the centralized rule
