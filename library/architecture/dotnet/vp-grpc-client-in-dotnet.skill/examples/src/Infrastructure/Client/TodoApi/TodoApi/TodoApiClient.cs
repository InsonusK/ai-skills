using Client.Grpc;
using Client.Pipeline;
using Contracts.Todo.V1;
using Microsoft.Extensions.Logging;
using TodoApi.Abstractions;
using TodoApi.Mock;

namespace TodoApi;

/// <summary>
/// The adapter of the todo API of another service
/// (docs/integration/client/todo/todo.proto): one method per method of the
/// contract this application calls, each a call of the generated client over
/// <see cref="GrpcApiClient"/>, its response mapped to <see cref="TodoApiItem"/>.
/// </summary>
public sealed class TodoApiClient : ITodoApi, IDisposable
{
    /// <summary>Names the settings of this API: TODO_API_HOST, TODO_API_GRPC_PORT, TODO_API_TLS, TODO_API_TIMEOUT.</summary>
    public const string Prefix = "TODO_API";

    /// <summary>
    /// Every method of the contract this adapter calls, by full name; one
    /// scenario holds them against the contract. A new method of the adapter
    /// adds its line, retryable when calling it again does what one call does.
    /// </summary>
    public static readonly IReadOnlyList<GrpcMethod> Methods =
    [
        // Planning stores nothing: a repeat plans the same items.
        new("/todo.v1.ItemsService/PlanItems", Retryable: true),
    ];

    private readonly GrpcApiClient _grpc;
    private readonly ItemsService.ItemsServiceClient _items;

    public TodoApiClient(GrpcApiConfig config, ILogger logger, IReadOnlyList<IClientMiddleware> middlewares)
    {
        // With TODO_API_HOST=mock every call is answered by TodoApiMock.
        _grpc = new GrpcApiClient(config, logger, TodoApiMock.Bind, Methods, middlewares);
        _items = new ItemsService.ItemsServiceClient(_grpc.CallInvoker);
    }

    public async Task<IReadOnlyList<TodoApiItem>> PlanItemsAsync(IReadOnlyList<string> titles, CancellationToken cancellationToken)
    {
        var answer = await _items.PlanItemsAsync(new PlanItemsRequest { Titles = { titles } }, cancellationToken: cancellationToken);
        return answer.Items.Select(item => new TodoApiItem(item.Title, item.Done)).ToList();
    }

    public void Dispose() => _grpc.Dispose();
}
