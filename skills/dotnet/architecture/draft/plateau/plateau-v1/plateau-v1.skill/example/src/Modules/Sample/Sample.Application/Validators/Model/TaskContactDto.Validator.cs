using FluentValidation;
using Sample.Interfaces.DTOs;
using Sample.Interfaces.ValueObjects;

namespace Sample.Application.Validators.Model;

public class TaskContactDtoValidator : AbstractValidator<TaskContactDto>
{
    public TaskContactDtoValidator(
        IValidator<SoftTitle> titleValidator,
        IValidator<SoftEmail> emailValidator,
        IValidator<SoftSchedule> scheduleValidator)
    {
        RuleFor(x => x.Title).SetValidator(titleValidator);
        RuleFor(x => x.AssigneeEmail).SetValidator(emailValidator);
        RuleFor(dto => new SoftSchedule(dto.StartDateTime, dto.DueDateTime)).SetValidator(scheduleValidator);
    }
}
