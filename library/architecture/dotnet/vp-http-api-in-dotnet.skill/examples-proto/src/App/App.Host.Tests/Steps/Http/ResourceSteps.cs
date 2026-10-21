using App.Host.Api.Http;
using App.Host.Api.Http.Endpoint;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Shared.Errors;
using Todo.Commands;
using Xunit;

namespace App.Host.Tests.Steps.Http;

/// <summary>A fake handler: answers what the scenario set or fails with it, records the command.</summary>
public sealed class FakeHandler<TCommand, TResult> : IRequestHandler<TCommand, TResult> where TCommand : IRequest<TResult>
{
    public Func<TCommand, TResult>? Answer { get; set; }
    public Exception? Failure { get; set; }
    public TCommand? Received { get; private set; }

    public Task<TResult> Handle(TCommand command, CancellationToken cancellationToken)
    {
        Received = command;
        return Failure is null ? Task.FromResult(Answer!(command)) : throw Failure;
    }
}

/// <summary>The application's own routes (Routes.cs, Endpoint/) over fake handlers.</summary>
[Binding]
public sealed class ResourceSteps(HttpWorld world)
{
    private readonly FakeHandler<PlanItems, PlanItemsResult> _planItems = new();
    private readonly FakeHandler<AddItem, AddItemResult> _addItem = new();
    private readonly FakeHandler<ListItems, ListItemsResult> _listItems = new();

    // The routes as the composition root fills them.
    public static void MapApplicationRoutes(HttpWorld world) =>
        world.Routes = (routes, sender) => new HttpEndpoints(Items: new ItemsHandler(sender), Lists: new ListsHandler(sender)).Map(routes);

    private static List<ItemView> Views(string titles) => titles.Split('|').Select(title => new ItemView(title, false)).ToList();

    private void Serve<TCommand, TResult>(FakeHandler<TCommand, TResult> handler) where TCommand : IRequest<TResult>
    {
        world.Handlers.AddSingleton<IRequestHandler<TCommand, TResult>>(handler);
        MapApplicationRoutes(world);
    }

    [Given("the HTTP API serves the application's routes")]
    public void GivenTheHttpApiServesTheApplicationsRoutes() => MapApplicationRoutes(world);

    [Given("PlanItems returns the items {string}")]
    public void GivenPlanItemsReturns(string titles)
    {
        _planItems.Answer = _ => new PlanItemsResult(Views(titles));
        Serve(_planItems);
    }

    [Given("AddItem returns the item {string}")]
    public void GivenAddItemReturns(string title)
    {
        _addItem.Answer = _ => new AddItemResult(new ItemView(title, false));
        Serve(_addItem);
    }

    [Given("AddItem fails with the kind {string} and {string}")]
    public void GivenAddItemFails(string kind, string message)
    {
        _addItem.Failure = new ErrorKindException(Enum.Parse<ErrorKind>(kind.Replace(" ", ""), ignoreCase: true), message);
        Serve(_addItem);
    }

    [Given("ListItems returns the items {string}")]
    public void GivenListItemsReturns(string titles)
    {
        _listItems.Answer = _ => new ListItemsResult(Views(titles));
        Serve(_listItems);
    }

    [Then("PlanItems should have received the titles {string}")]
    public void ThenPlanItemsShouldHaveReceived(string titles) => Assert.Equal(titles.Split('|'), _planItems.Received?.Titles);

    [Then("AddItem should have received the list {string} and the title {string}")]
    public void ThenAddItemShouldHaveReceived(string listId, string title) => Assert.Equal(new AddItem(listId, title), _addItem.Received);

    [Then("ListItems should have received the list {string}")]
    public void ThenListItemsShouldHaveReceived(string listId) => Assert.Equal(new ListItems(listId), _listItems.Received);
}
