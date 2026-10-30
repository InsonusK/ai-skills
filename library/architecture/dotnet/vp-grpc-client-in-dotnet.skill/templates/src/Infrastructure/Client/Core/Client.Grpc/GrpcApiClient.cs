// Source: skill vp-grpc-client-in-dotnet

using Client.Pipeline;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Logging;

namespace Client.Grpc;

/// <summary>One method of the contract an adapter calls.</summary>
/// <param name="Name">The full method name: "/todo.v1.ItemsService/PlanItems".</param>
/// <param name="Retryable">Marks a method safe to call again: a repeat does what one call does. Only such a method is repeated by the retry middleware.</param>
public sealed record GrpcMethod(string Name, bool Retryable = false);

/// <summary>
/// The connection to one gRPC API: the fixed part of every adapter of such an
/// API (Infrastructure/Client/{Api}), which builds the generated client of each
/// service over <see cref="CallInvoker"/>. Every unary call it makes passes
/// <see cref="CallInterceptor"/>.
/// </summary>
public sealed class GrpcApiClient : IDisposable
{
    private readonly GrpcChannel? _channel;

    /// <summary>
    /// Builds the connection to the API <paramref name="config"/> names. It
    /// does not connect: the first call does, so a start does not fail while
    /// the API is down. Mocked, every call is answered by the services
    /// <paramref name="emulator"/> binds, inside the process - the same
    /// interceptor, log, and outcomes, only no network - and a WARN record
    /// says so. It calls only <paramref name="methods"/>, each through
    /// <paramref name="middlewares"/>, the first the outermost.
    /// </summary>
    public GrpcApiClient(GrpcApiConfig config, ILogger logger, Action<ServiceBinderBase> emulator,
        IReadOnlyList<GrpcMethod> methods, IReadOnlyList<IClientMiddleware> middlewares)
    {
        CallInvoker invoker;
        if (config.Mocked)
        {
            invoker = new EmulatorCallInvoker(emulator);
            logger.Write(LogLevel.Warning, "api client mocked", LogAttr.Of("api", config.Name));
        }
        else
        {
            _channel = GrpcChannel.ForAddress(config.Target);
            invoker = _channel.CreateCallInvoker();
        }
        CallInvoker = invoker.Intercept(new CallInterceptor(config, logger, methods, middlewares));
    }

    /// <summary>The invoker the generated clients call through: <c>new ItemsService.ItemsServiceClient(client.CallInvoker)</c>.</summary>
    public CallInvoker CallInvoker { get; }

    /// <summary>Closes the channel.</summary>
    public void Dispose() => _channel?.Dispose();
}
