---
description: Replace solution-dto-property-validators's local cross-field Must(...) with a call to a centralized, Semantic-classified rule extension
project_name: "{Module}.Application"
name: "{Dto}.Validator.cs"
element_kind: class
change_kind: extend
tags:
  - solution/domain-rules
  - element/dto-validator-cs
---

# Goals
- Reuse a cross-field condition that was found duplicated between this DTO validator and an Entity method

# Implementation changes

Before (per `solution-dto-property-validators`, local condition):

```csharp
public class TodoTaskPostRequestDtoValidator : AbstractValidator<TodoTaskPostRequestDto>
{
    public TodoTaskPostRequestDtoValidator(IValidator<SoftComplexity> complexityValidator)
    {
        RuleFor(x => x.Complexity).SetValidator(complexityValidator);

        RuleFor(dto => dto)
            .Must(dto => dto.StartDateTime is null || dto.DueDateTime is null || dto.DueDateTime >= dto.StartDateTime)
            .WithMessage("Due date must not be earlier than start date.");
    }
}
```

After (redirected to a centralized Semantic rule — the wrapper `SoftSchedule` is assembled ad hoc from the DTO's own separate fields, reusing the same condition `TodoTask.UpdateSchedule` also uses):

```csharp
public class TodoTaskPostRequestDtoValidator : AbstractValidator<TodoTaskPostRequestDto>
{
    public TodoTaskPostRequestDtoValidator(IValidator<SoftComplexity> complexityValidator)
    {
        RuleFor(x => x.Complexity).SetValidator(complexityValidator);

        // SoftSchedule assembled ad hoc, purely to reuse ScheduleRules — the DTO itself
        // doesn't store this value as one property.
        RuleFor(dto => new SoftSchedule(dto.StartDateTime, dto.DueDateTime)).ScheduleIsValid();
    }
}
```

The local `.Must(...)`/`.WithMessage(...)` is deleted — `{Module}.Domain.Rules` is now the only place the condition exists.

# Rule changes

## MUST
- Call the centralized extension for a redirected cross-field condition instead of a local `.Must(...)`
- Delete the local condition this file used to define

## MUST NOT
- Keep the local condition alongside the centralized one
