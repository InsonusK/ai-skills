using Microsoft.Extensions.Logging;
using Reqnroll;
using Xunit;

namespace MaskedLogging.Tests.Steps;

/// <summary>The steps of settings.feature: <see cref="MaskedLogConfig.Load"/> over the environment.</summary>
[Binding]
public sealed class SettingsSteps
{
    private static readonly string[] Variables = ["LOG_PAYLOAD", "LOG_PAYLOAD_MAX_BYTES", "LOG_PAYLOAD_UNMASKED"];

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
    public void WhenTheSettingsAreLoaded() => Load(LogLevel.Information);

    [When("the logging settings are loaded for the level {string}")]
    public void WhenTheSettingsAreLoadedForTheLevel(string level) => Load(level switch
    {
        "DEBUG" => LogLevel.Debug,
        "INFO" => LogLevel.Information,
        "WARN" => LogLevel.Warning,
        _ => LogLevel.Error,
    });

    [Then("masking should be {word}")]
    public void ThenMaskingShouldBe(string state)
    {
        Assert.Null(_error);
        Assert.Equal(state == "off", _config!.Unmasked);
    }

    private void Load(LogLevel level)
    {
        try
        {
            _config = MaskedLogConfig.Load(level);
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
