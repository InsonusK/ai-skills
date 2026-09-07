using FluentValidation;
using Sample.Interfaces.Commands;

namespace Sample.Application.Features.CompleteItem;

public sealed class CompleteItemValidator : AbstractValidator<CompleteItemCommand>
{
    public CompleteItemValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.ExpectedVersion).GreaterThan(0u).WithErrorCode("Sample.Version.Required");
        RuleFor(x => x.ActionTimeStamp)
            .Must(ts => ts != default).WithErrorCode("Sample.ActionTimeStamp.Required")
            .Must(ts => ts <= DateTimeOffset.UtcNow.AddMinutes(1)).WithErrorCode("Sample.ActionTimeStamp.Future");
    }
}
