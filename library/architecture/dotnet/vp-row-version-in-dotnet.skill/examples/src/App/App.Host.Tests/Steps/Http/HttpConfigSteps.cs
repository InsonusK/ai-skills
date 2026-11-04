using App.Host.Api.Http.Pipeline;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Http;

[Binding]
public sealed class HttpConfigSteps(ITestOutputHelper output)
{
    private HttpConfig? _config;
    private FormatException? _error;

    [When("the HTTP settings are loaded")]
    public void WhenTheHttpSettingsAreLoaded()
    {
        try
        {
            _config = HttpConfig.Load();
        }
        catch (FormatException error)
        {
            _error = error;
        }
        output.WriteLine($"when: HttpConfig.Load() -> {_error?.Message ?? _config!.ToString()}");
    }

    [Then("the HTTP listen port should be {int}")]
    public void ThenTheHttpListenPortShouldBe(int port) => Assert.Equal(port, _config!.ListenPort);

    [Then("the HTTP base path should be {string}")]
    public void ThenTheHttpBasePathShouldBe(string basePath) => Assert.Equal(basePath, _config!.BasePath);

    [Then("the HTTP docs should be enabled")]
    public void ThenTheHttpDocsShouldBeEnabled() => Assert.True(_config!.DocsEnabled);

    [Then("the HTTP docs should be disabled")]
    public void ThenTheHttpDocsShouldBeDisabled() => Assert.False(_config!.DocsEnabled);

    [Then("loading the HTTP settings should fail mentioning {string}")]
    public void ThenLoadingTheHttpSettingsShouldFailMentioning(string text) => Assert.Contains(text, _error?.Message);
}
