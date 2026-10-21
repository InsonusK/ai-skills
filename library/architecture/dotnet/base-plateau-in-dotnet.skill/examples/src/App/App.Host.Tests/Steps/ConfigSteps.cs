using App.Host.Config;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps;

[Binding]
public sealed class ConfigSteps(ITestOutputHelper output)
{
    private AppConfig? _config;
    private FormatException? _error;

    [When("the configuration is loaded")]
    public void WhenTheConfigurationIsLoaded()
    {
        try
        {
            _config = AppConfig.Load();
        }
        catch (FormatException error)
        {
            _error = error;
        }
        output.WriteLine($"when: Load() -> {_error?.Message ?? _config!.ToString()}");
    }

    [Then("the log level should be {string}")]
    public void ThenTheLogLevelShouldBe(string level) => Assert.Equal(level, _config?.LogLevel.ToString());

    [Then("loading should fail mentioning {string}")]
    public void ThenLoadingShouldFailMentioning(string text) => Assert.Contains(text, _error?.Message);
}
