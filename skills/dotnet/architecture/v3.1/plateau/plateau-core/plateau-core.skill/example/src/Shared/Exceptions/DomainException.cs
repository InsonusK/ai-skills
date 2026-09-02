namespace Shared.Exceptions;

// Present at the baseline as the shared type the exception pipeline recognises;
// entities that throw it are introduced by solution-domain-behaviour (VP1).
public sealed class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
