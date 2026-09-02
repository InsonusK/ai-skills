using FluentValidation;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;

namespace Sample.Application.Features.RenameItem;

public sealed class RenameItemValidator : AbstractValidator<RenameItemCommand>
{
    public RenameItemValidator(IValidator<SoftItemTitle> title)
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.NewTitle).SetValidator(title);
        RuleFor(x => x.ExpectedVersion).GreaterThan(0u).WithErrorCode("Sample.Version.Required");
        RuleFor(x => x.ActionTimeStamp)
            .Must(ts => ts != default).WithErrorCode("Sample.ActionTimeStamp.Required")
            .Must(ts => ts <= DateTimeOffset.UtcNow.AddMinutes(1)).WithErrorCode("Sample.ActionTimeStamp.Future");
    }
}
