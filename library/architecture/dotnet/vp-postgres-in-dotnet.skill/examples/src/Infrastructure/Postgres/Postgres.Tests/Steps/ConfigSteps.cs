using Reqnroll;
using Xunit;

namespace Postgres.Tests.Steps;

[Binding]
public sealed class ConfigSteps
{
    private readonly List<string> _set = [];
    private PostgresConfig? _config;
    private Exception? _error;

    [Given("the environment variable {string} is {string}")]
    public void GivenTheEnvironmentVariableIs(string name, string value)
    {
        Environment.SetEnvironmentVariable(name, value);
        _set.Add(name);
    }

    [When("the settings are loaded")]
    public void WhenTheSettingsAreLoaded()
    {
        try
        {
            _config = PostgresConfig.Load();
        }
        catch (FormatException error)
        {
            _error = error;
        }
    }

    [Then("loading should fail mentioning {string}")]
    public void ThenLoadingShouldFailMentioning(string text) => Assert.Contains(text, _error!.Message);

    [Then("the failure should not mention {string}")]
    public void ThenTheFailureShouldNotMention(string text) => Assert.DoesNotContain(text, _error!.Message);

    [Then("the DSN should be {string}")]
    public void ThenTheDsnShouldBe(string dsn) => Assert.Equal(dsn, _config!.Dsn);

    [Then("the application should migrate on start: {string}")]
    public void ThenTheApplicationShouldMigrateOnStart(string expected) =>
        Assert.Equal(bool.Parse(expected), _config!.MigrateOnStart);

    [Then("the settings printed should not mention {string}")]
    public void ThenTheSettingsPrintedShouldNotMention(string text) => Assert.DoesNotContain(text, _config!.ToString());

    [BeforeScenario]
    public void ClearTheEnvironment()
    {
        Environment.SetEnvironmentVariable("DATABASE_DSN", null);
        Environment.SetEnvironmentVariable("MIGRATE_ON_START", null);
    }

    [AfterScenario]
    public void UnsetEnvironmentVariables() => _set.ForEach(name => Environment.SetEnvironmentVariable(name, null));
}
