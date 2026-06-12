---
description: Create command implements IHasGuid
name: "{Command}.cs"
change_kind: extend
---

# Goals
- Add `Guid` as a required property on create commands for externally-created entity types
- Implement `IHasGuid` alongside `ICommand<Result<T>>`

# Core Principles
- `Guid` is the first property — signals to the reader that this is an external-created entity
- Command carries the client-generated Guid — never a server-generated value
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
