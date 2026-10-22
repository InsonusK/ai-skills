using Reqnroll;
using Xunit;

namespace MaskedLogging.Tests.Steps;

/// <summary>The steps of settings.feature: <see cref="MaskedLogConfig.Load"/> over the environment.</summary>
[Binding]
public sealed class SettingsSteps
{
    private static readonly string[] Variables = ["LOG_PAYLOAD", "LOG_PAYLOAD_MAX_BYTES"];

    private MaskedLogConfig? _config;
    private FormatException? _error;

    [BeforeScenario]
    [AfterScenario]
    public void ClearTheVariables()
    {
        foreach (var name in Variables)
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [Given("the environment variable {string} is {string}")]
    public void GivenTheEnvironmentVariable(string name, string value) => Environment.SetEnvironmentVariable(name, value);

    [When("the logging settings are loaded")]
    public void WhenTheSettingsAreLoaded()
    {
        try
        {
            _config = MaskedLogConfig.Load();
        }
        catch (FormatException error)
        {
            _error = error;
        }
    }

    [Then("LOG_PAYLOAD should be {string} and the limit {int}")]
    public void ThenTheSettingsShouldBe(string mode, int limit)
    {
        Assert.Null(_error);
        Assert.Equal(new MaskedLogConfig(mode, limit), _config);
    }

    [Then("loading should fail mentioning {string}")]
    public void ThenLoadingShouldFail(string text) => Assert.Contains(text, Assert.IsType<FormatException>(_error).Message);
}
