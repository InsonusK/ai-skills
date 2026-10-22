using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Todo.Commands;

namespace App.Host.Api.Http.Endpoint;

// This file is the items resource (/v1/items/...): its routes, and the request
// and response types of each route with their mappings - the counterpart of a
// controller. A new command on items adds a route line, a method, and its
// types here, and its path in openapi.json.

/// <summary>
/// The items resource. It sends its commands through <see cref="ISender"/>, so
/// every pipeline behavior runs; an exception it lets out is answered by the
/// pipeline's ErrorAnswers, so each route is written for its success alone.
/// </summary>
public sealed class ItemsHandler(ISender sender)
{
    /// <summary>The items operations of the published contract.</summary>
    public void Map(IEndpointRouteBuilder routes)
    {
        var items = routes.MapGroup("/v1/items");
        // 200: planning creates nothing that is kept, and its work is done.
        items.MapPost("/plan", PlanItemsAsync);
    }

    private async Task<Ok<PlanItemsResponse>> PlanItemsAsync(PlanItemsRequest body, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new PlanItemsCommand(body.Titles ?? []), cancellationToken);
        return TypedResults.Ok(new PlanItemsResponse(result.Items.Select(item => new ItemResponse(item.Title, item.Done)).ToList()));
    }
}

/// <summary>The body of POST /v1/items/plan. Unknown fields are ignored.</summary>
public sealed record PlanItemsRequest(IReadOnlyList<string>? Titles);

/// <summary>The answer of POST /v1/items/plan.</summary>
public sealed record PlanItemsResponse(IReadOnlyList<ItemResponse> Items);

public sealed record ItemResponse(string Title, bool Done);
