using App.Host.Api.Cli.Endpoint;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Todo.Commands;
using Xunit;

namespace App.Host.Tests.Steps.Cli;

public sealed class FakeAddItemHandler : IRequestHandler<AddItemCommand, AddItemResult>
{
    public string Title { get; set; } = "";
    public AddItemCommand? Received { get; private set; }
    public Task<AddItemResult> Handle(AddItemCommand command, CancellationToken stopping)
    {
        Received = command;
        return Task.FromResult(new AddItemResult(new ItemView(Title, false, 41, "v7")));
    }
}

public sealed class FakeListItemsHandler : IRequestHandler<ListItemsCommand, ListItemsResult>
{
    public string Title { get; set; } = "";
    public ListItemsCommand? Received { get; private set; }
    public Task<ListItemsResult> Handle(ListItemsCommand command, CancellationToken stopping)
    {
        Received = command;
        return Task.FromResult(new ListItemsResult([new ItemView(Title, false, 41, "v7")]));
    }
}

public sealed class FakeCompleteItemHandler : IRequestHandler<CompleteItemCommand, CompleteItemResult>
{
    public CompleteItemCommand? Received { get; private set; }
    public Task<CompleteItemResult> Handle(CompleteItemCommand command, CancellationToken stopping)
    {
        Received = command;
        return Task.FromResult(new CompleteItemResult(new ItemView("buy-milk", true, 41, "v8")));
    }
}

[Binding]
[Scope(Feature = "lists commands in dotnet")]
public sealed class CliListsSteps(CliWorld world)
{
    private readonly FakeCompleteItemHandler _complete = new();
    private readonly FakeAddItemHandler _add = new();
    private readonly FakeListItemsHandler _list = new();

    [Given("the list command answers {string}")]
    public void GivenTheCommandAnswers(string title) => _add.Title = _list.Title = title;

    [When("I run the lists command {string}")]
    public async Task WhenIRun(string args)
    {
        using var provider = new ServiceCollection()
            .AddMediatR(mediatr => mediatr.RegisterServicesFromAssemblyContaining<FakeAddItemHandler>())
            .AddSingleton<IRequestHandler<CompleteItemCommand, CompleteItemResult>>(_complete)
            .AddSingleton<IRequestHandler<AddItemCommand, AddItemResult>>(_add)
            .AddSingleton<IRequestHandler<ListItemsCommand, ListItemsResult>>(_list)
            .BuildServiceProvider();
        await world.RunAsync("todo", "1", args, [new ListsHandler(provider.GetRequiredService<ISender>()).Resource()]);
    }

    [Then("AddItem should have received list {string} and title {string}")]
    public void ThenAddReceived(string list, string title) => Assert.Equal(new AddItemCommand(list, title), _add.Received);

    [Then("ListItems should have received list {string}")]
    public void ThenListReceived(string list) => Assert.Equal(new ListItemsCommand(list), _list.Received);

    [Then("CompleteItem should have received list {string}, id {long} and version {string}")]
    public void ThenCompleteReceived(string list, long id, string version) =>
        Assert.Equal(new CompleteItemCommand(list, id, version), _complete.Received);

    [Then("neither list handler should have been called")]
    public void ThenNeitherWasCalled()
    {
        Assert.Null(_add.Received);
        Assert.Null(_list.Received);
        Assert.Null(_complete.Received);
    }
}
