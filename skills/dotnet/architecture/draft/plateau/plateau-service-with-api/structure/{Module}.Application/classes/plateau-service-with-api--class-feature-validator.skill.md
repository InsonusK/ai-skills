---
name: class-feature-validator
description: Class {FeatureName}Validator in the service-with-api plateau
whenToUse: when a new command needs transport-correctness validation before it reaches its handler
domain: skill
type: template
plateau: service-with-api
version: 20260825120000
tags:
  - skill/template/class
  - plateau/service-with-api
created_by:
  - "[[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
---

# Goal
- Validate transport correctness of one command's input before it reaches the handler
- Express validation rules as a declarative FluentValidation rule set — not imperative checks

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs.create]]

# Core Principles
- Extends `AbstractValidator<TCommand>`, rules defined in the constructor via `RuleFor(...)`
- Transport correctness only: `NotEmpty`, `NotNull`, `MaximumLength`, `GreaterThan`, format
- No database access, no repository injection — purely declarative on the command's properties
- For a `Soft{ValueObject}`/DTO property owned by another module, injects `IValidator<T>` from `solution-dto-property-validators` and uses `SetValidator` — never duplicates its rules

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Command validator | {FeatureName}Validator | CreateTaskValidator | {FeatureName}.Validator.cs | CreateTask.Validator.cs |

# Implementation
```csharp
//Skill: class-feature-validator
//Plateau: service-with-api
//Version: 20260825120000

public class CreateTaskValidator : AbstractValidator<CreateTaskCommand>
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
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs.create]]

# Rules
MUST:
- Extend `AbstractValidator<TCommand>`, define all rules in the constructor
- Enforce transport correctness only — presence, length, format, numeric range
- Live in `/{Module}.Application/Features/{FeatureName}/{FeatureName}.Validator.cs`
- For a `Soft{ValueObject}`/DTO property from another module, inject `IValidator<T>` and use `SetValidator`
MUST NOT:
- Inject repositories, `DbContext`, or any service
- Contain business rules — entity existence or state checks
- Be shared across multiple commands
- Duplicate rules already defined in `{ValueObject}PropertyValidator` or `{Dto}Validator`

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs.create]]

# Check list
- [ ] Extends `AbstractValidator<TCommand>`, co-located with its handler
- [ ] Only transport-correctness rules, no business logic, no I/O
- [ ] Cross-module `Soft{ValueObject}`/DTO properties use `SetValidator`, never duplicated inline

__Applied solutions:__
- [[../../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs.create]]
