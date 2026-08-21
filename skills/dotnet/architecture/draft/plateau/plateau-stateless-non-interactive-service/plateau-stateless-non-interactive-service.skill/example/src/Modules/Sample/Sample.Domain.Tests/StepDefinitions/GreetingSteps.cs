using Reqnroll;
using Sample.Domain.Entities;

namespace Sample.Domain.Tests.StepDefinitions;

[Binding]
public sealed class GreetingSteps
{
    private string _name = string.Empty;
    private string? _result;
    private Exception? _caught;

    [Given("the name \"([^\"]*)\")]
    public void GivenName(string name) => _name = name;

    [When("the greeting is produced")]
    public void WhenProduced()
    {
        _caught = null;
        try
        {
            _result = Greeting.For(_name);
        }
        catch (Exception ex)
        {
            _caught = ex;
        }
    }

    [Then("the result is \"([^\"]*)\")]
    public void ThenResult(string expected) => Assert.Equal(expected, _result);

    [Then("an argument exception is thrown")]
    public void ThenThrows() => Assert.IsType<ArgumentException>(_caught);
}
