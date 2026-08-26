using Ardalis.Result;
using Reqnroll;
using Sample.Interfaces.Commands;
using Shared;

namespace Sample.Interfaces.Tests.StepDefinitions;

[Binding]
public sealed class GreetCommandShapeSteps
{
    private GreetCommand? _command;

    [Given("a greet command with name \"([^\"]*)\")]
    public void GivenCommand(string name) => _command = new GreetCommand(name);

    [Then("it implements ICommand of string")]
    public void ThenImplements() => Assert.IsAssignableFrom<ICommand<Result<string>>>(_command);
}
