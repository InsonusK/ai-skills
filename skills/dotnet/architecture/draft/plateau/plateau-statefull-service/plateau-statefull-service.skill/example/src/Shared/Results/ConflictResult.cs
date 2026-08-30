using Ardalis.Result;

namespace Shared.Results;

public sealed class ConflictResult<T> : Result<T>
{
    private ConflictResult(T value) : base(value) => Status = ResultStatus.Conflict;

    public static ConflictResult<T> For(T existing) => new(existing);
}
