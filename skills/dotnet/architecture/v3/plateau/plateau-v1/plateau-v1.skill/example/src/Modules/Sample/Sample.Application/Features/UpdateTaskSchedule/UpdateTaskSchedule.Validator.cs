using FluentValidation;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;

namespace Sample.Application.Features.UpdateTaskSchedule;

public class UpdateTaskScheduleValidator : AbstractValidator<UpdateTaskScheduleCommand>
{
    public UpdateTaskScheduleValidator(IValidator<SoftSchedule> scheduleValidator)
    {
        RuleFor(x => x.ActionTimeStamp).NotEmpty();
        RuleFor(x => new SoftSchedule(x.StartDateTime, x.DueDateTime)).SetValidator(scheduleValidator);
    }
}
