using Client.Http;
using Client.Pipeline;
using Microsoft.Extensions.Logging;
using TodoApi.Abstractions;
using TodoApi.Mock;

namespace TodoApi;

/// <summary>
/// The adapter of the todo API of another service
/// (docs/integration/client/todo/openapi.json): one method per operation this
/// application calls, each a call of <see cref="HttpApiClient"/> with request
/// and response types of its own, mapped to <see cref="TodoApiItem"/>.
/// </summary>
public sealed class TodoApiClient(HttpApiConfig config, ILogger logger, IReadOnlyList<IClientMiddleware> middlewares) : ITodoApi
{
    /// <summary>Names the settings of this API: TODO_API_HOST, TODO_API_HTTP_PORT, TODO_API_TLS, TODO_API_TIMEOUT.</summary>
    public const string Prefix = "TODO_API";

    /// <summary>
    /// Every operation this adapter calls; one scenario holds them against the
    /// copied contract. A new method of the adapter adds its line, retryable
    /// when sending it again does what one call does.
    /// </summary>
    public static readonly IReadOnlyList<HttpOperation> Operations =
    [
        // Planning stores nothing: a repeat plans the same items.
        new(HttpMethod.Post, "/v1/items/plan", Retryable: true),
    ];

    // With TODO_API_HOST=mock every call is answered by TodoApiMock.
    private readonly HttpApiClient _http = new(config, logger, new TodoApiMock(), Operations, middlewares);

    private sealed record PlanItemsRequest(IReadOnlyList<string> Titles);

    private sealed record PlanItemsResponse(IReadOnlyList<PlanItemsResponse.Item> Items)
    {
        public sealed record Item(string Title, bool Done);
    }

    public async Task<IReadOnlyList<TodoApiItem>> PlanItemsAsync(IReadOnlyList<string> titles, CancellationToken cancellationToken)
    {
        var answer = await _http.CallAsync<PlanItemsResponse>(HttpMethod.Post, "/v1/items/plan", new PlanItemsRequest(titles), cancellationToken);
        return answer.Items.Select(item => new TodoApiItem(item.Title, item.Done)).ToList();
    }
}
