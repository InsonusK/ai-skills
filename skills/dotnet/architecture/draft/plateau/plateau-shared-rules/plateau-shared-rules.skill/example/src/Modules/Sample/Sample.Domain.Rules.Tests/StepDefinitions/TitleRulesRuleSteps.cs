using FluentValidation.Results;
using Reqnroll;
using Sample.Domain.Rules;
using Sample.Interfaces.ValueObjects;
using Xunit;

namespace Sample.Domain.Rules.Tests.StepDefinitions;

[Binding, Scope(Feature = "Task title must be present and within length limits")]
public sealed class TitleRulesRuleSteps
{
    private SoftTitle _title = null!;
    private ValidationResult _result = null!;

    [Given(@"a title value of \"([^\"]*)\"")]
    public void GivenATitleValueOf(string value) => _title = new SoftTitle(value);

    [Given(@"a title value of (\d+) characters")]
    public void GivenATitleValueOfCharacters(int length) => _title = new SoftTitle(new string('x', length));

    [Given(@"a task title of \"([^\"]*)\"")]
    public void GivenATaskTitleOf(string title) => _title = new SoftTitle(title);

    [Given(@"a task title of (\d+) characters")]
    public void GivenATaskTitleOfCharacters(int length) => _title = new SoftTitle(new string('x', length));

    [When(@"TitleRules validates it")]
    public void WhenTitleRulesValidatesIt() => _result = _title.Check();

    [When(@"the command validator checks the title")]
    public void WhenTheCommandValidatorChecksTheTitle() => _result = _title.Check();

    [Then(@"the result is valid")]
    public void ThenTheResultIsValid() => Assert.True(_result.IsValid);

    [Then(@"the result is invalid with error code \"([^\"]*)\"")]
    public void ThenTheResultIsInvalidWithErrorCode(string errorCode) =>
        Assert.Contains(_result.Errors, e => e.ErrorCode == errorCode);
}
