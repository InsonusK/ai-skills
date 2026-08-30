using FluentValidation;
using Sample.Domain.Rules;
using Sample.Interfaces.ValueObjects;

namespace Sample.Application.Validators.Property;

public class SchedulePropertyValidator : AbstractValidator<SoftSchedule>
{
    public SchedulePropertyValidator() => RuleFor(x => x).ScheduleIsValid();
}
