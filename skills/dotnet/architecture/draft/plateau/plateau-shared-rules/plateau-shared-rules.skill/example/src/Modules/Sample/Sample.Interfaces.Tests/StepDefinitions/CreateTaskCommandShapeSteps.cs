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
    private DateTimeOffset _actionTimeStamp;
    private CreateTaskCommand? _command;

    [Given("a task title \"([^\"]*)\"")]
    public void GivenTitle(string title) => _title = title;

    [Given("an assignee id (\\d+)")]
    public void GivenAssigneeId(int id) => _assigneeId = id;

    [Given("an assignee email \"([^\"]*)\"")]
    public void GivenEmail(string email) => _assigneeEmail = new SoftEmail(email);

    [Given("an action timestamp \"([^\"]*)\"")]
    public void GivenActionTimeStamp(string timestamp) => _actionTimeStamp = DateTimeOffset.Parse(timestamp);

    [When("the CreateTaskCommand is created")]
    public void WhenCreated() => _command = new CreateTaskCommand(new SoftTitle(_title), _assigneeId, _assigneeEmail, _actionTimeStamp);

    [Then("it implements ICommand of CreateTaskResult")]
    public void ThenImplements()
        => Assert.IsAssignableFrom<ICommand<Result<CreateTaskResult>>>(_command);
}
