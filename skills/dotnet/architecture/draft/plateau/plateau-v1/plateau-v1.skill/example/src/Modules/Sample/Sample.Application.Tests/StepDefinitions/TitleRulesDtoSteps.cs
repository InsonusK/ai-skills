using FluentValidation.Results;
using Reqnroll;
using Sample.Application.Features.UpdateTask;
using Sample.Application.Validators.Property;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;
using Xunit;

namespace Sample.Application.Tests.StepDefinitions;

[Binding, Scope(Feature = "Task title must be present and within length limits")]
public sealed class TitleRulesDtoSteps
{
    private string _title = string.Empty;
    private ValidationResult? _result;

    [Given(@"a task title of "([^"]*)")"]
    public void GivenATaskTitleOf(string title) => _title = title;

    [Given(@"a task title of (\d+) characters")]
    public void GivenATaskTitleOfCharacters(int length) => _title = new string('x', length);

    [When(@"the command validator checks the title")]
    public void WhenTheCommandValidatorChecksTheTitle()
    {
        var validator = new UpdateTaskValidator(new TitlePropertyValidator());
        var command = new UpdateTaskCommand(
            1,
            new SoftTitle(_title),
            DateTimeOffset.UtcNow,
            new Dictionary<string, uint> { ["Task"] = 1 }.AsReadOnly());
        _result = validator.Validate(command);
    }

    [Then(@"the result is valid")]
    public void ThenTheResultIsValid() => Assert.True(_result!.IsValid);

    [Then(@"the result is invalid with error code "([^"]*)""]
    public void ThenTheResultIsInvalidWithErrorCode(string errorCode) =>
        Assert.Contains(_result!.Errors, e => e.ErrorCode == errorCode);
}
