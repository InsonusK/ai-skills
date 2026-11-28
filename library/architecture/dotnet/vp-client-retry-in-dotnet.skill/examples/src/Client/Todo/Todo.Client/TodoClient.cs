using TodoExampleService.Client.Core.Http;
using TodoExampleService.Client.Core.Pipeline;
using Microsoft.Extensions.Logging;
using TodoExampleService.Client.Todo.Abstractions;
using TodoExampleService.Client.Todo.Mock;

namespace TodoExampleService.Client.Todo;

/// <summary>
/// The adapter of the todo API of another service
/// (docs/integration/client/todo/openapi.json): one method per operation this
/// application calls, each a call of <see cref="HttpApiClient"/> with request
/// and response types of its own, mapped to <see cref="TodoItemDto"/>.
/// </summary>
public sealed class TodoClient(HttpApiConfig config, ILogger logger, IReadOnlyList<IClientMiddleware> middlewares) : ITodoClient
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

    // With TODO_API_HOST=mock every call is answered by TodoMock.
    private readonly HttpApiClient _http = new(config, logger, new TodoMock(), Operations, middlewares);

    private sealed record PlanItemsRequest(IReadOnlyList<string> Titles);

    private sealed record PlanItemsResponse(IReadOnlyList<PlanItemsResponse.Item> Items)
    {
        public sealed record Item(string Title, bool Done);
    }

    public async Task<IReadOnlyList<TodoItemDto>> PlanItemsAsync(IReadOnlyList<string> titles, CancellationToken cancellationToken)
    {
        var answer = await _http.CallAsync<PlanItemsResponse>(HttpMethod.Post, "/v1/items/plan", new PlanItemsRequest(titles), cancellationToken);
        return answer.Items.Select(item => new TodoItemDto(item.Title, item.Done)).ToList();
    }
}
