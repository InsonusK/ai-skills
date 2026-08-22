using Sample.Interfaces.ValueObjects;
using Shared.Exceptions;

namespace Sample.Domain.ValueObjects;

public sealed record Email : SoftEmail
{
    public Email(string value) : base(value)
    {
        if (!IsValid(value))
            throw new DomainException("Sample.Email.Invalid", "Email is not valid.");
    }

    private static bool IsValid(string value) => !string.IsNullOrWhiteSpace(value) && value.Contains('@');

    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string value) => new(value);
}
