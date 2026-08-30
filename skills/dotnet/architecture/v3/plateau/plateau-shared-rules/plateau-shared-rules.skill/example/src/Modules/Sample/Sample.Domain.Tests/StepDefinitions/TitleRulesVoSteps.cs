using Reqnroll;
using Sample.Domain.Entities;
using Sample.Domain.ValueObjects;
using Shared.Exceptions;
using Xunit;

namespace Sample.Domain.Tests.StepDefinitions;

[Binding, Scope(Feature = "Task title must be present and within length limits")]
public sealed class TitleRulesVoSteps
{
    private string _titleValue = string.Empty;
    private DomainException? _thrown;

    [Given(@"a title value of \"([^\"]*)\"")]
    public void GivenATitleValueOf(string value) => _titleValue = value;

    [Given(@"a title value of (\d+) characters")]
    public void GivenATitleValueOfCharacters(int length) => _titleValue = new string('x', length);

    [When(@"TitleRules validates it")]
    public void WhenTitleRulesValidatesIt()
    {
        var task = new TaskItem(1, new Email("test@example.com"));
        _thrown = Record.Exception(() => task.UpdateTitle(_titleValue)) as DomainException;
    }

    [Then(@"the result is valid")]
    public void ThenTheResultIsValid() => Assert.Null(_thrown);

    [Then(@"the result is invalid with error code \"([^\"]*)\"")]
    public void ThenTheResultIsInvalidWithErrorCode(string errorCode)
    {
        Assert.NotNull(_thrown);
        Assert.Equal(errorCode, _thrown!.Code);
    }
}
