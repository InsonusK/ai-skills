---
description: Add ActionTimeStamp and ICommandWithTimestamp to create/update commands
project_name: "{Module}.Interfaces"
name: "{Command}.cs"
element_kind: class
change_kind: extend
tags:
  - solution/entity-edit-timestamp
  - element/command-cs
---

# Goals
- Make create and update commands carry the user action timestamp.
- Mark these commands so validators and handlers can rely on `ICommandWithTimestamp`.

# Core Principles
- `ActionTimeStamp` is the first property — it signals that the command records an action in time.
- Commands implement both `ICommand<Result<T>>` and `ICommandWithTimestamp`.
- Result records remain unchanged.

# Structure

## Project Structure
```
/{Module}.Interfaces
  /Commands
    {Command}.cs
```

# Naming convention
| use case | record name pattern | record name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Create command with timestamp | `Create{Entity}Command` | `CreateTaskCommand` | `Create{Entity}Command.cs` | `CreateTaskCommand.cs` |
| Update command with timestamp | `Update{Entity}Command` | `UpdateTaskCommand` | `Update{Entity}Command.cs` | `UpdateTaskCommand.cs` |

# Implementation changes

Create command for a timestamped entity:

```csharp
// {Module}.Interfaces/Commands/Create{Entity}Command.cs
using Ardalis.Result;
using Shared.MediatR;
using Shared.Timestamps;

namespace {Module}.Interfaces.Commands;

public record Create{Entity}Command(
    DateTimeOffset ActionTimeStamp,
    // ... other properties
) : ICommand<Result<Create{Entity}Result>>, ICommandWithTimestamp;

public record Create{Entity}Result(int Id);
```

Update command for a timestamped entity:

```csharp
// {Module}.Interfaces/Commands/Update{Entity}Command.cs
using Ardalis.Result;
using Shared.MediatR;
using Shared.Timestamps;

namespace {Module}.Interfaces.Commands;

public record Update{Entity}Command(
    DateTimeOffset ActionTimeStamp,
    int {Entity}Id,
    // ... other properties
) : ICommand<Result>, ICommandWithTimestamp;
```
# Rule changes

## MUST
- `ActionTimeStamp` is the first property.
- Command implements both `ICommand<Result<T>>` and `ICommandWithTimestamp`.
- `ActionTimeStamp` is typed as `DateTimeOffset`.
- Command validators check that `ActionTimeStamp` is not `default(DateTimeOffset)` and is not greater than `DateTimeOffset.UtcNow`.

## SHOULD
- Keep timestamp interfaces and the command marker free of behavior logic.
- Name the command timestamp property `ActionTimeStamp` consistently.

## MUST NOT
- Delete commands implement `ICommandWithTimestamp`.
- Commands targeting `Internal Immutable` entities implement `ICommandWithTimestamp`.

# Anti-patterns
- `ActionTimeStamp` not as the first property.
- Adding `ActionTimeStamp` to commands that do not affect timestamped entities.

# Check list
- [ ] `ActionTimeStamp` is the first property.
- [ ] Create/update command implements `ICommandWithTimestamp`.
- [ ] `ActionTimeStamp` is `DateTimeOffset`.

# Unittest TestCases
- [ ] WHEN create command implements `ICommandWithTimestamp` THEN it exposes `ActionTimeStamp`.
- [ ] WHEN update command implements `ICommandWithTimestamp` THEN it exposes `ActionTimeStamp`.
- [ ] WHEN naming THEN pattern matches convention.
