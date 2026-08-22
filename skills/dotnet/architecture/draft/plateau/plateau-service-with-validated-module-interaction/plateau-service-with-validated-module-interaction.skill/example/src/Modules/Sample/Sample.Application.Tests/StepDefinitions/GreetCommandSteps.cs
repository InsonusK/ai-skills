using Reqnroll;
using Sample.Application.Handlers;
using Sample.Interfaces.Commands;

namespace Sample.Application.Tests.StepDefinitions;

[Binding]
public sealed class GreetCommandSteps
{
    private string _name = string.Empty;
    private string? _result;

    [Given("the name \"([^\"]*)\")]
    public void GivenName(string name) => _name = name;

    [When("the greet command is handled")]
    public async Task WhenHandled()
    {
        var handler = new GreetCommandHandler();
        var response = await handler.Handle(new GreetCommand(_name), CancellationToken.None);
        _result = response.Value;
    }

    [Then("the result value is \"([^\"]*)\")]
    public void ThenValue(string expected) => Assert.Equal(expected, _result);
}
