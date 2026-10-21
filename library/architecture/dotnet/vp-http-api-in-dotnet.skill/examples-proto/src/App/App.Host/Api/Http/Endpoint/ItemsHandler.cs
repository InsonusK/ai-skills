using Contracts.Todo.V1;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PlanItemsCommand = Todo.Commands.PlanItems;

namespace App.Host.Api.Http.Endpoint;

// This file is the items resource (/v1/items/...) of the HTTP form of the
// proto contract: each route reads the rpc's generated request message and
// writes its response message (ProtoJson), as its google.api.http annotation
// says. A new rpc on items adds a route line and a method here.

/// <summary>The items resource. It sends its commands through <see cref="ISender"/>.</summary>
public sealed class ItemsHandler(ISender sender)
{
    /// <summary>The items operations of the HTTP form.</summary>
    public void Map(IEndpointRouteBuilder routes)
    {
        var items = routes.MapGroup("/v1/items");
        // rpc PlanItems - 200: planning creates nothing that is kept.
        items.MapPost("/plan", PlanItemsAsync);
    }

    private async Task<IResult> PlanItemsAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var body = await Pipeline.ProtoJson.ReadAsync<PlanItemsRequest>(request);
        var result = await sender.Send(new PlanItemsCommand([.. body.Titles]), cancellationToken);
        var response = new PlanItemsResponse();
        response.Items.AddRange(result.Items.Select(item => new Item { Title = item.Title, Done = item.Done }));
        return Pipeline.ProtoJson.Result(response);
    }
}
