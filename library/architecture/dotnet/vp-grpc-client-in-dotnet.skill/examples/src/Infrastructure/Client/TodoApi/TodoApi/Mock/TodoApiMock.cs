using Contracts.Todo.V1;
using Grpc.Core;

namespace TodoApi.Mock;

/// <summary>
/// The mock of the todo API's client (<see cref="TodoApiClient"/>): an emulator
/// of the API inside the process, which TODO_API_HOST=mock runs the client
/// against, so the application runs with no todo service reachable. It answers
/// every method of <see cref="TodoApiClient.Methods"/> as the todo API does,
/// with stable data and no state; a method it does not override answers
/// UNIMPLEMENTED.
/// </summary>
internal sealed class TodoApiMock : ItemsService.ItemsServiceBase
{
    /// <summary>Binds the emulated services, as a server would serve them.</summary>
    public static void Bind(ServiceBinderBase binder) => ItemsService.BindService(binder, new TodoApiMock());

    public override Task<PlanItemsResponse> PlanItems(PlanItemsRequest request, ServerCallContext context)
    {
        if (request.Titles.Count == 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "no items to plan"));
        }
        var response = new PlanItemsResponse();
        response.Items.AddRange(request.Titles.Select(title => new Item { Title = title, Done = false }));
        return Task.FromResult(response);
    }
}
