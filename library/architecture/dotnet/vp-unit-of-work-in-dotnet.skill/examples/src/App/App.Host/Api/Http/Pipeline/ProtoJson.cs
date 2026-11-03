// Source: skill vp-http-api-in-dotnet

using System.Text.Encodings.Web;
using System.Text.Json;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Shared.Errors;

namespace App.Host.Api.Http.Pipeline;

/// <summary>
/// Copied only when the contract is a proto file: the routes then read and
/// write the generated messages in the standard proto-JSON mapping (skill
/// api-develop) instead of records of their own - a field accepted by its
/// lowerCamelCase or its proto name, an unknown field ignored, a field at its
/// default left out of a response.
/// </summary>
public static class ProtoJson
{
    // An unknown field is ignored, so a client built against a newer minor
    // version of the contract still works.
    private static readonly JsonParser Parser = new(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

    /// <summary>
    /// Reads the request body as proto-JSON into the method's generated
    /// request message <typeparamref name="TMessage"/> (e.g. <c>AddItemRequest</c>).
    /// Path parameters are not in the body: the route sets them from its own
    /// parameters. A body that is not JSON, or not the message, is rejected as
    /// invalid - 400 INVALID_ARGUMENT, the command never sent.
    /// </summary>
    public static async Task<TMessage> ReadAsync<TMessage>(HttpRequest request) where TMessage : IMessage<TMessage>, new()
    {
        if (!request.HasJsonContentType())
        {
            throw new ErrorKindException(ErrorKind.Invalid, "request body is not JSON: Content-Type must be application/json");
        }
        var body = await new StreamReader(request.Body).ReadToEndAsync(request.HttpContext.RequestAborted);
        try
        {
            using var _ = JsonDocument.Parse(body);
        }
        catch (JsonException error)
        {
            throw new ErrorKindException(ErrorKind.Invalid, "request body is not valid JSON: " + error.Message);
        }
        try
        {
            return Parser.Parse<TMessage>(body);
        }
        catch (Exception error) when (error is InvalidProtocolBufferException or InvalidJsonException)
        {
            throw new ErrorKindException(ErrorKind.Invalid, "request body does not match the contract: " + error.Message);
        }
    }

    /// <summary>
    /// The answer of a route: the method's generated response message in
    /// proto-JSON, compact as every other answer, with the route's success status.
    /// </summary>
    public static IResult Result(IMessage message, int status = StatusCodes.Status200OK) =>
        Results.Content(Compact(JsonFormatter.Default.Format(message)), "application/json", statusCode: status);

    // Text as it is ("купить молоко", not "\u043A..."), as JsonFormatter writes it.
    private static readonly JsonSerializerOptions CompactJson = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    // JsonFormatter writes a space after every ':' and ','.
    private static string Compact(string json)
    {
        using var document = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(document.RootElement, CompactJson);
    }
}
