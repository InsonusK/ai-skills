using System.Reflection;
using Contracts.Todo.V1;
using Google.Protobuf.Reflection;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Grpc;

/// <summary>The services the application serves held against the services of its proto contract.</summary>
[Binding]
public sealed class GrpcContractSteps(GrpcWorld world, ITestOutputHelper output)
{
    // The descriptor of the service a class implements: the generated base
    // (ItemsServiceBase) is nested in the static class (ItemsService) that has it.
    private static ServiceDescriptor DescriptorOf(Type implementation) =>
        (ServiceDescriptor)implementation.BaseType!.DeclaringType!.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;

    private IReadOnlyList<Type> Served()
    {
        ItemsServiceSteps.ServeApplicationServices(world);
        return world.BuildServices().Select(service => service.Type).ToList();
    }

    [Then("the gRPC API and its proto contract both have exactly the services:")]
    public void ThenTheGrpcApiAndItsContractHaveExactlyTheServices(DataTable table)
    {
        var listed = table.Rows.Select(row => row["service"]).Order().ToList();
        var contract = TodoReflection.Descriptor.Services.Select(service => service.FullName).Order().ToList();
        var served = Served().Select(type => DescriptorOf(type).FullName).Order().ToList();
        output.WriteLine($"then: contract [{string.Join(", ", contract)}], served [{string.Join(", ", served)}]");
        Assert.Equal(listed, contract);
        Assert.Equal(listed, served);
    }

    // A method left to the generated base answers UNIMPLEMENTED at run time;
    // the base's method is virtual, so the compiler does not catch it.
    [Then("every method of the proto contract should be implemented by its service")]
    public void ThenEveryMethodShouldBeImplemented()
    {
        var missing = Served()
            .SelectMany(type => DescriptorOf(type).Methods
                .Where(method => type.GetMethod(method.Name)?.DeclaringType != type)
                .Select(method => $"{type.Name}.{method.Name}"))
            .ToList();
        Assert.Empty(missing);
    }
}
