---
name: class-property-validator
description: Class {ValueObject}PropertyValidator in the v1 plateau
whenToUse: when another module (or this one) needs to validate a Soft{ValueObject} through DI, without referencing this module's Domain
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
  - "[[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
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
//Plateau: v1
//Version: 20260825140000

public class EmailPropertyValidator : AbstractValidator<SoftEmail>
{
    public EmailPropertyValidator()
        => RuleFor(x => x).Must(IsValid).WithMessage("Email is not valid.");

    private static bool IsValid(SoftEmail email) => !string.IsNullOrWhiteSpace(email.Value) && email.Value.Contains('@');
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]

## Once the same condition is duplicated elsewhere: redirect to the centralized IRuleBuilder extension

Optional, applied only once the same condition is found to duplicate `{ValueObject}`'s own constructor check. Before (local condition, per `solution-dto-property-validators`):

```csharp
public class ComplexityPropertyValidator : AbstractValidator<SoftComplexity>
{
    public ComplexityPropertyValidator()
        => RuleFor(x => x.Value).GreaterThanOrEqualTo(0).WithMessage("Complexity must be non-negative.");
}
```

After (redirected to the centralized `IRuleBuilder` extension from `{Module}.Domain.Rules`):

```csharp
public class ComplexityPropertyValidator : AbstractValidator<SoftComplexity>
{
    public ComplexityPropertyValidator() => RuleFor(x => x).ComplexityIsValid();
}
```

The local `.Must(...)`/`.WithMessage(...)` chain is deleted — `ErrorCode`/`Message`/`State` now come from the same extension `{ValueObject}.cs`'s `Check()` also uses. See [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] and its [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.extend.md|{ValueObject}PropertyValidator.cs.extend]].

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.extend.md|{ValueObject}PropertyValidator.cs.extend]]

# Rules
MUST:
- Extend `AbstractValidator<Soft{ValueObject}>`
- Be named `{ValueObject}PropertyValidator`, live in `/{Module}.Application/Validators/Property`
- Own its own condition — declared and checkable in this file alone
- Call the centralized `{ValueObject}IsValid()` extension instead of a local `Must(...)`, once redirected — delete the local condition, never keep both
MUST NOT:
- Inject repositories or services
- Throw exceptions

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.extend.md|{ValueObject}PropertyValidator.cs.extend]]

# Check list
- [ ] Extends `AbstractValidator<Soft{ValueObject}>`, lives in `/Validators/Property`
- [ ] The condition is fully readable from this file alone

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]
