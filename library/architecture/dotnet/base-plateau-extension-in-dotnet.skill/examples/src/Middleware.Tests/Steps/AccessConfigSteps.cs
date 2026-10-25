using Middleware.Access;
using Reqnroll;
using Xunit;

namespace Middleware.Tests.Steps;

/// <summary>The steps of AccessConfig.feature: <see cref="AccessConfig.Load"/> over the environment.</summary>
[Binding]
public sealed class AccessConfigSteps
{
    private static readonly string[] Variables = ["ACCESS_TOKENS", "ACCESS_MAX_BATCH"];

    private AccessConfig? _config;
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

    [When("the access settings are loaded")]
    public void WhenTheSettingsAreLoaded()
    {
        try
        {
            _config = AccessConfig.Load();
        }
        catch (FormatException error)
        {
            _error = error;
        }
    }

    [Then("the token {string} should name {string} with the roles {string}")]
    public void ThenTheTokenShouldName(string token, string user, string roles)
    {
        var caller = Assert.Contains(token, _config!.Tokens);
        Assert.Equal((user, roles), (caller.User, string.Join(',', caller.Roles)));
    }

    [Then("the batch limit should be {int}")]
    public void ThenTheBatchLimitShouldBe(int limit) => Assert.Equal(limit, _config!.MaxBatch);

    [Then("loading should fail mentioning {string}")]
    public void ThenLoadingShouldFail(string text) => Assert.Contains(text, Assert.IsType<FormatException>(_error).Message);
}
