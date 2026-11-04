using Contracts.Todo.V1;
using MediatR;
using Todo.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Errors;

namespace App.Host.Api.Http.Endpoint;

// This file is the lists resource (/v1/lists/{listId}/...). The path parameter
// is named as the generated spec names it - listId, the lowerCamelCase form of
// the proto field list_id - and bound from the route, since it is not in the body.

/// <summary>The lists resource. It sends its commands through <see cref="ISender"/>.</summary>
public sealed class ListsHandler(ISender sender)
{
    /// <summary>The lists operations of the HTTP form.</summary>
    public void Map(IEndpointRouteBuilder routes)
    {
        var lists = routes.MapGroup("/v1/lists/{listId}");
        // rpc AddItem - 201: the item is created and kept.
        lists.MapPost("/items", AddItemAsync);
        // rpc ListItems - a GET has no body.
        lists.MapGet("/items", ListItemsAsync);
        // rpc CompleteItem - the item from the path, the version from the body.
        lists.MapPost("/items/{itemId}/complete", CompleteItemAsync);
        // rpc ClearList - deprecated since 1.1.0: no command, the same answer as its gRPC form.
        lists.MapPost("/clear", () => { throw new ErrorKindException(ErrorKind.Unimplemented, "ClearList is deprecated since 1.1.0"); });
    }

    private async Task<IResult> AddItemAsync(string listId, HttpRequest request, CancellationToken cancellationToken)
    {
        var body = await Pipeline.ProtoJson.ReadAsync<AddItemRequest>(request);
        var result = await sender.Send(new AddItemCommand(listId, body.Title), cancellationToken);
        var response = new AddItemResponse { Item = Message(result.Item) };
        return Pipeline.ProtoJson.Result(response, StatusCodes.Status201Created);
    }

    private async Task<IResult> ListItemsAsync(string listId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListItemsCommand(listId), cancellationToken);
        var response = new ListItemsResponse();
        response.Items.AddRange(result.Items.Select(Message));
        return Pipeline.ProtoJson.Result(response);
    }

    // An item id that is no number is a request the contract cannot decode: 400.
    private async Task<IResult> CompleteItemAsync(string listId, string itemId, HttpRequest request, CancellationToken cancellationToken)
    {
        var body = await Pipeline.ProtoJson.ReadAsync<CompleteItemRequest>(request);
        if (!long.TryParse(itemId, out var id))
        {
            throw new ErrorKindException(ErrorKind.Invalid, $"item id \"{itemId}\" is not a number");
        }
        var result = await sender.Send(new CompleteItemCommand(listId, id, body.Version), cancellationToken);
        return Pipeline.ProtoJson.Result(new CompleteItemResponse { Item = Message(result.Item) });
    }

    // One item, with its id and version when it is kept.
    private static Item Message(ItemView view) =>
        new() { Title = view.Title, Done = view.Done, Id = view.Id ?? 0, Version = view.Version ?? "" };
}
