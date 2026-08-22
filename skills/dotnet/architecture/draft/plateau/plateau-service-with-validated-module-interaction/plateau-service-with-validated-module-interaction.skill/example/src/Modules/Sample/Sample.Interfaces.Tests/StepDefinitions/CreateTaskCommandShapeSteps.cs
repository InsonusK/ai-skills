using Ardalis.Result;
using Reqnroll;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;
using Shared;
using Xunit;

namespace Sample.Interfaces.Tests.StepDefinitions;

[Binding]
public sealed class CreateTaskCommandShapeSteps
{
    private string _title = string.Empty;
    private int _assigneeId;
    private SoftEmail _assigneeEmail = null!;
    private CreateTaskCommand? _command;

    [Given("a task title \"([^\"]*)\"")]
    public void GivenTitle(string title) => _title = title;

    [Given("an assignee id (\\d+)")]
    public void GivenAssigneeId(int id) => _assigneeId = id;

    [Given("an assignee email \"([^\"]*)\"")]
    public void GivenEmail(string email) => _assigneeEmail = new SoftEmail(email);

    [When("the CreateTaskCommand is created")]
    public void WhenCreated() => _command = new CreateTaskCommand(_title, _assigneeId, _assigneeEmail);

    [Then("it implements ICommand of CreateTaskResult")]
    public void ThenImplements()
        => Assert.IsAssignableFrom<ICommand<CreateTaskResult>>(_command);
}
