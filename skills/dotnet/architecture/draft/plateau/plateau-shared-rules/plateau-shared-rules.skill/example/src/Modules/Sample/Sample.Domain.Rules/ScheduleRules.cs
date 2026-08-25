using FluentValidation;
using FluentValidation.Results;
using Sample.Domain.Rules.Common;
using Sample.Interfaces.ValueObjects;

namespace Sample.Domain.Rules;

public static class ScheduleRules
{
    public const string WindowInvertedCode = ModuleInfo.ModuleName + ".Schedule.WindowInverted";

    public static bool IsValid(this SoftSchedule schedule)
        => schedule.StartDateTime is null || schedule.DueDateTime is null || schedule.DueDateTime >= schedule.StartDateTime;

    public static IRuleBuilderOptions<T, SoftSchedule> ScheduleIsValid<T>(this IRuleBuilder<T, SoftSchedule> rule)
        => rule.Must(x => x.IsValid())
               .WithErrorCode(WindowInvertedCode)
               .WithMessage("Due date must not be earlier than start date.")
               .WithState((_, x) => new { x.StartDateTime, x.DueDateTime });

    private static readonly InlineValidator<SoftSchedule> _validator = new();
    static ScheduleRules() => _validator.RuleFor(x => x).ScheduleIsValid();
    public static ValidationResult Check(this SoftSchedule schedule) => _validator.Validate(schedule);
}
