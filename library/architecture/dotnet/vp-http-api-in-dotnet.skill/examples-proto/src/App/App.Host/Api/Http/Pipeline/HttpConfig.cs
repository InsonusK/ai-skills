// Source: skill vp-http-api-in-dotnet

namespace App.Host.Api.Http.Pipeline;

/// <summary>
/// The HTTP entry point's own settings. They are read here, not in
/// App.Host/Config, so they come and go together with this folder.
/// </summary>
/// <param name="ListenPort">HTTP_LISTEN_PORT, the port the server listens on; default 8080.</param>
/// <param name="BasePath">
/// HTTP_BASE_PATH, the path prefix the service is published under: empty, or
/// with a leading "/" and no trailing one. Routes stay mounted at "/"; only
/// URLs handed to a client carry it.
/// </param>
/// <param name="DocsEnabled">HTTP_API_DOCS_ENABLED: serve the OpenAPI spec and Swagger UI.</param>
public sealed record HttpConfig(int ListenPort, string BasePath, bool DocsEnabled)
{
    /// <summary>Reads the settings; throws <see cref="FormatException"/> naming the variable it could not parse.</summary>
    public static HttpConfig Load()
    {
        var port = 8080;
        var rawPort = Environment.GetEnvironmentVariable("HTTP_LISTEN_PORT");
        if (!string.IsNullOrEmpty(rawPort) && !(ushort.TryParse(rawPort, out var parsedPort) && (port = parsedPort) > 0))
        {
            throw new FormatException($"http config: HTTP_LISTEN_PORT \"{rawPort}\" is not a port");
        }
        var docs = false;
        var rawDocs = Environment.GetEnvironmentVariable("HTTP_API_DOCS_ENABLED");
        if (!string.IsNullOrEmpty(rawDocs) && !bool.TryParse(rawDocs, out docs))
        {
            throw new FormatException($"http config: HTTP_API_DOCS_ENABLED \"{rawDocs}\" is not a bool");
        }
        return new HttpConfig(port, NormalizeBasePath(Environment.GetEnvironmentVariable("HTTP_BASE_PATH")), docs);
    }

    /// <summary>
    /// Gives a path prefix its one form: "" for none, otherwise a leading "/"
    /// and no trailing one ("orders/" and "/orders" both become "/orders").
    /// </summary>
    public static string NormalizeBasePath(string? path)
    {
        var trimmed = (path ?? "").Trim().Trim('/');
        return trimmed.Length == 0 ? "" : "/" + trimmed;
    }
}
