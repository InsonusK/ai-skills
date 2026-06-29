---
name: class-property-validator
description: FluentValidation property validator for a Soft{ValueObject} declared in {Module}.Interfaces
domain: skill
type: template
version: 20260629210700
plateau: default
tags:
  - skill/template/class
  - plateau/default
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators.skill]]"
---

# Goal
- Validate a `Soft{ValueObject}` so other modules can check values they receive
- Reuse the same validation contract that the Domain Value Object enforces

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]

# Core Principles
- Apply ONE plateau template per class
- Extends `AbstractValidator<Soft{ValueObject}>`
- Stateless and declarative
- Registered by FluentValidation's assembly scan of `{Module}.Application`
- Validates values only by calling Rules
- No infrastructure or business-rule dependencies

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Property validator | `{ValueObject}PropertyValidator` | `EmailPropertyValidator` | `{ValueObject}PropertyValidator.cs` | `EmailPropertyValidator.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]

# Implementation

Write a comment at the top of the created class with the applied skill metadata:

```csharp
//Skill: class-property-validator
//Plateau: default
//Version: 20260629210700
```

Single-property soft value object validator:

```csharp
// {Module}.Application/Validators/EmailPropertyValidator.cs
using FluentValidation;
using {Module}.Domain.Rules;
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Application.Validators.Property;

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

namespace {Module}.Application.Validators.Property;

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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]

# Rules
MUST:
	- Extend `AbstractValidator<Soft{ValueObject}>`
	- Be named `{ValueObject}PropertyValidator`
	- Live in `/{Module}.Application/Validators/Property`
	- Validate transport/value correctness only by calling Rules
MUST NOT:
	- Inject repositories or services
	- Contain business rules
	- Contain inline FluentValidation predicates that duplicate Rule logic
	- Throw exceptions

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]

# Anti-patterns
- Apply SEVERAL plateau template per class
- Validating `Soft{ValueObject}` inline instead of calling a Rule
- Re-implementing validation logic already in the Domain Value Object instead of sharing a Rule
- Referencing Domain types inside the property validator

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Validate a Soft{ValueObject} so other modules can check values they receive
- [ ] WHEN applied THEN Reuse the same validation contract that the Domain Value Object enforces
- [ ] WHEN applied THEN Extends AbstractValidator<Soft{ValueObject}>
- [ ] WHEN applied THEN Stateless and declarative
- [ ] WHEN applied THEN No infrastructure or business-rule dependencies
- [ ] WHEN naming 'Property validator' THEN pattern matches convention
- [ ] When a valid Soft{ValueObject} is validated THEN no errors are returned
- [ ] When an invalid Soft{ValueObject} is validated THEN validation errors are returned
- [ ] When resolved from DI as IValidator<Soft{ValueObject}> THEN the property validator is returned
- [ ] WHEN applied THEN validates by calling a Rule

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill.md|solution-soft-value-objects-and-dto-validators]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]
