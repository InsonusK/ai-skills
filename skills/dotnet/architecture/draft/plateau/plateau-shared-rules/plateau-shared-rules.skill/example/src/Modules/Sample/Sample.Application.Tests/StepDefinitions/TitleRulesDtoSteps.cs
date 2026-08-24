using FluentValidation;
using Reqnroll;
using Sample.Application.Features.CreateTask;
using Sample.Application.Validators.Property;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;
using Xunit;

namespace Sample.Application.Tests.StepDefinitions;

[Binding, Scope(Feature = "Task title must be present and within length limits")]
public sealed class TitleRulesDtoSteps
{
    private readonly CreateTaskValidator _validator;
    private CreateTaskCommand _command = null!;
    private FluentValidation.Results.ValidationResult _result = null!;

    public TitleRulesDtoSteps()
    {
        _validator = new CreateTaskValidator(
            new EmailPropertyValidator(),
            new TitlePropertyValidator(),
            new SchedulePropertyValidator());
    }

    [Given(@"a task title of \"([^\"]*)\"")]
    public void GivenATaskTitleOf(string title)
    {
        _command = new CreateTaskCommand(
            new SoftTitle(title),
            1,
            new SoftEmail("test@example.com"));
    }

    [Given(@"a task title of (\d+) characters")]
    public void GivenATaskTitleOfCharacters(int length)
    {
        _command = new CreateTaskCommand(
            new SoftTitle(new string('x', length)),
            1,
            new SoftEmail("test@example.com"));
    }

    [When(@"the command validator checks the title")]
    public void WhenTheCommandValidatorChecksTheTitle() => _result = _validator.Validate(_command);

    [Then(@"the result is valid")]
    public void ThenTheResultIsValid() => Assert.True(_result.IsValid);

    [Then(@"the result is invalid with error code \"([^\"]*)\"")]
    public void ThenTheResultIsInvalidWithErrorCode(string errorCode) =>
        Assert.Contains(_result.Errors, e => e.ErrorCode == errorCode);
}
