using System.Globalization;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace Client.Http.Tests.Steps;

[Binding]
public sealed class ConfigSteps(ITestOutputHelper output)
{
    // The settings of the test API a scenario may set; each starts and ends with none.
    private static readonly string[] Variables = ["TEST_API_HOST", "TEST_API_HTTP_PORT", "TEST_API_TLS", "TEST_API_TIMEOUT"];

    private HttpApiConfig? _loaded;
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

    [When("the settings of {string} are loaded")]
    public void WhenTheSettingsAreLoaded(string prefix)
    {
        try
        {
            _loaded = HttpApiConfig.Load(prefix);
        }
        catch (FormatException error)
        {
            _error = error;
        }
        output.WriteLine($"when: Load({prefix}) -> {_loaded}, error {_error?.Message}");
    }

    [Then("the base URL should be {string} and the timeout {string}")]
    public void ThenTheBaseUrlShouldBe(string baseUrl, string timeout)
    {
        Assert.Null(_error);
        var number = double.Parse(timeout.TrimEnd('m', 's'), CultureInfo.InvariantCulture);
        var expected = timeout.EndsWith("ms") ? TimeSpan.FromMilliseconds(number) : TimeSpan.FromSeconds(number);
        Assert.Equal((new Uri(baseUrl), expected), (_loaded!.BaseUri, _loaded.Timeout));
    }

    [Then("loading should fail mentioning {string}")]
    public void ThenLoadingShouldFailMentioning(string text) => Assert.Contains(text, Assert.IsType<FormatException>(_error).Message);
}
