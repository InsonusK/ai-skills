// Source: skill vp-grpc-api-in-dotnet

namespace App.Host.Api.Grpc.Pipeline;

/// <summary>
/// The gRPC entry point's own settings. They are read here, not in
/// App.Host/Config, so they come and go together with this folder.
/// </summary>
/// <param name="ListenPort">GRPC_LISTEN_PORT, the port the server listens on; default 9090.</param>
/// <param name="ReflectionEnabled">
/// GRPC_REFLECTION_ENABLED: serve gRPC server reflection, so grpcurl or Postman
/// can list and call the methods without the proto file - the gRPC
/// counterpart of Swagger UI. Off by default.
/// </param>
public sealed record GrpcConfig(int ListenPort, bool ReflectionEnabled)
{
    /// <summary>Reads the settings; throws <see cref="FormatException"/> naming the variable it could not parse.</summary>
    public static GrpcConfig Load()
    {
        var port = 9090;
        var rawPort = Environment.GetEnvironmentVariable("GRPC_LISTEN_PORT");
        if (!string.IsNullOrEmpty(rawPort) && !(ushort.TryParse(rawPort, out var parsedPort) && (port = parsedPort) > 0))
        {
            throw new FormatException($"grpc config: GRPC_LISTEN_PORT \"{rawPort}\" is not a port");
        }
        var reflection = false;
        var rawReflection = Environment.GetEnvironmentVariable("GRPC_REFLECTION_ENABLED");
        if (!string.IsNullOrEmpty(rawReflection) && !bool.TryParse(rawReflection, out reflection))
        {
            throw new FormatException($"grpc config: GRPC_REFLECTION_ENABLED \"{rawReflection}\" is not a bool");
        }
        return new GrpcConfig(port, reflection);
    }
}
