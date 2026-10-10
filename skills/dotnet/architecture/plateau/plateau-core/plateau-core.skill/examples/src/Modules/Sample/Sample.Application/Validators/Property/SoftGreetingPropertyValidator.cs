using FluentValidation;
using Sample.Interfaces.ValueObjects;

namespace Sample.Application.Validators.Property;

// Owns its own local condition; resolvable cross-module via IValidator<SoftGreeting>.
public sealed class SoftGreetingPropertyValidator : AbstractValidator<SoftGreeting>
{
    public SoftGreetingPropertyValidator()
    {
        RuleFor(x => x.Value).NotEmpty().WithErrorCode("Sample.Greeting.Required");
        RuleFor(x => x.Value).MaximumLength(100).WithErrorCode("Sample.Greeting.TooLong");
    }
}
