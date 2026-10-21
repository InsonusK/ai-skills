using App.Host.Api.Grpc;
using App.Host.Api.Grpc.Endpoint;
using App.Host.Tests.Steps.Http;
using Contracts.Todo.V1;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Shared.Errors;
using Todo.Commands;
using Xunit;
using AddItemRequest = Contracts.Todo.V1.AddItemRequest;
using ListItemsRequest = Contracts.Todo.V1.ListItemsRequest;
using PlanItemsRequest = Contracts.Todo.V1.PlanItemsRequest;

namespace App.Host.Tests.Steps.Grpc;

/// <summary>The application's own services (Services.cs, Endpoint/) over fake handlers.</summary>
[Binding]
public sealed class ItemsServiceSteps(GrpcWorld world)
{
    private readonly FakeHandler<PlanItems, PlanItemsResult> _planItems = new();
    private readonly FakeHandler<AddItem, AddItemResult> _addItem = new();
    private readonly FakeHandler<ListItems, ListItemsResult> _listItems = new();
    private IReadOnlyList<Item> _items = [];
    private RpcException? _error;

    // The services as the composition root fills them.
    public static void ServeApplicationServices(GrpcWorld world) =>
        world.Services = sender => new GrpcEndpoints(Items: new ItemsHandler(sender)).Services;

    private static List<ItemView> Views(string titles) => titles.Split('|').Select(title => new ItemView(title, false)).ToList();

    private void Serve<TCommand, TResult>(FakeHandler<TCommand, TResult> handler) where TCommand : IRequest<TResult>
    {
        world.Handlers.AddSingleton<IRequestHandler<TCommand, TResult>>(handler);
        ServeApplicationServices(world);
    }

    private async Task<ItemsService.ItemsServiceClient> Client()
    {
        ServeApplicationServices(world);
        return new ItemsService.ItemsServiceClient(await world.ChannelAsync());
    }

    [Given("PlanItems returns the items {string} over gRPC")]
    public void GivenPlanItemsReturns(string titles)
    {
        _planItems.Answer = _ => new PlanItemsResult(Views(titles));
        Serve(_planItems);
    }

    [Given("AddItem returns the item {string} over gRPC")]
    public void GivenAddItemReturns(string title)
    {
        _addItem.Answer = _ => new AddItemResult(new ItemView(title, false));
        Serve(_addItem);
    }

    [Given("AddItem fails with the kind {string} over gRPC")]
    public void GivenAddItemFails(string kind)
    {
        _addItem.Failure = new ErrorKindException(Enum.Parse<ErrorKind>(kind.Replace(" ", ""), ignoreCase: true), "the reason");
        Serve(_addItem);
    }

    [Given("ListItems returns the items {string} over gRPC")]
    public void GivenListItemsReturns(string titles)
    {
        _listItems.Answer = _ => new ListItemsResult(Views(titles));
        Serve(_listItems);
    }

    private async Task Call(Func<ItemsService.ItemsServiceClient, Task<IReadOnlyList<Item>>> call)
    {
        try
        {
            _items = await call(await Client());
        }
        catch (RpcException error)
        {
            _error = error;
        }
    }

    [When("I call ItemsService.PlanItems with the titles {string}")]
    public Task WhenICallPlanItems(string titles) => Call(async client =>
    {
        var request = new PlanItemsRequest();
        request.Titles.AddRange(titles.Split('|'));
        return (await client.PlanItemsAsync(request)).Items;
    });

    [When("I call ItemsService.AddItem with the list {string} and the title {string}")]
    public Task WhenICallAddItem(string listId, string title) => Call(async client =>
        [(await client.AddItemAsync(new AddItemRequest { ListId = listId, Title = title })).Item]);

    [When("I call ItemsService.ListItems with the list {string}")]
    public Task WhenICallListItems(string listId) => Call(async client =>
        (await client.ListItemsAsync(new ListItemsRequest { ListId = listId })).Items);

    [When("I call ItemsService.ClearList with the list {string}")]
    public Task WhenICallClearList(string listId) => Call(async client =>
    {
#pragma warning disable CS0612 // the rpc is deprecated: the scenario checks what it answers
        await client.ClearListAsync(new ClearListRequest { ListId = listId });
#pragma warning restore CS0612
        return [];
    });

    [Then("the gRPC response should hold the items {string}, none done")]
    public void ThenTheGrpcResponseShouldHoldTheItems(string titles)
    {
        Assert.Null(_error);
        Assert.Equal(titles.Split('|'), _items.Select(item => item.Title));
        Assert.All(_items, item => Assert.False(item.Done));
    }

    [Then("the gRPC response should hold the item {string}")]
    public void ThenTheGrpcResponseShouldHoldTheItem(string title) => Assert.Equal(title, Assert.Single(_items).Title);

    [Then("the gRPC call of ItemsService should fail with {word}")]
    public void ThenTheGrpcCallShouldFailWith(string code) => Assert.Equal(Enum.Parse<StatusCode>(code), _error?.StatusCode);

    [Then("the gRPC call of ItemsService should fail with {word} and the message {string}")]
    public void ThenTheGrpcCallShouldFailWithTheMessage(string code, string message)
    {
        Assert.Equal(Enum.Parse<StatusCode>(code), _error?.StatusCode);
        Assert.Equal(message, _error!.Status.Detail);
    }

    [Then("PlanItems should have received the titles {string} over gRPC")]
    public void ThenPlanItemsShouldHaveReceived(string titles) => Assert.Equal(titles.Split('|'), _planItems.Received?.Titles);

    [Then("AddItem should have received the list {string} and the title {string} over gRPC")]
    public void ThenAddItemShouldHaveReceived(string listId, string title) => Assert.Equal(new AddItem(listId, title), _addItem.Received);

    [Then("ListItems should have received the list {string} over gRPC")]
    public void ThenListItemsShouldHaveReceived(string listId) => Assert.Equal(new ListItems(listId), _listItems.Received);
}
