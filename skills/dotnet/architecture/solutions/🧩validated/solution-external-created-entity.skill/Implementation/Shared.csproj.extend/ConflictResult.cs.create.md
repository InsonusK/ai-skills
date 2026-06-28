---
description: Result carrying the existing entity result for 409 Conflict responses
project_name: Shared
name: ConflictResult.cs
element_kind: class
change_kind: create
---

# Goals
- Provide a typed `Result<T>` with `ResultStatus.Conflict` that carries the existing entity result
- Enable `IGuidResolver<TResponse>` to express a duplicate Guid conflict without throwing an exception
- Allow the API layer to map conflict results to a 409 response containing the existing entity result

# Core Principles
- Lives in `Shared` so both `{Module}.Application` resolvers and BuildingBlocks behavior can reference it
- Inherits from `Ardalis.Result.Result<T>` — fully compatible with existing result-based controllers and mapping
- `Status` is `ResultStatus.Conflict`
- `Value` holds the existing entity result
- Generic on `T` so it can be reused for any idempotent-create response type

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Conflict result | `ConflictResult<T>` | `ConflictResult<CreateTaskResult>` | `ConflictResult.cs` | `ConflictResult.cs` |

# Implementation changes

```csharp
// Shared/Results/ConflictResult.cs
using Ardalis.Result;

namespace Shared.Results;

public class ConflictResult<T> : Result<T>
{
    public ConflictResult(T value)
        : base(ResultStatus.Conflict)
    {
        Value = value;
    }
}
```

# Rules

MUST:
- Inherit from `Ardalis.Result.Result<T>`
- Set `Status` to `ResultStatus.Conflict`
- Accept the conflict value via constructor and assign it to `Value`
- Be defined in `Shared/Results/ConflictResult.cs`

MUST NOT:
- Carry additional metadata beyond the conflict value
- Throw exceptions in the constructor
- Be defined in BuildingBlocks — it is a primitive used by both Application and BuildingBlocks layers

# Anti-patterns
- Returning `Result<T>.Conflict()` and losing the existing entity result
- Storing the conflict payload in `Errors` or `Location` instead of `Value`
- Defining `ConflictResult<T>` in BuildingBlocks — forces Shared to reference BuildingBlocks

# Check list
- [ ] `ConflictResult<T>` defined in `Shared/Results/ConflictResult.cs`
- [ ] Inherits from `Ardalis.Result.Result<T>`
- [ ] Constructor accepts `T value` and assigns it to `Value`
- [ ] `Status` is `ResultStatus.Conflict`

# Unittest TestCases
- [ ] WHEN applied THEN it carries the existing entity result in `Value`
- [ ] WHEN applied THEN `Status` equals `ResultStatus.Conflict`
- [ ] WHEN applied THEN it is assignable to `Result<T>`
- [ ] WHEN applied THEN JSON serialization includes the conflict value
- [ ] WHEN naming 'Conflict result' THEN pattern matches convention
