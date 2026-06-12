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
- Command carries the client-generated Guid — never a server-generated value
- Command implements `IHasGuid` from Shared — `{Module}.Interfaces` does not reference BuildingBlocks
- Result record unchanged from command-integration.solution.skill — still just `{Entity}Id`

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

# Rules

MUST:
- `Guid` is the first property on the command record
- Command implements both `ICommand<Result<T>>` and `IHasGuid`
- `Guid` typed as `System.Guid` — never `string` or `int`

MUST NOT:
- Update, delete, or internal-create commands implement `IHasGuid`

# Anti-patterns
- `Guid` not as first property — signals external-created entity at a glance

# Check list
- [ ] `Guid` is first property in create command record
- [ ] Command implements `ICommand<Result<T>>` and `IHasGuid`

# Unittest TestCases
- [ ] WHEN applied THEN Add Guid as a required property on create commands for externally-created entity types
- [ ] WHEN inspected THEN it implement IHasGuid alongside ICommand<Result<T>>
- [ ] WHEN applied THEN Guid is the first property — signals to the reader that this is an external-created entity
- [ ] WHEN applied THEN Command carries the client-generated Guid — never a server-generated value
- [ ] WHEN applied THEN Command implements IHasGuid from Shared — {Module}.Interfaces does not reference BuildingBlocks
- [ ] WHEN applied THEN Result record unchanged from command-integration.solution.skill — still just {Entity}Id
- [ ] WHEN verified THEN Guid is first property in create command record
- [ ] WHEN verified THEN Command implements ICommand<Result<T>> and IHasGuid
- [ ] WHEN naming 'Create command' THEN pattern matches convention
