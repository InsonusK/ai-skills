---
description: Validate ActionTimeStamp on commands that implement ICommandWithTimestamp
project_name: "{Module}.Application"
name: "{FeatureName}.Validator.cs"
element_kind: class
change_kind: extend
tags:
  - solution/entity-edit-timestamp
  - element/featurename-validator-cs
---

# Goals
- Reject commands whose `ActionTimeStamp` is missing or in the future before the handler runs.

# Core Principles
- Transport correctness only — no business rules.
- `ActionTimeStamp` must be a real, non-future value.

# Structure

## Project Structure
```
/{Module}.Application
  /Features
    /{FeatureName}
      {FeatureName}.Validator.cs
```

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Command validator | `{FeatureName}Validator` | `CreateTaskValidator` | `{FeatureName}.Validator.cs` | `CreateTask.Validator.cs` |

# Implementation changes

```csharp
// {Module}.Application/Features/Create{Entity}/Create{Entity}.Validator.cs
using FluentValidation;
using {Module}.Interfaces.Commands;
using Shared.Timestamps;

namespace {Module}.Application.Features.Create{Entity};

public class Create{Entity}Validator : AbstractValidator<Create{Entity}Command>
{
    public Create{Entity}Validator()
    {
        RuleFor(x => x.ActionTimeStamp)
            .NotEmpty()
            .Must(ts => ts <= DateTimeOffset.UtcNow)
            .WithMessage("ActionTimeStamp must not be in the future.");

        // ... other transport rules
    }
}
```

```csharp
// {Module}.Application/Features/Update{Entity}/Update{Entity}.Validator.cs
using FluentValidation;
using {Module}.Interfaces.Commands;

namespace {Module}.Application.Features.Update{Entity};

public class Update{Entity}Validator : AbstractValidator<Update{Entity}Command>
{
    public Update{Entity}Validator()
    {
        RuleFor(x => x.ActionTimeStamp)
            .NotEmpty()
            .Must(ts => ts <= DateTimeOffset.UtcNow)
            .WithMessage("ActionTimeStamp must not be in the future.");

        // ... other transport rules
    }
}
```

> **Note:** `NotEmpty()` on `DateTimeOffset` rejects `default(DateTimeOffset)` because it is considered empty.

# Rule changes

## MUST
- Extend `AbstractValidator<TCommand>` where `TCommand` implements `ICommandWithTimestamp`.
- Reject `default(DateTimeOffset)`.
- Reject timestamps greater than `DateTimeOffset.UtcNow`.
- Never access repositories or services.
- Never contain business rules.

## SHOULD
- Avoid checking `ActionTimeStamp` in the handler.
- Avoid using `DateTime.Now` or local time for comparison.

# Check list
- [ ] Validator extends `AbstractValidator<TCommand>`.
- [ ] Validator rejects default `ActionTimeStamp`.
- [ ] Validator rejects future `ActionTimeStamp`.

# Unittest TestCases
- [ ] WHEN `ActionTimeStamp` is default THEN validator fails.
- [ ] WHEN `ActionTimeStamp` is in the future THEN validator fails.
- [ ] WHEN `ActionTimeStamp` is current or past THEN validator passes.
- [ ] WHEN naming THEN pattern matches convention.
