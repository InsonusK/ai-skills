---
uid: 2c8e47cf-7be5-44bb-a5fc-dba32a848e8a
name: class-feature-validator
description: Transport correctness validator
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration.skill]]"
---

# Goal
- Validate transport correctness of one command's input before it reaches the handler
- Express validation rules as a declarative FluentValidation rule set — not imperative checks

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs.create]]

# Core Principals
- Extends `AbstractValidator<TCommand>`
- Rules defined in constructor via `RuleFor(...)`
- Transport correctness only: `NotEmpty`, `NotNull`, `MaximumLength`, `GreaterThan`, `InclusiveBetween`, email format, regex format
- No database access, no repository injection — purely declarative on the command's properties
- No business logic — existence and state checks belong in handler guard or domain

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Command validator | `{FeatureName}Validator` | `CreateTaskValidator` | `{FeatureName}.Validator.cs` | `CreateTask.Validator.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs.create]]

# Implementation
Validator declares rules for each command property in the constructor:

```csharp
// {Module}.Application/Features/CreateTask/CreateTask.Validator.cs
using FluentValidation;

namespace {Module}.Application.Features.CreateTask;

public class CreateTaskValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.AssigneeId)
            .GreaterThan(0);
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

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs.create]]

# Rules
MUST:
	- Extend `AbstractValidator<TCommand>`
	- Define all rules in the constructor
	- Enforce transport correctness only — presence, length, format, numeric range
	- Be named `{FeatureName}Validator`
	- Live in `/{Module}.Application/Features/{FeatureName}/{FeatureName}.Validator.cs`
MUST NOT:
	- Inject repositories, `DbContext`, or any service — purely declarative on command properties
	- Contain business rules — entity existence checks, state checks, or invariant enforcement
	- Be shared across multiple commands

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Validate transport correctness of one command's input before it reaches the handler
- [ ] WHEN applied THEN Express validation rules as a declarative FluentValidation rule set — not imperative checks
- [ ] WHEN applied THEN Extends AbstractValidator<TCommand>
- [ ] WHEN applied THEN Rules defined in constructor via RuleFor(...)
- [ ] WHEN applied THEN Transport correctness only: NotEmpty, NotNull, MaximumLength, GreaterThan, InclusiveBetween, email format, regex format
- [ ] WHEN applied THEN No database access, no repository injection — purely declarative on the command's properties
- [ ] WHEN applied THEN No business logic — existence and state checks belong in handler guard or domain
- [ ] WHEN naming 'Command validator' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend/{FeatureName}.Validator.cs.create.md|{FeatureName}.Validator.cs.create]]
