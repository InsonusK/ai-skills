---
description: Create command implements IHasGuid
project_name: "{Module}.Interfaces"
name: "{Command}.cs"
element_kind: class
change_kind: extend
---

# Goals
- Add `Guid` as a required property on create commands for externally-created entity types
- Implement `IHasGuid` alongside `ICommand<Result<T>>`

# Core Principles
- `Guid` is the first property — signals to the reader that this is an external-created entity
- Command implements `IHasGuid` from Shared — `{Module}.Interfaces` does not reference BuildingBlocks
- Result record unchanged from solution-command-integration.skill — still just `{Entity}Id`
- Both 201 Created and 409 Conflict return the same response type (`Result<Create{Entity}Result>`)

# Naming convention
| use case | record name pattern | record name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Create command | `Create{Entity}Command` | `CreateTaskCommand` | `Create{Entity}Command.cs` | `CreateTaskCommand.cs` |

# Implementation changes
Create command extended with `Guid` and `IHasGuid`:

```csharp
// {Module}.Interfaces/Commands/Create{Entity}Command.cs
public record Create{Entity}Command(
    Guid Guid,         // ← client-generated, first property
    // ... other properties
) : ICommand<Result<Create{Entity}Result>>, IHasGuid;

public record Create{Entity}Result(int Id);
```
# Rule changes

## MUST
- `Guid` is the first property on the command record
- Command implements both `ICommand<Result<T>>` and `IHasGuid`
- `Guid` typed as `System.Guid` — never `string` or `int`
- Result record co-located with the command
- Resolver response type matches `Result<Create{Entity}Result>` exactly
- 409 response body contains the existing entity result — which is `{ id: ... }` because the result contains only Id
- `Create{Entity}Result` contains only the entity Id

## SHOULD
- `Guid` be the first property in the command record — signals external-created entity at a glance

## MUST NOT
- Update, delete, or internal-create commands implement `IHasGuid`
- Resolver return a different response type than the command handler
- `Create{Entity}Result` carry fields beyond the entity Id for external-created entities

# Anti-patterns
- `Guid` not as first property — signals external-created entity at a glance
- Resolver response type different from command handler response type — breaks 201/409 symmetry

# Check list
- [ ] `Guid` is first property in create command record
- [ ] Command implements `ICommand<Result<T>>` and `IHasGuid`
- [ ] Result record co-located with command
- [ ] Resolver response type matches command handler response type

# Unittest TestCases
- [ ] WHEN applied THEN Add Guid as a required property on create commands for externally-created entity types
- [ ] WHEN inspected THEN it implement IHasGuid alongside ICommand<Result<T>>
- [ ] WHEN applied THEN Guid is the first property — signals to the reader that this is an external-created entity
- [ ] WHEN applied THEN Command carries the client-generated Guid — never a server-generated value
- [ ] WHEN applied THEN Command implements IHasGuid from Shared — {Module}.Interfaces does not reference BuildingBlocks
- [ ] WHEN applied THEN Result record unchanged from solution-command-integration.skill — still just {Entity}Id
- [ ] WHEN applied THEN Both 201 Created and 409 Conflict return the same response type
- [ ] WHEN verified THEN Guid is first property in create command record
- [ ] WHEN verified THEN Command implements ICommand<Result<T>> and IHasGuid
- [ ] WHEN naming 'Create command' THEN pattern matches convention
