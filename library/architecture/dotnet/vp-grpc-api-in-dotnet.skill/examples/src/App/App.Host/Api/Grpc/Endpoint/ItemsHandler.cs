using Contracts.Todo.V1;
using Grpc.Core;
using MediatR;
using PlanItemsCommand = Todo.Interfaces.Commands.PlanItems;

namespace App.Host.Api.Grpc.Endpoint;

// This file is the ItemsService of the contract (docs/integration/api/todo/todo.proto):
// one override per rpc of its generated base, each mapping the request message
// to the command and the result back to the response message - the counterpart
// of a controller. A new rpc adds an override here.

/// <summary>
/// The ItemsService of the contract. It sends its commands through
/// <see cref="ISender"/>, so every pipeline behavior runs; an exception it
/// lets out is answered by the pipeline's ErrorAnswers, so each method is
/// written for its success alone. An rpc it does not override answers
/// UNIMPLEMENTED; the contract scenario fails then.
/// </summary>
public sealed class ItemsHandler(ISender sender) : ItemsService.ItemsServiceBase
{
    public override async Task<PlanItemsResponse> PlanItems(PlanItemsRequest request, ServerCallContext context)
    {
        var result = await sender.Send(new PlanItemsCommand([.. request.Titles]), context.CancellationToken);
        var response = new PlanItemsResponse();
        response.Items.AddRange(result.Items.Select(item => new Item { Title = item.Title, Done = item.Done }));
        return response;
    }
}
