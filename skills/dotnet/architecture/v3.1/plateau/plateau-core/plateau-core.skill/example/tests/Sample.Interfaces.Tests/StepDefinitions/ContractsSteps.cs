using Ardalis.Result;
using Reqnroll;
using Sample.Interfaces.Commands;
using Sample.Interfaces.Events;
using Shared.MediatR;
using Xunit;

namespace Sample.Interfaces.Tests.StepDefinitions;

[Binding]
public sealed class ContractsSteps
{
    private GreetCommand? _command;

    [When("a GreetCommand is created with message {string}")]
    public void WhenCommandCreated(string message) => _command = new GreetCommand(message);

    [Then("it implements ICommand of Result of GreetResult")]
    public void ThenCommandShape() =>
        Assert.IsAssignableFrom<ICommand<Result<GreetResult>>>(_command);

    [Then("Greeted implements INotificationEvent")]
    public void ThenGreetedShape() =>
        Assert.True(typeof(INotificationEvent).IsAssignableFrom(typeof(Greeted)));
}
