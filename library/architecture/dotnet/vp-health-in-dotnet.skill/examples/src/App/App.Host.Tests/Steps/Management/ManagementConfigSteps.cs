using App.Host.Api.Management;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Management;

[Binding]
public sealed class ManagementConfigSteps(ITestOutputHelper output)
{
    private ManagementConfig? _config;
    private FormatException? _error;

    [When("the management settings are loaded")]
    public void WhenTheManagementSettingsAreLoaded()
    {
        try
        {
            _config = ManagementConfig.Load();
        }
        catch (FormatException error)
        {
            _error = error;
        }
        output.WriteLine($"when: ManagementConfig.Load() -> {_error?.Message ?? _config!.ToString()}");
    }

    [Then("the management listen port should be {int}")]
    public void ThenTheManagementListenPortShouldBe(int port) => Assert.Equal(port, _config!.ListenPort);

    [Then("loading the management settings should fail mentioning {string}")]
    public void ThenLoadingTheManagementSettingsShouldFailMentioning(string text) => Assert.Contains(text, _error?.Message);
}
