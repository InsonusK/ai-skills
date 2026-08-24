namespace Shared.Exceptions;

public class DomainException : Exception
{
    public string? Code { get; }

    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public DomainException(string code, string message)
        : base(message)
    {
        Code = code;
    }
}
