---
description: Commands for timestamped entities implement ICommandWithTimestamp
name: "{Module}.Interfaces.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/entity-edit-timestamp
  - element/module-interfaces-csproj
---

# Goals
- Express the user action time on commands that create or update timestamped entities.
- Keep command records immutable and free of logic.

# Core Principles
- Commands remain declarations only — records with properties.
- Commands implement `ICommandWithTimestamp` alongside `ICommand<Result<T>>`.
- `ActionTimeStamp` is a required property and is listed first.

# Structure

## Project Structure
```
/{Module}.Interfaces
  /Commands
    {Command}.cs
```

## Directory and class skills
| Directory \ file | Description |
| ----------------- | ----------- |
| /Commands/{Command}.cs | Command record with `ActionTimeStamp` and `ICommandWithTimestamp` |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Required for `ICommand<T>` marker usage. |
| `Ardalis.Result` | latest stable | Required for `Result<T>` return type usage. |

# Allowed Dependencies
- Shared

# Rules

## MUST
- Create and update commands for timestamped entities implement `ICommandWithTimestamp`.
- `ActionTimeStamp` is the first property on the command record.
- Commands remain records with no methods.
- Never implement `ICommandWithTimestamp` on a delete command or a command targeting an `Internal Immutable` entity.
- Never add timestamp logic to command records.

## SHOULD
- Avoid putting `ActionTimeStamp` last or in the middle of the record — it is the user-supplied action context and should be visible first.
- Avoid implementing `ICommandWithTimestamp` on read-only queries.

# Check list
- [ ] Create commands for timestamped entities implement `ICommandWithTimestamp`.
- [ ] Update commands for timestamped entities implement `ICommandWithTimestamp`.
- [ ] `ActionTimeStamp` is the first property.
- [ ] Commands remain records with no logic.
