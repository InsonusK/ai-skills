using Reqnroll;
using Sample.Domain.Entities;
using Sample.Domain.ValueObjects;
using Shared.Exceptions;
using Xunit;

namespace Sample.Domain.Tests.StepDefinitions;

[Binding]
public sealed class TaskItemSteps
{
    private TaskItem? _task;
    private string? _title;
    private Exception? _caught;

    [Given("a task with email \"([^\"]*)\"")]
    public void GivenTask(string email)
    {
        _caught = null;
        _task = new TaskItem(1, new Email(email));
    }

    [When("the title is updated to \"([^\"]*)\"")]
    public void WhenUpdateTitle(string title)
    {
        _caught = null;
        try
        {
            _task!.UpdateTitle(title);
            _title = title;
        }
        catch (Exception ex)
        {
            _caught = ex;
        }
    }

    [Then("the title is \"([^\"]*)\"")]
    public void ThenTitle(string expected) => Assert.Equal(expected, _task!.Title);

    [Then("a DomainException is thrown")]
    public void ThenThrows() => Assert.IsType<DomainException>(_caught);
}
