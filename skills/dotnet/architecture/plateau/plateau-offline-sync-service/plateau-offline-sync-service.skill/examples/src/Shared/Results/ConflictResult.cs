using Ardalis.Result;

namespace Shared.Results;

// A Result<T> with Conflict status that still carries the existing entity's response,
// so 201 and 409 share the same response shape. No exception, no extra metadata.
public sealed class ConflictResult<T> : Result<T>
{
    public ConflictResult(T value) : base(ResultStatus.Conflict) => Value = value;
}
