---
description: Exception carrying existing entity result for 409 responses
project_name: Shared
name: ConflictException.cs
element_kind: class
change_kind: create
---
# Goals
- Carry the existing entity result from `GuidResolvingBehavior` to the `ConflictExceptionMiddleware`
- Enable the middleware to write the existing entity in the 409 response body — client recovers without a second GET
- Provide a non-generic base class so middleware can catch all `ConflictException<T>` instances without knowing `T`

# Core Principles
- Non-generic `ConflictException` base class lives in Shared — middleware catches this type
- Generic `ConflictException<T>` carries the typed existing result and implements `GetValue()` to extract the body
- `GetValue()` unwraps `Result<T>.Value` when `T` is an Ardalis `Result<>`; otherwise returns the object itself
- Generic on the result type `T` — typed to the command's result type (e.g. `Result<CreateTaskResult>`)
- Not a domain exception — it is a pipeline coordination exception

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Guid conflict exception base | `ConflictException` | `ConflictException` | `ConflictException.cs` | `ConflictException.cs` |
| Guid conflict exception | `ConflictException<T>` | `ConflictException<Result<CreateTaskResult>>` | `ConflictException.cs` | `ConflictException.cs` |

# Implementation changes

```csharp
// Shared/Exceptions/ConflictException.cs
using System.Reflection;

public abstract class ConflictException : Exception
{
    protected ConflictException(string message)
        : base(message) { }

    public abstract object? GetValue();
}

public class ConflictException<T> : ConflictException
{
    public T Existing { get; }

    public ConflictException(T existing)
        : base("Entity with this Guid already exists.")
        => Existing = existing;

    public override object? GetValue()
    {
        if (Existing is null)
            return null;

        var type = typeof(T);
        var valueProperty = type.GetProperty(
            "Value",
            BindingFlags.Public | BindingFlags.Instance);

        if (valueProperty is not null)
            return valueProperty.GetValue(Existing);

        return Existing;
    }
}
```

# Rules

MUST:
- Non-generic `ConflictException` base class defined in Shared
- `ConflictException<T>` defined in Shared
- `Existing` property carries the full resolved result — never just an Id
- `GetValue()` extracts the entity body from `Result<T>` wrappers
- Message always the same — middleware never reads the message, only `GetValue()`

MUST NOT:
- Carry only the Id — the full result shape is required so middleware can extract the entity body
- Define in BuildingBlocks — it is caught by both BuildingBlocks middleware and potentially other layers

# Anti-patterns
- `ConflictException` without non-generic base — middleware would need reflection or generic type matching to catch all instances
- `ConflictException<T>` carrying only an Id — client loses the full existing entity needed for recovery

# Check list
- [ ] `ConflictException` non-generic base defined in `Shared/Exceptions/ConflictException.cs`
- [ ] `ConflictException<T>` defined in `Shared/Exceptions/ConflictException.cs`
- [ ] `GetValue()` unwraps `Result<T>.Value`

# Unittest TestCases
- [ ] WHEN applied THEN Carry the existing entity result from GuidResolvingBehavior to the ConflictExceptionMiddleware
- [ ] WHEN applied THEN Enable the middleware to write the existing entity in the 409 response body — client recovers without a second GET
- [ ] WHEN component is requested THEN it provide a non-generic base class so middleware can catch all ConflictException<T> instances without knowing T
- [ ] WHEN applied THEN Non-generic ConflictException base class lives in Shared — middleware catches this type
- [ ] WHEN applied THEN Generic ConflictException<T> carries the typed existing result and implements GetValue() to extract the body
- [ ] WHEN applied THEN GetValue() unwraps Result<T>.Value when T is an Ardalis Result<>; otherwise returns the object itself
- [ ] WHEN applied THEN Generic on the result type T — typed to the command's result type (e.g. Result<CreateTaskResult>)
- [ ] WHEN applied THEN Not a domain exception — it is a pipeline coordination exception
- [ ] WHEN verified THEN ConflictException non-generic base defined in Shared/Exceptions/ConflictException.cs
- [ ] WHEN verified THEN ConflictException<T> defined in Shared/Exceptions/ConflictException.cs
- [ ] WHEN verified THEN GetValue() unwraps Result<T>.Value
- [ ] WHEN naming 'Guid conflict exception base' THEN pattern matches convention
- [ ] WHEN naming 'Guid conflict exception' THEN pattern matches convention
