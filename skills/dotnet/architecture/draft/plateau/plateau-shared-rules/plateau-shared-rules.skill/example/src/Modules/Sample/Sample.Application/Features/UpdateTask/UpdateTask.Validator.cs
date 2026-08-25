using FluentValidation;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;

namespace Sample.Application.Features.UpdateTask;

public class UpdateTaskValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskValidator(IValidator<SoftTitle> titleValidator)
    {
        RuleFor(x => x.TaskId).GreaterThan(0);
        RuleFor(x => x.Title).SetValidator(titleValidator);
        RuleFor(x => x.ActionTimeStamp).NotEmpty();
        RuleFor(x => x.Versions).NotEmpty();
    }
}
