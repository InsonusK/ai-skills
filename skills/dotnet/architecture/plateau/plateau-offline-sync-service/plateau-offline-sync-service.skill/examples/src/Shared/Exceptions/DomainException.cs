namespace Shared.Exceptions;

// One exception type for any guarded entity method whose condition fails.
// Code convention: {Module}.{Entity}.{Reason}
public sealed class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
