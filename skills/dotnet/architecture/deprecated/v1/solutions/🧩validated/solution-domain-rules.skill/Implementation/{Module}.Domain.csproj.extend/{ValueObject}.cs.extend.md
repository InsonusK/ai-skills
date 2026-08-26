---
description: Replace solution-value-objects's local validation predicate with a call to the centralized Check()
project_name: "{Module}.Domain"
name: "{ValueObject}"
element_kind: class
change_kind: extend
tags:
  - solution/domain-rules
  - element/valueobject-cs
---

# Goals
- Remove the duplicate copy of a condition once `{ValueObject}PropertyValidator` (or another consumer) already needs the same one

# Core Principles
- Only apply this once the same condition genuinely exists in a second consumer — see this solution's own Boundaries and Check list
- The `DomainException` thrown, its message, and its behavior on invalid input do not change — only where the condition is declared changes

# Implementation changes

Before (per `solution-value-objects`, local predicate):

```csharp
public sealed record Complexity : SoftComplexity
{
    public Complexity(int value) : base(value)
    {
        if (!IsValid(value))
            throw new DomainException("TaskModule.Complexity.NonNegative", $"Complexity must be non-negative, but was {value}.");
    }

    private static bool IsValid(int value) => value >= 0;
}
```

After (redirected to the centralized `Check()` from `{Module}.Domain.Rules`):

```csharp
namespace TaskUnderControl.Srv.TaskModule.Domain.ValueObjects;

using TaskUnderControl.Srv.TaskModule.Domain.Rules;   // Check() extension — now defined here
using TaskUnderControl.Srv.TaskModule.Interfaces.ValueObjects;

public sealed record Complexity : SoftComplexity
{
    public Complexity(int value) : base(value)
    {
        var result = this.Check();

        // Errors.Any(Severity == Error), not !result.IsValid — ValidationResult.IsValid ignores
        // Severity, so a mixed Error/Warning result would incorrectly block on a Warning.
        var blocking = result.Errors.FirstOrDefault(e => e.Severity == Severity.Error);
        if (blocking is not null)
            throw new DomainException(blocking.ErrorCode, blocking.ErrorMessage);
    }
}
```

The local `private static IsValid` method and its own inline `DomainException` construction are deleted — `{Module}.Domain.Rules` is now the only place the condition exists.

# Rule changes

## MUST
- Call `this.Check()` instead of a local predicate, once redirected
- Delete the local `private static` predicate this file used to define — do not leave both

## MUST NOT
- Keep the local predicate "just in case" alongside the centralized one — that recreates the duplication this solution exists to remove
