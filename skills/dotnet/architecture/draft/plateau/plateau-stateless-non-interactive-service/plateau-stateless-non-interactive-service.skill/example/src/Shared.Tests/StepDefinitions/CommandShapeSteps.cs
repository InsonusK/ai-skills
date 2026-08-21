using Reqnroll;
using Shared;

namespace Shared.Tests.StepDefinitions;

[Binding]
public sealed class CommandShapeSteps
{
    private string _name = string.Empty;
    private ICommand<string>? _command;

    [Given("a command name \"([^\"]*)\")]
    public void GivenName(string name) => _name = name;

    [When("the command is created")]
    public void WhenCreated() => _command = new SampleCommand(_name);

    [Then("it implements ICommand of string")]
    public void ThenImplements() => Assert.IsAssignableFrom<ICommand<string>>(_command);

    private record SampleCommand(string Name) : ICommand<string>;
}
