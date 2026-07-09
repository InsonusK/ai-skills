---
description: Handlers assign user timestamps; validators reject invalid ActionTimeStamp
name: "{Module}.Application.csproj"
element_kind: project
change_kind: extend
---

# Goals
- Validate `ActionTimeStamp` early in the command validator.
- Assign user timestamps in handlers from the validated `ActionTimeStamp`.
- Keep handlers as orchestrators without business rules.

# Core Principles
- Validation of `ActionTimeStamp` is transport correctness and belongs in the validator.
- Handlers know whether the command creates or updates the entity and set the correct user timestamp fields.
- Handlers cast the entity to the mutable timestamp interface to assign values while respecting internal setters.

# Structure

## Project Structure
```
/{Module}.Application
  /Features
    /{FeatureName}
      {FeatureName}.Handler.cs
      {FeatureName}.Validator.cs
```

## Directory and class skills
| Directory \ file | Description |
| ----------------- | ----------- |
| /Features/{FeatureName}/{FeatureName}.Handler.cs | Handler assigning user timestamps |
| /Features/{FeatureName}/{FeatureName}.Validator.cs | Validator checking `ActionTimeStamp` |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `FluentValidation` | latest stable | Provides `AbstractValidator<T>` for `ActionTimeStamp` rules. |
| `Ardalis.Result` | latest stable | Provides `Result<T>` return types. |
| `MediatR` | latest stable | Provides handler interfaces. |

# Allowed Dependencies
- Shared
- {Module}.Domain
- {Module}.Interfaces

# Rules

## MUST
- Commands implementing `ICommandWithTimestamp` have a validator that checks `ActionTimeStamp` is not default and not in the future.
- Create handlers for mutable entities set both `UserCreatedDateTime` and `UserUpdatedDateTime`.
- Update handlers set only `UserUpdatedDateTime`.
- Create handlers for `External Immutable` entities set only `UserCreatedDateTime`.
- Handlers assign timestamps through the mutable interface (`ICreationInfoModel` / `IUpdateInfoModel`).

## MUST NOT
- Validate `ActionTimeStamp` in the handler.
- Set server timestamps in the handler.
- Set user timestamps in `AppDbContext`.

# Anti-patterns
- Duplicating timestamp validation in every handler.
- Assigning user timestamps without casting to the mutable interface — fails when entity setters are `internal`.
- Forgetting to set `UserUpdatedDateTime` on creation for mutable entities.

# Check list
- [ ] Validator checks `ActionTimeStamp` is not default and not in the future.
- [ ] Create handler sets `UserCreatedDateTime` and `UserUpdatedDateTime` for mutable entities.
- [ ] Update handler sets only `UserUpdatedDateTime`.
- [ ] Create handler for `External Immutable` sets only `UserCreatedDateTime`.
