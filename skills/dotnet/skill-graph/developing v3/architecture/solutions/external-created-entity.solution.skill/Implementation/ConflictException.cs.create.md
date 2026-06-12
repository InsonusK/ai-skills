---
description: Exception carrying existing entity result for 409 responses
name: ConflictException.cs
change_kind: create
---

# Goals
- Carry the existing entity result from `GuidResolvingBehavior` to the API controller
- Enable the controller to return the existing entity in the 409 response body — client recovers without a second GET

# Core Principles
- Generic on result type `T` — typed to the command's result type (e.g. `Result<CreateTaskResult>`)
- Single property: `Existing` — the resolved result from `IGuidResolver<T>`
- Controller catches this specific type and extracts `Existing.Value` for the 409 body

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid conflict exception | `ConflictException<T>` | `ConflictException<Result<CreateTaskResult>>` | `ConflictException.cs` | `ConflictException.cs` |

# Implementation changes

```csharp
// Shared/Exceptions/ConflictException.cs
public class ConflictException<T> : Exception
{
    public T Existing { get; }

    public ConflictException(T existing)
        : base("Entity with this Guid already exists.")
        => Existing = existing;
}
```

# Rules

MUST:
- `Existing` property carries the full resolved result — never just an Id
- Message always the same — controller never reads the message, only `Existing`

MUST NOT:
- Carry only the Id — the full result shape is required for client recovery

# Anti-patterns
- `ConflictException` without generic parameter — controller loses typed access to the existing entity

# Check list
- [ ] `ConflictException<T>` defined in `Shared/Exceptions/ConflictException.cs`
- [ ] `Existing` property carries full result type
