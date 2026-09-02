---
name: plateau-domain-service--class-feature-validator
description: Class {FeatureName}Validator in the plateau-domain-service plateau — the per-feature transport-correctness validator co-located with its handler
whenToUse: when creating or editing a per-feature command validator in {Module}.Application/Features, or deciding whether a check is transport correctness or a business rule
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]]"
---

# Goal
- Validate the transport correctness of one command's input before it reaches the handler, as a declarative FluentValidation rule set.

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- Extends `AbstractValidator<{Command}>`; all rules in the constructor via `RuleFor(...)`.
- Transport correctness only: `NotEmpty`, `NotNull`, `MaximumLength`, `GreaterThan`, `InclusiveBetween`, format/regex.
- For a `Soft{ValueObject}` or DTO property from another module, inject `IValidator<T>` and use `SetValidator` — never restate that condition.
- No I/O, no repository, no service injection; no business rules (existence / state / uniqueness belong in a handler guard, the domain, or a `{Feature}Check`).
- One validator per command; registered by the `AddValidatorsFromAssembly` scan.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Command validator | `{FeatureName}Validator` | `CreateTaskValidator` | `{FeatureName}.Validator.cs` | `CreateTask.Validator.cs` |

# Implementation
```csharp
// Skill: plateau-domain-service--class-feature-validator
// Plateau: core
// Version: 20260902000000
using FluentValidation;
using {OtherModule}.Interfaces.ValueObjects;

namespace {Module}.Application.Features.CreateTask;

public sealed class CreateTaskValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskValidator(IValidator<SoftEmail> emailValidator)
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AssigneeId).GreaterThan(0);
        RuleFor(x => x.Email).SetValidator(emailValidator);
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] - [[../../../../../solutions/solution-mediator-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs.create]]

# Rules
MUST:
- Extend `AbstractValidator<{Command}>`; define every rule in the constructor.
- Enforce transport correctness only — presence, length, format, numeric range.
- Compose another module's `Soft{ValueObject}`/DTO validation via `SetValidator(IValidator<T>)`.
- Live in `/Features/{FeatureName}/{FeatureName}.Validator.cs`, class `{FeatureName}Validator`, one per command.
- Never inject a repository, `DbContext`, or service; never contain a business rule; never be shared across commands.
- Never apply several plateau templates per class.

# Check list
- [ ] `{FeatureName}Validator : AbstractValidator<{Command}>` in the feature folder.
- [ ] Rules are transport-only and declarative; value-concept properties use `SetValidator`.
- [ ] No repository/service injection; no business rule.

# Unittest TestCases
- [ ] WHEN a required field is empty THEN validation fails with that property's error.
- [ ] WHEN a `Soft{ValueObject}` property is invalid THEN the composed property validator's error surfaces.
- [ ] WHEN every field is well-formed THEN validation passes.
