using App.Host.Api.Grpc.Pipeline;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Grpc;

[Binding]
public sealed class GrpcConfigSteps(ITestOutputHelper output)
{
    private GrpcConfig? _config;
    private FormatException? _error;

    [When("the gRPC settings are loaded")]
    public void WhenTheGrpcSettingsAreLoaded()
    {
        try
        {
            _config = GrpcConfig.Load();
        }
        catch (FormatException error)
        {
            _error = error;
        }
        output.WriteLine($"when: GrpcConfig.Load() -> {_error?.Message ?? _config!.ToString()}");
    }

    [Then("the gRPC listen port should be {int}")]
    public void ThenTheGrpcListenPortShouldBe(int port) => Assert.Equal(port, _config!.ListenPort);

    [Then("gRPC reflection should be enabled")]
    public void ThenGrpcReflectionShouldBeEnabled() => Assert.True(_config!.ReflectionEnabled);

    [Then("gRPC reflection should be disabled")]
    public void ThenGrpcReflectionShouldBeDisabled() => Assert.False(_config!.ReflectionEnabled);

    [Then("loading the gRPC settings should fail mentioning {string}")]
    public void ThenLoadingTheGrpcSettingsShouldFailMentioning(string text) => Assert.Contains(text, _error?.Message);
}
