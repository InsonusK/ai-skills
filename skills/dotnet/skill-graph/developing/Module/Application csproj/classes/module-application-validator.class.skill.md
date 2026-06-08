---
name: feature-validator
description: rules for implementing FluentValidation validators for command handlers and query handlers
domain: skill
type: template
version: 20260606
tags:
  - skill/template/class
  - dotnet
  - application
  - validation
  - fluentvalidation
triggers:
  - implement validator
  - validate command
  - command validation
aliases:
  - Application Validator
---
# Goal
Define how to write a `FluentValidation` validator for a command and query. The validator is the single place that ensures all input is valid before the handler runs. They run before the handler via `ValidationBehavior` in the `MediatR` pipeline based on [[skills/dotnet/skill-graph/developing/Module/Application csproj/Solutions/command-handling.solution.skill|command-handler-pattern.skill]]. It collects all failures and returns them together as a structured 400 response. The handler never receives invalid input. 

# Core Principles
- Validator is the single validation gate — handler receives only fully valid input
- Validator uses domain rules directly — no duplication, domain rules are the single source of truth
- Validator may inject `IReadRepository<T>` — cross-entity existence and business rule checks belong here
- All errors collected and returned together — never one error at a time
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/value-object.skill.skill|Value Object]] constructors are the last line of defense — they should never throw if validator did its job
- One validator per command — required
- Query validators are optional — add one when the query has enforceable constraints (pagination limits, max range size, required filter combinations)

# What Belongs in Validator

| Concern                                                  | How                                                                                                                                            |
| -------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| Field not null/empty                                     | FluentValidation built-in rules                                                                                                                |
| VO invariant (email format, positive value, max length)  | [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-rule.class.skill\|Domain rule]] via `Must(x => x.IsValidEmail())` |
| Cross-entity existence check ("assignee exists")         | `MustAsync` with `IReadRepository<T>`                                                                                                          |
| Business rule requiring DB ("task has no open subtasks") | `MustAsync` with `IReadRepository<T>`                                                                                                          |
| Entity state transition guard                            | Domain entity method — last line of defense                                                                                                    |
# Structure
## Place in csproj
Defined in [[skills/dotnet/skill-graph/developing/Module/Application csproj/module-application.csproj.skill|module-application.csproj.skill]]
```
/{ModuleName}.Application
  /Features
    /CreateTask
      CreateTask.Handler.cs
      CreateTask.Validator.cs   ← same folder as handler
```

## Naming convention
- class name
	- rule: CommandName + Validator suffix
	- pattern: {CommandName}Validator
	- example: {CommandName}Validator
- file name:
	- rule: CommandName + .Validator.cs
	- pattern: {CommandName}.Validator.cs
	- example: {CommandName}.Validator.cs 

## Implementation
```csharp
// Task.Application/Commands/CreateTask/CreateTask.Validator.cs
public class CreateTaskValidator : AbstractValidator<CreateTaskCommand>
{
    private readonly IReadRepository<User> _userRepository;

    public CreateTaskValidator(IReadRepository<User> userRepository)
    {
        _userRepository = userRepository;

        RuleFor(x => x.Guid)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .Must(t => t.IsValidTaskTitle())   // domain rule — single source of truth
            .WithMessage("Title is invalid");

        RuleFor(x => x.AssigneeId)
            .GreaterThan(0)
            .MustAsync(async (id, ct) =>
                await _userRepository.AnyAsync(new UserByIdSpec(id), ct))
            .WithMessage("Assigned user does not exist");
    }
}
```

# Rules
MUST:
- One validator per command — in the same feature folder as handler
- Use domain rules for VO invariant checks — never reimplement the rule inline
- Inject `IReadRepository<T>` for existence and business rule checks that require DB
- Collect all errors — `FluentValidation` runs all rules before returning
- Registered via assembly scan — not manually
SHOULD:
- Add validator for query handler when query has enforceable constraints
MUST NOT:
- Reimplement VO invariant logic inline — use domain rules
- Inject `IRepository<T>` — read-only checks only, use `IReadRepository<T>`
- Call `SaveChangesAsync` or modify any state

# Anti-patterns
- Inline invariant check: `.Must(e => e.Contains("@"))` — use domain rule `e.IsValidEmail()` instead
- Existence check in handler guard instead of validator — all errors should be collected before handler runs
- No validator for a command — handler may receive invalid or inconsistent input

# Checklist
- [ ] Validator exists for every command handler
- [ ] Query handler has validator if it has enforceable constraints (pagination, max range, etc.)
- [ ]  VO invariant checks use domain rules
- [ ]  Existence checks use `MustAsync` with `IReadRepository<T>`
- [ ]  No `IRepository<T>` — only `IReadRepository<T>`
- [ ]  No state modification

# Unittest TestCases
- [ ]  When required field is empty Then Result.Invalid returned with that field's error
- [ ]  When multiple fields invalid Then all errors returned together in one response
- [ ]  When VO invariant violated Then domain rule returns false and error collected
- [ ]  When referenced entity does not exist Then error collected
- [ ]  When all fields valid Then handler executes
# Relations
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-rule.class.skill|domain-rule-pattern.skill]] — domain rules used inside validator for VO invariant checks
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/classes/module-application-command-handler.class.skill|feature-command-handler.skill]] — handler that runs after this validator passes
- [[skills/dotnet/skill-graph/developing/Module/Application csproj/Solutions/command-handling.solution.skill]] — `ValidationBehavior` pipeline position
- [[files/repository.skill|repository.skill]] — IReadRepository injected for DB checks
