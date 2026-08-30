---
name: plateau-service-with-api--class-property-validator
description: Class {ValueObject}PropertyValidator in the service-with-api plateau
whenToUse: when another module (or this one) needs to validate a Soft{ValueObject} through DI, without referencing this module's Domain
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/class
  - plateau/service-with-api
created_by:
  - "[[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
---

# Goal
- Validate a `Soft{ValueObject}` so other modules can check values they receive, resolvable as `IValidator<Soft{ValueObject}>` through DI

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]

# Core Principles
- Extends `AbstractValidator<Soft{ValueObject}>` — resolvable generically by another module through DI
- The condition is written locally, in this file — a `Must(...)` predicate this solution owns and is free to change on its own
- Works completely on its own — a later, optional `solution-domain-rules` may redirect it to a shared condition, but nothing here assumes that exists

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Property validator | {ValueObject}PropertyValidator | EmailPropertyValidator | {ValueObject}PropertyValidator.cs | EmailPropertyValidator.cs |

# Implementation
```csharp
//Skill: class-property-validator
//Plateau: service-with-api
//Version: 20260825120000

public class EmailPropertyValidator : AbstractValidator<SoftEmail>
{
    public EmailPropertyValidator()
        => RuleFor(x => x).Must(IsValid).WithMessage("Email is not valid.");

    private static bool IsValid(SoftEmail email) => !string.IsNullOrWhiteSpace(email.Value) && email.Value.Contains('@');
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]

# Rules
MUST:
- Extend `AbstractValidator<Soft{ValueObject}>`
- Be named `{ValueObject}PropertyValidator`, live in `/{Module}.Application/Validators/Property`
- Own its own condition — declared and checkable in this file alone
MUST NOT:
- Inject repositories or services
- Throw exceptions

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]

# Check list
- [ ] Extends `AbstractValidator<Soft{ValueObject}>`, lives in `/Validators/Property`
- [ ] The condition is fully readable from this file alone

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]
