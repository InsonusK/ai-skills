---
description: Replace solution-dto-property-validators's local Must(...) condition with a call to the centralized IRuleBuilder extension
project_name: "{Module}.Application"
name: "{ValueObject}PropertyValidator.cs"
element_kind: class
change_kind: extend
tags:
  - solution/domain-rules
  - element/valueobject-propertyvalidator-cs
---

# Goals
- Make the property validator call the same condition `{ValueObject}`'s constructor now calls, once both were found to duplicate each other

# Implementation changes

Before (per `solution-dto-property-validators`, local condition):

```csharp
public class ComplexityPropertyValidator : AbstractValidator<SoftComplexity>
{
    public ComplexityPropertyValidator()
        => RuleFor(x => x.Value).GreaterThanOrEqualTo(0).WithMessage("Complexity must be non-negative.");
}
```

After (redirected to the centralized `IRuleBuilder` extension from `{Module}.Domain.Rules`):

```csharp
namespace TaskUnderControl.Srv.TaskModule.Application.Validators.Property;

using FluentValidation;
using TaskUnderControl.Srv.TaskModule.Domain.Rules;
using TaskUnderControl.Srv.TaskModule.Interfaces.ValueObjects;

public class ComplexityPropertyValidator : AbstractValidator<SoftComplexity>
{
    public ComplexityPropertyValidator() => RuleFor(x => x).ComplexityIsValid();
}
```

The local `.Must(...)`/`.WithMessage(...)` chain is deleted — `ErrorCode`/`Message`/`State` now come from the same extension `{ValueObject}.cs`'s `Check()` also uses.

# Rule changes

## MUST
- Call the centralized `{ValueObject}IsValid()` extension instead of a local `Must(...)`, once redirected
- Delete the local condition this file used to define
- Never keep the local condition alongside the centralized one

