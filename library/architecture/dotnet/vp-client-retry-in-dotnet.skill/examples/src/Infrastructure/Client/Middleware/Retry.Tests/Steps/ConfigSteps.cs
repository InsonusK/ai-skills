using Reqnroll;
using Xunit;

namespace Retry.Tests.Steps;

[Binding]
public sealed class ConfigSteps
{
    // The settings a scenario may set; each starts and ends with none.
    private static readonly string[] Variables = ["TEST_API_RETRY_ATTEMPTS", "TEST_API_RETRY_BACKOFF"];

    private RetryConfig? _loaded;
    private FormatException? _error;

    [BeforeScenario]
    [AfterScenario]
    public static void UnsetVariables()
    {
        foreach (var name in Variables)
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [Given("the environment variable {string} is {string}")]
    public void GivenTheEnvironmentVariableIs(string name, string value) => Environment.SetEnvironmentVariable(name, value);

    [When("the retry settings of {string} are loaded")]
    public void WhenTheRetrySettingsAreLoaded(string prefix)
    {
        try
        {
            _loaded = RetryConfig.Load(prefix);
        }
        catch (FormatException error)
        {
            _error = error;
        }
    }

    [Then("the retry should allow {int} attempts with a backoff of {string}")]
    public void ThenTheRetryShouldAllow(int attempts, string backoff)
    {
        Assert.Null(_error);
        Assert.Equal(new RetryConfig(attempts, TimeSpan.FromMilliseconds(int.Parse(backoff[..^2]))), _loaded);
    }

    [Then("loading should fail mentioning {string}")]
    public void ThenLoadingShouldFailMentioning(string text) => Assert.Contains(text, Assert.IsType<FormatException>(_error).Message);
}
