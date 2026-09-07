using FluentValidation;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;

namespace Sample.Application.Features.AddItem;

public sealed class AddItemValidator : AbstractValidator<AddItemCommand>
{
    public AddItemValidator(IValidator<SoftItemTitle> title)
    {
        RuleFor(x => x.Title).SetValidator(title);
        RuleFor(x => x.Guid).NotEqual(System.Guid.Empty).WithErrorCode("Sample.Guid.Required");
        RuleFor(x => x.ActionTimeStamp)
            .Must(ts => ts != default).WithErrorCode("Sample.ActionTimeStamp.Required")
            .Must(ts => ts <= DateTimeOffset.UtcNow.AddMinutes(1)).WithErrorCode("Sample.ActionTimeStamp.Future");
    }
}
