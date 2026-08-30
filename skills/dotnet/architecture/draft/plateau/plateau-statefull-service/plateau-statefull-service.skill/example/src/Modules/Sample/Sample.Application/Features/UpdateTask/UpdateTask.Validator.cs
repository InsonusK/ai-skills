using FluentValidation;
using Sample.Interfaces.Commands;

namespace Sample.Application.Features.UpdateTask;

public class UpdateTaskValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.TaskId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ActionTimeStamp).NotEmpty();
        RuleFor(x => x.Versions).NotEmpty();
    }
}
