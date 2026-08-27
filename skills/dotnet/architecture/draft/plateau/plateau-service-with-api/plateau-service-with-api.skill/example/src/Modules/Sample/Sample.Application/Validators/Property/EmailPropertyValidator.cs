using FluentValidation;
using Sample.Interfaces.ValueObjects;

namespace Sample.Application.Validators.Property;

public class EmailPropertyValidator : AbstractValidator<SoftEmail>
{
    public EmailPropertyValidator()
        => RuleFor(x => x).Must(IsValid).WithMessage("Email is not valid.");

    private static bool IsValid(SoftEmail email) => !string.IsNullOrWhiteSpace(email.Value) && email.Value.Contains('@');
}
