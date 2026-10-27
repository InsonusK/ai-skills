// Source: skill vp-grpc-client-in-dotnet

using Google.Protobuf;
using Grpc.Core;

namespace Client.Grpc;

/// <summary>
/// The transport of a mocked client: it answers every unary call with the
/// emulator's implementation of the generated service, inside the process.
/// Request and response pass through their wire form, as over the network, so
/// the emulator sees the message the API would and the caller gets a message
/// of its own, never the emulator's object; the emulator's
/// RpcException reaches the interceptor as the API's status would, and a call
/// past its deadline fails with DEADLINE_EXCEEDED.
/// </summary>
internal sealed class EmulatorCallInvoker : CallInvoker
{
    // Each unary method the emulator binds, by its full name: "/todo.v1.ItemsService/PlanItems".
    private readonly Dictionary<string, Delegate> _methods = [];

    public EmulatorCallInvoker(Action<ServiceBinderBase> emulator) => emulator(new Binder(_methods));

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
    {
        var status = Status.DefaultSuccess;
        var answer = Serve(method, options, request).ContinueWith(served =>
        {
            if (served.Exception?.InnerException is RpcException failed)
            {
                status = failed.Status;
            }
            return served;
        }, TaskScheduler.Default).Unwrap();
        return new AsyncUnaryCall<TResponse>(answer, Task.FromResult(new Metadata()), () => status, () => [], () => { });
    }

    private async Task<TResponse> Serve<TRequest, TResponse>(Method<TRequest, TResponse> method, CallOptions options, TRequest request)
        where TRequest : class
        where TResponse : class
    {
        if (!_methods.TryGetValue(method.FullName, out var bound))
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, $"the emulator does not answer {method.FullName}"));
        }
        var handler = (UnaryServerMethod<TRequest, TResponse>)bound;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(options.CancellationToken);
        if (options.Deadline is { } at && at != DateTime.MaxValue)
        {
            deadline.CancelAfter(at - DateTime.UtcNow is var left && left > TimeSpan.Zero ? left : TimeSpan.Zero);
        }
        var received = OverTheWire(request);
        var serving = handler(received, new EmulatorCallContext(method.FullName, options, deadline.Token));
        var finished = await Task.WhenAny(serving, Task.Delay(Timeout.Infinite, deadline.Token));
        if (finished != serving)
        {
            throw new RpcException(options.CancellationToken.IsCancellationRequested
                ? new Status(StatusCode.Cancelled, "the call was cancelled")
                : new Status(StatusCode.DeadlineExceeded, "the emulator did not answer within the deadline"));
        }
        var response = await serving;
        return OverTheWire(response);
    }

    /// <summary>The message as the other side reads it: written to bytes and parsed again.</summary>
    private static T OverTheWire<T>(T message) where T : class =>
        message is IMessage protobuf ? (T)protobuf.Descriptor.Parser.ParseFrom(protobuf.ToByteString()) : message;

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request) =>
        AsyncUnaryCall(method, host, options, request).ResponseAsync.GetAwaiter().GetResult();

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request) =>
        throw new NotSupportedException("a mocked client answers unary calls only");

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options) =>
        throw new NotSupportedException("a mocked client answers unary calls only");

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options) =>
        throw new NotSupportedException("a mocked client answers unary calls only");

    /// <summary>Collects the unary methods of the services the emulator binds, as a server would.</summary>
    private sealed class Binder(Dictionary<string, Delegate> methods) : ServiceBinderBase
    {
        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, UnaryServerMethod<TRequest, TResponse>? handler)
        {
            if (handler is not null)
            {
                methods[method.FullName] = handler;
            }
        }
    }

    /// <summary>What the emulator's method sees of its call: the method, the request's headers, the deadline.</summary>
    private sealed class EmulatorCallContext(string method, CallOptions options, CancellationToken cancellationToken) : ServerCallContext
    {
        private readonly Metadata _trailers = [];

        protected override string MethodCore => method;
        protected override string HostCore => GrpcApiConfig.Mock;
        protected override string PeerCore => GrpcApiConfig.Mock;
        protected override DateTime DeadlineCore => options.Deadline ?? DateTime.MaxValue;
        protected override Metadata RequestHeadersCore => options.Headers ?? [];
        protected override CancellationToken CancellationTokenCore => cancellationToken;
        protected override Metadata ResponseTrailersCore => _trailers;
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore => new(null, []);

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) =>
            throw new NotSupportedException("a mocked call propagates nothing");

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;
    }
}
