using FluentValidation.Results;
using Reqnroll;
using Sample.Domain.Rules;
using Sample.Interfaces.ValueObjects;
using Xunit;

namespace Sample.Domain.Rules.Tests.StepDefinitions;

[Binding, Scope(Feature = "Task schedule window must not be inverted")]
public sealed class ScheduleRulesRuleSteps
{
    private SoftSchedule _schedule = null!;
    private ValidationResult _result = null!;

    [Given(@"a schedule from \"([^\"]*)\" to \"([^\"]*)\"")]
    public void GivenAScheduleFromTo(string start, string due)
    {
        _schedule = new SoftSchedule(
            ParseDateOrNull(start),
            ParseDateOrNull(due));
    }

    [Given(@"a command with start date \"([^\"]*)\" and due date \"([^\"]*)\"")]
    public void GivenACommandWithStartDateAndDueDate(string start, string due)
    {
        _schedule = new SoftSchedule(
            ParseDateOrNull(start),
            ParseDateOrNull(due));
    }

    [When(@"ScheduleRules validates it")]
    public void WhenScheduleRulesValidatesIt() => _result = _schedule.Check();

    [Then(@"the result is valid")]
    public void ThenTheResultIsValid() => Assert.True(_result.IsValid);

    [Then(@"the result is invalid with error code \"([^\"]*)\"")]
    public void ThenTheResultIsInvalidWithErrorCode(string errorCode) =>
        Assert.Contains(_result.Errors, e => e.ErrorCode == errorCode);

    private static DateTimeOffset? ParseDateOrNull(string value)
        => string.IsNullOrWhiteSpace(value) ? null : DateTimeOffset.Parse(value);
}
