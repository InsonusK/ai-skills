using App.Host.Api.Grpc.Pipeline;
using BuildingBlocks.Logging;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Shared.Errors;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Grpc;

/// <summary>An interceptor that writes the record "interceptor {name}" before it passes the call on.</summary>
public sealed class NamedInterceptor(string name, ILogger logger) : Interceptor
{
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        logger.Write(LogLevel.Information, $"interceptor {name}");
        return continuation(request, context);
    }
}

/// <summary>An interceptor that rejects every call, as an authentication interceptor rejects one without credentials.</summary>
public sealed class RejectingInterceptor(string message) : Interceptor
{
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation) =>
        throw new ErrorKindException(ErrorKind.Unauthenticated, message);
}

[Binding]
public sealed class InterceptorSteps(GrpcWorld world, ITestOutputHelper output)
{
    private Exception? _error;

    [Given("the gRPC interceptors {string} and {string}, each logging a record with its name")]
    public void GivenTheInterceptorsLogging(string outer, string inner)
    {
        var logger = world.Logs.Factory.CreateLogger("test");
        world.Interceptors.Add(new NamedInterceptor(outer, logger));
        world.Interceptors.Add(new NamedInterceptor(inner, logger));
    }

    [Given("a gRPC interceptor that rejects every call as unauthenticated with {string}")]
    public void GivenARejectingInterceptor(string message) => world.Interceptors.Add(new RejectingInterceptor(message));

    [When("the type {string} is built as a gRPC interceptor")]
    public void WhenTheTypeIsBuilt(string name)
    {
        _error = Record.Exception(() => GrpcServer.Activate(new ServiceCollection().BuildServiceProvider(), [Type.GetType($"System.{name}")!]));
        output.WriteLine($"when: activate {name} -> {_error?.Message}");
    }

    [Then("building the gRPC interceptors should fail mentioning {string}")]
    public void ThenBuildingShouldFail(string text) => Assert.Contains(text, Assert.IsType<ArgumentException>(_error).Message);
}
