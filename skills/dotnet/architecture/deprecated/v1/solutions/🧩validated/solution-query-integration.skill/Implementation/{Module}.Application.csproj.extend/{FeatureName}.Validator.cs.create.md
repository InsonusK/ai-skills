---
description: Optional transport validator for a query input
project_name: "{Module}.Application"
name: "{FeatureName}.Validator.cs"
element_kind: class
change_kind: create
tags:
  - solution/query-integration
  - element/featurename-validator-cs
---

# Goals
- Validate query input before the handler runs
- Reuse existing property and DTO validators from `solution-dto-property-validators.skill` for cross-module properties

# Core Principles
- Extends `AbstractValidator<TQuery>`
- Rules defined in the constructor via `RuleFor(...)`
- Transport correctness only: `NotEmpty`, `NotNull`, `MaximumLength`, `GreaterThan`, `InclusiveBetween`, email format, regex format
- No database access, no repository injection
- No business logic
- For `Soft{ValueObject}` or DTO properties owned by another module, inject `IValidator<T>` from `solution-dto-property-validators.skill` and use `SetValidator`

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Query validator | `{FeatureName}Validator` | `GetTasksValidator` | `{FeatureName}.Validator.cs` | `GetTasks.Validator.cs` |

# Implementation changes

```csharp
// {Module}.Application/Queries/GetTasks/GetTasks.Validator.cs
using FluentValidation;
using {OtherModule}.Interfaces.ValueObjects;

namespace {Module}.Application.Queries.GetTasks;

public class GetTasksValidator : AbstractValidator<GetTasksQuery>
{
    public GetTasksValidator(IValidator<SoftEmail> emailValidator)
    {
        RuleFor(x => x.AssigneeId).GreaterThan(0);
        RuleFor(x => x.Email).SetValidator(emailValidator);
    }
}
```

# Rule changes

## MUST
- Extend `AbstractValidator<TQuery>`
- Define all rules in the constructor
- Enforce transport correctness only
- Be named `{FeatureName}Validator`
- Live in `/{Module}.Application/Queries/{FeatureName}/{FeatureName}.Validator.cs`
- Use `SetValidator` with injected `IValidator<T>` for `Soft{ValueObject}` and DTO properties from other modules

## MUST NOT
- Inject repositories, `DbContext`, or services
- Contain business rules
- Duplicate rules already defined in `{ValueObject}PropertyValidator` or `{Dto}Validator`

# Anti-patterns
- Query validator with business rules
- Duplicating Soft{ValueObject} validation instead of using `IValidator<Soft{ValueObject}>`

# Check list
- [ ] Extends `AbstractValidator<TQuery>`
- [ ] Named `{FeatureName}Validator`
- [ ] Located in `/{Module}.Application/Queries/{FeatureName}`
- [ ] Uses `IValidator<T>` from `solution-dto-property-validators.skill` for cross-module properties

# Unittest TestCases
- [ ] When query input is valid Then no validation errors
- [ ] When query input has invalid Soft{ValueObject} property Then validation errors are returned
- [ ] When query input is sent Then `ValidationBehavior` returns `Result.Invalid` before handler runs
