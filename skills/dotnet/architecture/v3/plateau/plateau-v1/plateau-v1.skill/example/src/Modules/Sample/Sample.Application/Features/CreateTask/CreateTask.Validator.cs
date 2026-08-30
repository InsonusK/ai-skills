using FluentValidation;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;

namespace Sample.Application.Features.CreateTask;

public class CreateTaskValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskValidator(
        IValidator<SoftEmail> emailValidator,
        IValidator<SoftTitle> titleValidator,
        IValidator<SoftSchedule> scheduleValidator)
    {
        RuleFor(x => x.Title).SetValidator(titleValidator);
        RuleFor(x => x.AssigneeId).GreaterThan(0);
        RuleFor(x => x.AssigneeEmail).SetValidator(emailValidator);
        RuleFor(x => x.ActionTimeStamp).NotEmpty();
        RuleFor(x => new SoftSchedule(x.StartDateTime, x.DueDateTime)).SetValidator(scheduleValidator);
    }
}
