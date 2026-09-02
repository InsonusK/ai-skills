---
name: plateau-offline-sync-service--class-value-object-property-validator
description: Class {ValueObject}PropertyValidator in the plateau-offline-sync-service plateau — a reusable AbstractValidator<Soft{ValueObject}> owning its own local condition, resolvable cross-module
whenToUse: when creating or editing a property validator for a Soft Value Object, or wiring another module's Soft VO validation into a command validator
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
  - "[[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Validate a `Soft{ValueObject}` so any module can check values it receives, resolvable as `IValidator<Soft{ValueObject}>` through DI.

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- Extends `AbstractValidator<Soft{ValueObject}>` — not `PropertyValidator<T,TProperty>`, which another module cannot resolve generically.
- **VP4:** once this and the strict `{ValueObject}` constructor were found to duplicate a condition, the local `Must(...)` is replaced by a single call to the centralized `{Module}.Domain.Rules` `IRuleBuilder` extension — `RuleFor(x => x).{ValueObject}IsValid()`. `ErrorCode`/`Message`/`State` now come from that one extension; the local chain is deleted.
- No repository, no service, no throwing.
- Lives in `/{Module}.Application/Validators/Property`; registered by the `AddValidatorsFromAssembly` scan.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Property validator | `{ValueObject}PropertyValidator` | `EmailPropertyValidator` | `{ValueObject}PropertyValidator.cs` | `EmailPropertyValidator.cs` |

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-value-object-property-validator
// Plateau: core
// Version: 20260902000000
using FluentValidation;
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Application.Validators.Property;

public sealed class EmailPropertyValidator : AbstractValidator<SoftEmail>
{
    public EmailPropertyValidator()
        => RuleFor(x => x.Value).Must(IsValid).WithMessage("Email is not valid.")
            .WithErrorCode("{Module}.Email.Invalid");

    private static bool IsValid(string value) => !string.IsNullOrWhiteSpace(value) && value.Contains('@');
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs.create]]

# Rules
MUST:
- Extend `AbstractValidator<Soft{ValueObject}>`, named `{ValueObject}PropertyValidator`, in `/Validators/Property`.
- Own its condition — fully readable from this file, no external rules-project reference required.
- Never inject a repository or service; never throw.
- Never apply several plateau templates per class.

# Check list
- [ ] `AbstractValidator<Soft{ValueObject}>`, named `{ValueObject}PropertyValidator`, in `/Validators/Property`.
- [ ] The condition is local and self-contained.
- [ ] No repository/service injection; no throw.

# Unittest TestCases
- [ ] WHEN a valid `Soft{ValueObject}` is validated THEN no errors are returned.
- [ ] WHEN an invalid one is validated THEN a failure with the expected error code is returned.
- [ ] WHEN resolved from DI as `IValidator<Soft{ValueObject}>` THEN this validator is returned.
