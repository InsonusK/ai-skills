using Postgres.Testing;
using Reqnroll;
using Shared.Errors;
using Todo.Application.Commands;
using Todo.Commands;
using Todo.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Application.Tests.Steps;

/// <summary>
/// The handlers over the repository of the items, kept in memory for one
/// scenario (Postgres.Testing); the lists are read back through ListItems.
/// </summary>
[Binding]
public sealed class ItemListsSteps(ITestOutputHelper output)
{
    private readonly InMemoryRepository<Item> _lists = new();
    private AddItemResult? _added;
    private Exception? _error;

    private static ErrorKind KindNamed(string name) => Enum.Parse<ErrorKind>(name.Replace(" ", ""), ignoreCase: true);

    private static async Task<Exception?> AttemptAsync(Func<Task> call)
    {
        try
        {
            await call();
            return null;
        }
        catch (Exception error)
        {
            return error;
        }
    }

    private async Task<IReadOnlyList<ItemView>> ListAsync(string listId) =>
        (await new ListItemsHandler(_lists).Handle(new ListItemsCommand(listId), CancellationToken.None)).Items;

    [Given("the list {string} holds {string}")]
    public async Task GivenTheListHolds(string listId, string titles)
    {
        foreach (var title in titles.Split('|'))
        {
            await new AddItemHandler(_lists).Handle(new AddItemCommand(listId, title), CancellationToken.None);
        }
    }

    [When("I send AddItem with the list {string} and the title {string}")]
    public async Task WhenISendAddItem(string listId, string title)
    {
        _error = await AttemptAsync(async () => _added = await new AddItemHandler(_lists).Handle(new AddItemCommand(listId, title), CancellationToken.None));
        output.WriteLine($"when: AddItem({listId}, {title}) -> {_error?.Message ?? _added!.ToString()}");
    }

    [When("I send ListItems with the list {string}")]
    public async Task WhenISendListItems(string listId)
    {
        _error = await AttemptAsync(() => ListAsync(listId));
        output.WriteLine($"when: ListItems({listId}) -> {_error?.Message ?? "ok"}");
    }

    [Then("the added item should be the open item {string}")]
    public void ThenTheAddedItemShouldBe(string title) => Assert.Equal(new ItemView(title, false), _added?.Item);

    [Then("the list {string} should hold {string}")]
    public async Task ThenTheListShouldHold(string listId, string titles) =>
        Assert.Equal(titles.Split('|'), (await ListAsync(listId)).Select(item => item.Title));

    [Then("nobody should have added to the list {string}")]
    public Task ThenNobodyShouldHaveAdded(string listId) => Assert.ThrowsAsync<ErrorKindException>(() => ListAsync(listId));

    [Then("the command should fail with the kind {string}")]
    public void ThenTheCommandShouldFailWithTheKind(string kind) => Assert.Equal(KindNamed(kind), ErrorKindException.KindOf(_error));
}
