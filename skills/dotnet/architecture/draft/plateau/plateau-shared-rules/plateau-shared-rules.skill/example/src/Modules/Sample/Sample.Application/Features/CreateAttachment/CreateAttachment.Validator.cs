using FluentValidation;
using Sample.Interfaces.Commands;

namespace Sample.Application.Features.CreateAttachment;

public class CreateAttachmentValidator : AbstractValidator<CreateAttachmentCommand>
{
    public CreateAttachmentValidator()
    {
        RuleFor(x => x.Guid).NotEmpty();
        RuleFor(x => x.TaskId).GreaterThan(0);
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ActionTimeStamp).NotEmpty();
    }
}
