using Reqnroll;
using Sample.Domain.Entities;
using Sample.Domain.ValueObjects;
using Shared.Exceptions;
using Xunit;

namespace Sample.Domain.Tests.StepDefinitions;

[Binding, Scope(Feature = "Task schedule window must not be inverted")]
public sealed class ScheduleRulesVoSteps
{
    private DateTimeOffset? _start;
    private DateTimeOffset? _due;
    private DomainException? _thrown;

    [Given(@"a schedule from \"([^\"]*)\" to \"([^\"]*)\"")]
    public void GivenAScheduleFromTo(string start, string due)
    {
        _start = ParseDateOrNull(start);
        _due = ParseDateOrNull(due);
    }

    [Given(@"a command with start date \"([^\"]*)\" and due date \"([^\"]*)\"")]
    public void GivenACommandWithStartDateAndDueDate(string start, string due)
    {
        _start = ParseDateOrNull(start);
        _due = ParseDateOrNull(due);
    }

    [When(@"ScheduleRules validates it")]
    public void WhenScheduleRulesValidatesIt()
    {
        var task = new TaskItem(1, new Email("test@example.com"));
        _thrown = Record.Exception(() => task.UpdateSchedule(_start, _due)) as DomainException;
    }

    [Then(@"the result is valid")]
    public void ThenTheResultIsValid() => Assert.Null(_thrown);

    [Then(@"the result is invalid with error code \"([^\"]*)\"")]
    public void ThenTheResultIsInvalidWithErrorCode(string errorCode)
    {
        Assert.NotNull(_thrown);
        Assert.Equal(errorCode, _thrown!.Code);
    }

    private static DateTimeOffset? ParseDateOrNull(string value)
        => string.IsNullOrWhiteSpace(value) ? null : DateTimeOffset.Parse(value);
}
