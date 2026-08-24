using FluentValidation;
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
    private readonly CreateTaskValidator _validator;
    private CreateTaskCommand _command = null!;
    private FluentValidation.Results.ValidationResult _result = null!;

    public ScheduleRulesDtoSteps()
    {
        _validator = new CreateTaskValidator(
            new EmailPropertyValidator(),
            new TitlePropertyValidator(),
            new SchedulePropertyValidator());
    }

    [Given(@"a command with start date \"([^\"]*)\" and due date \"([^\"]*)\"")]
    public void GivenACommandWithStartDateAndDueDate(string start, string due)
    {
        _command = new CreateTaskCommand(
            new SoftTitle("Review shared-rules example"),
            1,
            new SoftEmail("test@example.com"),
            ParseDateOrNull(start),
            ParseDateOrNull(due));
    }

    [When(@"ScheduleRules validates it")]
    public void WhenScheduleRulesValidatesIt() => _result = _validator.Validate(_command);

    [Then(@"the result is valid")]
    public void ThenTheResultIsValid() => Assert.True(_result.IsValid);

    [Then(@"the result is invalid with error code \"([^\"]*)\"")]
    public void ThenTheResultIsInvalidWithErrorCode(string errorCode) =>
        Assert.Contains(_result.Errors, e => e.ErrorCode == errorCode);

    private static DateTimeOffset? ParseDateOrNull(string value)
        => string.IsNullOrWhiteSpace(value) ? null : DateTimeOffset.Parse(value);
}
