using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace TodoApi.Mock;

/// <summary>
/// The mock of the todo API's client (<see cref="TodoApiClient"/>): an emulator
/// of the API inside the process, which TODO_API_HOST=mock runs the client
/// against, so the application runs with no todo service reachable. It answers
/// every operation of <see cref="TodoApiClient.Operations"/> as the todo API
/// does, with stable data and no state; a new operation adds its route here.
/// </summary>
public sealed class TodoApiMock : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return (request.Method.Method, request.RequestUri?.AbsolutePath) switch
        {
            ("POST", "/v1/items/plan") => await PlanItems(request, cancellationToken),
            _ => Answer(HttpStatusCode.NotFound, new { code = 5, message = "no such operation", details = Array.Empty<object>() }),
        };
    }

    private static async Task<HttpResponseMessage> PlanItems(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        PlanItemsRequest? body = null;
        try
        {
            body = request.Content is null ? null : await request.Content.ReadFromJsonAsync<PlanItemsRequest>(JsonSerializerOptions.Web, cancellationToken);
        }
        catch (JsonException)
        {
            // answered as no titles below
        }
        if (body?.Titles is not { Count: > 0 } titles)
        {
            return Answer(HttpStatusCode.BadRequest, new { code = 3, message = "no items to plan", details = Array.Empty<object>() });
        }
        return Answer(HttpStatusCode.OK, new { items = titles.Select(title => new { title, done = false }) });
    }

    private sealed record PlanItemsRequest(IReadOnlyList<string>? Titles);

    private static HttpResponseMessage Answer(HttpStatusCode status, object body) =>
        new(status) { Content = JsonContent.Create(body, options: JsonSerializerOptions.Web) };
}
