---
description: FluentValidation validator for a Soft{ValueObject}, using its own local condition — validation-agnostic today, may later be redirected to a centralized condition by solution-domain-rules
project_name: "{Module}.Application"
name: "{ValueObject}PropertyValidator.cs"
element_kind: class
change_kind: create
tags:
  - solution/dto-property-validators
  - element/valueobject-propertyvalidator-cs
---

# Goals
- Validate a `Soft{ValueObject}` so other modules can check values they receive, resolvable as `IValidator<Soft{ValueObject}>` through DI

# Core Principles
- Extends `AbstractValidator<Soft{ValueObject}>` — not `PropertyValidator<T,TProperty>`, which cannot be resolved generically by another module (see the `use-abstract-validator-for-soft-value-objects` ADR)
- The condition is written locally, in this file — a `Must(...)` predicate this solution owns and is free to change on its own. It is not required to match the Domain `{ValueObject}` constructor's own predicate word-for-word, though the two typically agree
- This solution works completely on its own; a later, optional `solution-domain-rules` solution may redirect this validator to a shared, centralized condition, but nothing here assumes that exists

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Property validator | `{ValueObject}PropertyValidator` | `EmailPropertyValidator` | `{ValueObject}PropertyValidator.cs` | `EmailPropertyValidator.cs` |

# Implementation changes

```csharp
// {Module}.Application/Validators/Property/EmailPropertyValidator.cs
using FluentValidation;
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Application.Validators.Property;

public class EmailPropertyValidator : AbstractValidator<SoftEmail>
{
    public EmailPropertyValidator()
        => RuleFor(x => x).Must(IsValid).WithMessage("Email is not valid.");

    private static bool IsValid(SoftEmail email) => !string.IsNullOrWhiteSpace(email.Value) && email.Value.Contains('@');
}
```

Worked example (`TaskModule`, `SoftComplexity`):

```csharp
namespace TaskUnderControl.Srv.TaskModule.Application.Validators.Property;

using FluentValidation;
using TaskUnderControl.Srv.TaskModule.Interfaces.ValueObjects;

public class ComplexityPropertyValidator : AbstractValidator<SoftComplexity>
{
    public ComplexityPropertyValidator()
        => RuleFor(x => x.Value).GreaterThanOrEqualTo(0).WithMessage("Complexity must be non-negative.");
}
```

Usage from another module's validator, through DI:

```csharp
public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator(IValidator<SoftEmail> emailValidator)
    {
        RuleFor(x => x.Email).SetValidator(emailValidator);
    }
}
```

# Rule changes

## MUST
- Extend `AbstractValidator<Soft{ValueObject}>`
- Be named `{ValueObject}PropertyValidator`
- Live in `/{Module}.Application/Validators/Property`
- Own its own condition — declared and checkable in this file alone
- Never inject repositories or services
- Never throw exceptions

# Check list
- [ ] Extends `AbstractValidator<Soft{ValueObject}>`
- [ ] Named `{ValueObject}PropertyValidator`, lives in `/Validators/Property`
- [ ] The condition is fully readable from this file, with no external rules-project reference required

# Unittest TestCases
- [ ] When a valid `Soft{ValueObject}` is validated Then no errors are returned
- [ ] When an invalid `Soft{ValueObject}` is validated Then a validation error is returned
- [ ] When resolved from DI as `IValidator<Soft{ValueObject}>` Then the property validator is returned
