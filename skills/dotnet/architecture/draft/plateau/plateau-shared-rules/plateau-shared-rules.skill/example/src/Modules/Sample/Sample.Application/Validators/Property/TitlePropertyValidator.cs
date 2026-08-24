using FluentValidation;
using Sample.Domain.Rules;
using Sample.Interfaces.ValueObjects;

namespace Sample.Application.Validators.Property;

public class TitlePropertyValidator : AbstractValidator<SoftTitle>
{
    public TitlePropertyValidator() => RuleFor(x => x).TitleIsValid();
}
