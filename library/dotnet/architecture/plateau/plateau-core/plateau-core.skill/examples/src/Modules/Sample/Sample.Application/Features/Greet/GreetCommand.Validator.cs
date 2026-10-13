using FluentValidation;
using Sample.Application.Validators.Property;
using Sample.Interfaces.Commands;

namespace Sample.Application.Features.Greet;

public sealed class GreetValidator : AbstractValidator<GreetCommand>
{
    public GreetValidator(IValidator<Interfaces.ValueObjects.SoftGreeting> greeting)
        => RuleFor(x => x.Message).SetValidator(greeting);
}
