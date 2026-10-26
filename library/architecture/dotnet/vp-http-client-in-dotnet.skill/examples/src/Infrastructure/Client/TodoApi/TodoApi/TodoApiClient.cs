using Client.Http;
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
public sealed class TodoApiClient(HttpApiConfig config, ILogger logger) : ITodoApi
{
    /// <summary>Names the settings of this API: TODO_API_HOST, TODO_API_HTTP_PORT, TODO_API_TLS, TODO_API_TIMEOUT.</summary>
    public const string Prefix = "TODO_API";

    /// <summary>
    /// Every operation this adapter calls; one scenario holds them against the
    /// copied contract. A new method of the adapter adds its line.
    /// </summary>
    public static readonly IReadOnlyList<(HttpMethod Method, string Path)> Operations =
    [
        (HttpMethod.Post, "/v1/items/plan"),
    ];

    // With TODO_API_HOST=mock every call is answered by TodoApiMock.
    private readonly HttpApiClient _http = new(config, logger, new TodoApiMock());

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
