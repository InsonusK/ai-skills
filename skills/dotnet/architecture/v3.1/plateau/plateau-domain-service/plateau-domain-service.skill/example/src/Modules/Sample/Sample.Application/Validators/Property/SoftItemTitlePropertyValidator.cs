using FluentValidation;
using Sample.Interfaces.ValueObjects;

namespace Sample.Application.Validators.Property;

public sealed class SoftItemTitlePropertyValidator : AbstractValidator<SoftItemTitle>
{
    public SoftItemTitlePropertyValidator()
    {
        RuleFor(x => x.Value).NotEmpty().WithErrorCode("Sample.Title.Required");
        RuleFor(x => x.Value).MaximumLength(100).WithErrorCode("Sample.Title.TooLong");
    }
}
