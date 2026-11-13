using Reqnroll;

namespace App.Host.Tests.Steps;

/// <summary>Sets environment variables for one scenario and unsets them after it.</summary>
[Binding]
public sealed class EnvironmentSteps
{
    private readonly List<string> _set = [];

    [Given("the environment variable {string} is {string}")]
    public void GivenTheEnvironmentVariableIs(string name, string value)
    {
        Environment.SetEnvironmentVariable(name, value);
        _set.Add(name);
    }

    [AfterScenario]
    public void UnsetEnvironmentVariables() => _set.ForEach(name => Environment.SetEnvironmentVariable(name, null));
}
