using Grpc.Core;
using Grpc.Reflection.V1Alpha;
using Reqnroll;
using Xunit;

namespace App.Host.Tests.Steps.Grpc;

[Binding]
public sealed class ReflectionSteps(GrpcWorld world)
{
    private List<string> _services = [];
    private RpcException? _error;

    [Given("gRPC reflection is switched on")]
    public void GivenGrpcReflectionIsSwitchedOn() => world.Config = world.Config with { ReflectionEnabled = true };

    [When("I list the gRPC services by reflection")]
    public async Task WhenIListTheGrpcServicesByReflection()
    {
        var client = new ServerReflection.ServerReflectionClient(await world.ChannelAsync());
        try
        {
            using var call = client.ServerReflectionInfo();
            await call.RequestStream.WriteAsync(new ServerReflectionRequest { ListServices = "" });
            await call.RequestStream.CompleteAsync();
            await foreach (var answer in call.ResponseStream.ReadAllAsync())
            {
                _services.AddRange(answer.ListServicesResponse.Service.Select(service => service.Name));
            }
        }
        catch (RpcException error)
        {
            _error = error;
        }
    }

    [Then("the listed gRPC services should include {string}")]
    public void ThenTheListedGrpcServicesShouldInclude(string service) => Assert.Contains(service, _services);

    [Then("the reflection call should fail with {word}")]
    public void ThenTheReflectionCallShouldFailWith(string code) => Assert.Equal(Enum.Parse<StatusCode>(code), _error?.StatusCode);
}
