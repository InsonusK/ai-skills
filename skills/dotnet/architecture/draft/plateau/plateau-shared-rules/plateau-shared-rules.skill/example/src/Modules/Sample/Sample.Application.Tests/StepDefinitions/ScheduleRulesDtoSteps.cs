using FluentValidation.Results;
using Reqnroll;
using Sample.Application.Features.CreateTask;
using Sample.Application.Validators.Property;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;
using Xunit;

namespace Sample.Application.Tests.StepDefinitions;

[Binding, Scope(Feature = "Task schedule window must not be inverted")]
public sealed class ScheduleRulesDtoSteps
{
    private DateTimeOffset? _start;
    private DateTimeOffset? _due;
    private ValidationResult? _result;

    [Given(@"a command with start date "([^"]*)" and due date "([^"]*)"")]
    public void GivenACommandWithStartDateAndDueDate(string start, string due)
    {
        _start = ParseDateOrNull(start);
        _due = ParseDateOrNull(due);
    }

    [When(@"ScheduleRules validates it")]
    public void WhenScheduleRulesValidatesIt()
    {
        var validator = new CreateTaskValidator(
            new EmailPropertyValidator(),
            new TitlePropertyValidator(),
            new SchedulePropertyValidator());
        var command = new CreateTaskCommand(
            new SoftTitle("Review shared-rules example"),
            1,
            new SoftEmail("user@example.com"),
            DateTimeOffset.UtcNow,
            _start,
            _due);
        _result = validator.Validate(command);
    }

    [Then(@"the result is valid")]
    public void ThenTheResultIsValid() => Assert.True(_result!.IsValid);

    [Then(@"the result is invalid with error code "([^"]*)""]
    public void ThenTheResultIsInvalidWithErrorCode(string errorCode) =>
        Assert.Contains(_result!.Errors, e => e.ErrorCode == errorCode);

    private static DateTimeOffset? ParseDateOrNull(string value)
        => string.IsNullOrWhiteSpace(value) ? null : DateTimeOffset.Parse(value);
}
