---
description: FluentValidation property validator for a Soft{ValueObject}
project_name: "{Module}.Application"
name: "{ValueObject}PropertyValidator.cs"
element_kind: class
change_kind: create
---

# Goals
- Validate a `Soft{ValueObject}` so other modules can check values they receive
- Reuse the same validation contract that the Domain Value Object enforces

# Core Principles
- Extends `AbstractValidator<Soft{ValueObject}>`
- Stateless and declarative
- Registered by FluentValidation's assembly scan of `{Module}.Application`
- Validates values only by calling Rules
- No infrastructure or business-rule dependencies

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Property validator | `{ValueObject}PropertyValidator` | `EmailPropertyValidator` | `{ValueObject}PropertyValidator.cs` | `EmailPropertyValidator.cs` |

# Implementation changes

```csharp
// {Module}.Application/Validators/EmailPropertyValidator.cs
using FluentValidation;
using {Module}.Domain.Rules;
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Application.Validators;

public class EmailPropertyValidator : AbstractValidator<SoftEmail>
{
    public EmailPropertyValidator()
    {
        RuleFor(x => x).Must(x => x.IsValidEmail());
    }
}
```

Multi-property example:

```csharp
// {Module}.Application/Validators/MoneyPropertyValidator.cs
using FluentValidation;
using {Module}.Domain.Rules;
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Application.Validators;

public class MoneyPropertyValidator : AbstractValidator<SoftMoney>
{
    public MoneyPropertyValidator()
    {
        RuleFor(x => x).Must(x => x.IsValidMoney());
    }
}
```

Usage from another module's validator through DI:

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
MUST:
- Extend `AbstractValidator<Soft{ValueObject}>`
- Be named `{ValueObject}PropertyValidator`
- Live in `/{Module}.Application/Validators`
- Validate transport/value correctness only by calling Rules

MUST NOT:
- Inject repositories or services
- Contain business rules
- Contain inline FluentValidation predicates that duplicate Rule logic
- Throw exceptions

# Anti-patterns
- Validating `Soft{ValueObject}` inline instead of calling a Rule
- Re-implementing validation logic already in the Domain Value Object instead of sharing a Rule
- Referencing Domain types inside the property validator

# Check list
- [ ] Extends `AbstractValidator<Soft{ValueObject}>`
- [ ] Named `{ValueObject}PropertyValidator`
- [ ] Located in `/Validators`
- [ ] Stateless

# Unittest TestCases
- [ ] When a valid `Soft{ValueObject}` is validated Then no errors are returned
- [ ] When an invalid `Soft{ValueObject}` is validated Then validation errors are returned
- [ ] When resolved from DI as `IValidator<Soft{ValueObject}>` Then the property validator is returned
- [ ] When validated Then the property validator calls a Rule
