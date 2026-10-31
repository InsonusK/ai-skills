// Source: skill vp-http-client-in-dotnet

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BuildingBlocks.Logging;
using Client.Abstractions;
using Client.Pipeline;
using Microsoft.Extensions.Logging;

namespace Client.Http;

/// <summary>One operation of the contract an adapter calls.</summary>
/// <param name="Retryable">Marks an operation safe to send again: a repeat does what one call does. Only such an operation is repeated by the retry middleware.</param>
public sealed record HttpOperation(HttpMethod Method, string Path, bool Retryable = false);

/// <summary>
/// Calls one HTTP API: the fixed part of every adapter of such an API
/// (Infrastructure/Client/{Api}), which writes each of its operations as one
/// <see cref="CallAsync{TResponse}"/> with its path and its two types.
/// </summary>
public sealed class HttpApiClient
{
    /// <summary>Carries the request id of the call being served to the API.</summary>
    public const string RequestIdHeader = "X-Request-Id";

    /// <summary>The most of an answer read; a longer one is an error.</summary>
    private const int MaxBody = 10 << 20;

    private readonly HttpApiConfig _config;
    private readonly HttpClient _http;
    private readonly ILogger _logger;
    private readonly Dictionary<string, HttpOperation> _operations;
    private readonly IReadOnlyList<IClientMiddleware> _middlewares;

    /// <summary>
    /// Builds the client of the API <paramref name="config"/> names, which
    /// calls only <paramref name="operations"/>, each through
    /// <paramref name="middlewares"/>, the first the outermost. Mocked,
    /// every call is answered by <paramref name="emulator"/> inside the
    /// process - the same request, log, and reading of the answer, only no
    /// network - and a WARN record says so.
    /// </summary>
    public HttpApiClient(HttpApiConfig config, ILogger logger, HttpMessageHandler emulator,
        IReadOnlyList<HttpOperation> operations, IReadOnlyList<IClientMiddleware> middlewares)
    {
        _config = config;
        _logger = logger;
        _operations = operations.ToDictionary(operation => Key(operation.Method, operation.Path));
        _middlewares = middlewares;
        if (config.Mocked)
        {
            _http = new HttpClient(emulator) { BaseAddress = new Uri("http://" + HttpApiConfig.Mock) };
            logger.Write(LogLevel.Warning, "api client mocked", LogAttr.Of("api", config.Name));
        }
        else
        {
            _http = new HttpClient { BaseAddress = config.BaseUri };
        }
        // The timeout of a call is the client's own, per call, below.
        _http.Timeout = Timeout.InfiniteTimeSpan;
    }

    /// <summary>
    /// Sends one request of an operation of the adapter's list -
    /// <paramref name="request"/> as its JSON body, none when null - and
    /// decodes a 2xx answer into <typeparamref name="TResponse"/>, through the
    /// client middleware, each attempt as <see cref="AttemptAsync{TResponse}"/>
    /// does. A call of an operation not in the list throws
    /// <see cref="InvalidOperationException"/> before it is sent.
    /// </summary>
    public async Task<TResponse> CallAsync<TResponse>(HttpMethod method, string path, object? request, CancellationToken cancellationToken)
    {
        if (!_operations.TryGetValue(Key(method, path), out var operation))
        {
            throw new InvalidOperationException($"{method} {path} is no operation of the adapter's list");
        }
        TResponse response = default!;
        await ClientPipeline.RunAsync(new ClientCall(_config.Name, Key(method, path), operation.Retryable), _middlewares,
            async token => response = await AttemptAsync<TResponse>(method, path, request, token), cancellationToken);
        return response;
    }

    private static string Key(HttpMethod method, string path) => $"{method.Method} {path}";

    /// <summary>
    /// Sends the request once. It ends after the timeout at the latest, sends the request id of the
    /// call being served, and writes two records: "http client request sent"
    /// and "http client response received". It throws
    /// <see cref="ApiUnavailableException"/>, <see cref="ApiStatusException"/>,
    /// or <see cref="InvalidDataException"/> when the answer does not fit
    /// <typeparamref name="TResponse"/>; a cancelled
    /// <paramref name="cancellationToken"/> cancels the call as it is.
    /// </summary>
    private async Task<TResponse> AttemptAsync<TResponse>(HttpMethod method, string path, object? request, CancellationToken cancellationToken)
    {
        var api = LogAttr.Of("api", _config.Name);
        var called = new[] { api, LogAttr.Of("method", method.Method), LogAttr.Of("path", path) };
        _logger.Write(LogLevel.Information, "http client request sent", [.. called, LogAttr.Payload(new Dictionary<string, object?> { ["request"] = request })]);
        var started = Stopwatch.GetTimestamp();
        var status = 0;
        object? received = null;
        Exception? failure = null;
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(_config.Timeout);
            using var message = new HttpRequestMessage(method, path);
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (request is not null)
            {
                message.Content = new StringContent(JsonSerializer.Serialize(request, JsonSerializerOptions.Web), Encoding.UTF8, "application/json");
            }
            if (RequestId.Current is { } id)
            {
                message.Headers.Add(RequestIdHeader, id);
            }

            string body;
            try
            {
                using var answer = await _http.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                status = (int)answer.StatusCode;
                body = await ReadAsync(answer, timeout.Token);
            }
            catch (HttpRequestException noAnswer)
            {
                throw new ApiUnavailableException(_config.Name, $"{method} {path}: {noAnswer.Message}", noAnswer);
            }
            catch (OperationCanceledException noAnswer) when (!cancellationToken.IsCancellationRequested)
            {
                throw new ApiUnavailableException(_config.Name, $"{method} {path}: no answer within {_config.Timeout}", noAnswer);
            }
            received = body;

            if (status is (int)HttpStatusCode.BadGateway or (int)HttpStatusCode.ServiceUnavailable or (int)HttpStatusCode.GatewayTimeout)
            {
                throw new ApiUnavailableException(_config.Name, $"{method} {path}: answered {status}");
            }
            if (status >= 400)
            {
                throw StatusError(status, body);
            }
            try
            {
                var response = JsonSerializer.Deserialize<TResponse>(body, JsonSerializerOptions.Web)
                    ?? throw new InvalidDataException($"{method} {path}: the answer is empty");
                received = response;
                return response;
            }
            catch (JsonException wrongShape)
            {
                throw new InvalidDataException($"{method} {path}: the answer does not match the contract: {wrongShape.Message}", wrongShape);
            }
        }
        catch (Exception error)
        {
            failure = error;
            throw;
        }
        finally
        {
            // A failed call's record repeats the request, so it is in the log
            // even when only failed records keep their payload.
            var payload = new Dictionary<string, object?> { ["response"] = received };
            if (failure is not null)
            {
                payload["request"] = request;
            }
            _logger.Write(LogLevel.Information, "http client response received",
                [.. called, LogAttr.Of("status", status), LogAttr.Of("duration", Stopwatch.GetElapsedTime(started)),
                 LogAttr.Failed(failure is not null), LogAttr.Payload(payload)]);
        }
    }

    /// <summary>Reads the body of <paramref name="answer"/>, at most <see cref="MaxBody"/> bytes.</summary>
    private static async Task<string> ReadAsync(HttpResponseMessage answer, CancellationToken cancellationToken)
    {
        await using var stream = await answer.Content.ReadAsStreamAsync(cancellationToken);
        var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await stream.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > MaxBody)
            {
                throw new InvalidDataException($"the answer is longer than {MaxBody} bytes");
            }
            buffer.Write(chunk, 0, read);
        }
        return Encoding.UTF8.GetString(buffer.GetBuffer(), 0, (int)buffer.Length);
    }

    /// <summary>Reads a google.rpc.Status body, or keeps the status alone with the code Unknown.</summary>
    private ApiStatusException StatusError(int status, string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("code", out var code) && code.TryGetInt32(out var number) && number != 0)
            {
                var message = document.RootElement.TryGetProperty("message", out var text) ? text.GetString() ?? "" : "";
                return new ApiStatusException(_config.Name, (ApiStatusCode)number, message, status);
            }
        }
        catch (JsonException)
        {
            // not a google.rpc.Status: the status alone
        }
        return new ApiStatusException(_config.Name, ApiStatusCode.Unknown, "", status);
    }
}
