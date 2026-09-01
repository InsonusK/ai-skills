---
description: Transport correctness validator
project_name: "{Module}.Application"
name: "{FeatureName}.Validator.cs"
element_kind: class
change_kind: create
tags:
  - solution/mediator-integration
  - element/featurename-validator-cs
---

# Goals
- Validate transport correctness of one command's input before it reaches the handler
- Express validation rules as a declarative FluentValidation rule set — not imperative checks

# Core Principles
- Extends `AbstractValidator<TCommand>`
- Rules defined in constructor via `RuleFor(...)`
- Transport correctness only: `NotEmpty`, `NotNull`, `MaximumLength`, `GreaterThan`, `InclusiveBetween`, email format, regex format
- No database access, no repository injection — purely declarative on the command's properties
- No business logic — existence and state checks belong in handler guard or domain
- For `Soft{ValueObject}` or DTO properties coming from another module, inject `IValidator<T>` from `solution-dto-property-validators.skill` and use `SetValidator`

## What belongs in a validator vs domain

| Concern | Belongs in | Example |
| --- | --- | --- |
| Field is required | Validator | `RuleFor(x => x.Title).NotEmpty()` |
| Max string length | Validator | `RuleFor(x => x.Title).MaximumLength(200)` |
| Numeric range | Validator | `RuleFor(x => x.AssigneeId).GreaterThan(0)` |
| Valid format | Validator | `RuleFor(x => x.Email).EmailAddress()` |
| Entity must exist | Handler guard | `if (assignee is null) return Result.NotFound()` |
| Business state allows action | Domain entity | `task.Assign(assigneeId)` throws `DomainException` |
| Unique constraint | Domain / DB | Unique index + handler conflict guard |

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Command validator | `{FeatureName}Validator` | `CreateTaskValidator` | `{FeatureName}.Validator.cs` | `CreateTask.Validator.cs` |

# Implementation changes

Validator declares rules for each command property in the constructor:

```csharp
// {Module}.Application/Features/CreateTask/CreateTask.Validator.cs
using FluentValidation;
using {OtherModule}.Interfaces.ValueObjects;

namespace {Module}.Application.Features.CreateTask;

public class CreateTaskValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskValidator(IValidator<SoftEmail> emailValidator)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.AssigneeId)
            .GreaterThan(0);

        RuleFor(x => x.Email)
            .SetValidator(emailValidator);
    }
}
```

```csharp
// {Module}.Application/Features/AssignTask/AssignTask.Validator.cs
using FluentValidation;

namespace {Module}.Application.Features.AssignTask;

public class AssignTaskValidator : AbstractValidator<AssignTaskCommand>
{
    public AssignTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .GreaterThan(0);

        RuleFor(x => x.AssigneeId)
            .GreaterThan(0);
    }
}
```

# Rule changes

## MUST
- Extend `AbstractValidator<TCommand>`
- Define all rules in the constructor
- Enforce transport correctness only — presence, length, format, numeric range
- Be named `{FeatureName}Validator`
- Live in `/{Module}.Application/Features/{FeatureName}/{FeatureName}.Validator.cs`
- For `Soft{ValueObject}` or DTO properties owned by another module, inject `IValidator<T>` from `solution-dto-property-validators.skill` and use `SetValidator`
- Validators registered via `AddValidatorsFromAssembly` in module registration — this also registers property validators and DTO validators from `solution-dto-property-validators.skill`
- Never inject repositories, `DbContext`, or any service — purely declarative on command properties
- Never contain business rules — entity existence checks, state checks, or invariant enforcement
- Never be shared across multiple commands

# Unittest TestCases
- [ ] WHEN applied THEN Validate transport correctness of one command's input before it reaches the handler
- [ ] WHEN applied THEN Express validation rules as a declarative FluentValidation rule set — not imperative checks
- [ ] WHEN applied THEN Extends AbstractValidator<TCommand>
- [ ] WHEN applied THEN Rules defined in constructor via RuleFor(...)
- [ ] WHEN applied THEN Transport correctness only: NotEmpty, NotNull, MaximumLength, GreaterThan, InclusiveBetween, email format, regex format
- [ ] WHEN applied THEN No database access, no repository injection — purely declarative on the command's properties
- [ ] WHEN applied THEN No business logic — existence and state checks belong in handler guard or domain
- [ ] WHEN naming 'Command validator' THEN pattern matches convention
