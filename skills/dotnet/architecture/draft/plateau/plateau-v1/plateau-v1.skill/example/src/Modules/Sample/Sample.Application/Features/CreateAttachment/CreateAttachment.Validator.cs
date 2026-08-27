using FluentValidation;
using Sample.Application.Validators.Async;
using Sample.Interfaces.Commands;

namespace Sample.Application.Features.CreateAttachment;

public class CreateAttachmentValidator : AbstractValidator<CreateAttachmentCommand>
{
    public CreateAttachmentValidator(AttachmentTaskExistsCheck taskExistsCheck)
    {
        RuleFor(x => x.Guid).NotEmpty();
        RuleFor(x => x.TaskId).GreaterThan(0);
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ActionTimeStamp).NotEmpty();

        RuleFor(x => x).CustomAsync(taskExistsCheck.CheckAsync);
    }
}
