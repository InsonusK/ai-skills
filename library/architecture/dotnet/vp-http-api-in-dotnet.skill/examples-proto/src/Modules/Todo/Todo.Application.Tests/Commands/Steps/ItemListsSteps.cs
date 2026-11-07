using Reqnroll;
using Shared.Errors;
using Todo.Application.Commands;
using Todo.Application.Tests.Common.Steps;
using Todo.Commands;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Application.Tests.Commands.Steps;

/// <summary>The handlers over the lists of one scenario; the lists are read back through ListItems.</summary>
[Binding]
public sealed class ItemListsSteps(CommandOutcome outcome, ITestOutputHelper output)
{
    private readonly KeptLists _lists = new();
    private AddItemResult? _added;

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
        await outcome.AttemptAsync(async () => _added = await new AddItemHandler(_lists).Handle(new AddItemCommand(listId, title), CancellationToken.None));
        output.WriteLine($"when: AddItem({listId}, {title}) -> {outcome.Error?.Message ?? _added!.ToString()}");
    }

    [When("I send ListItems with the list {string}")]
    public async Task WhenISendListItems(string listId)
    {
        await outcome.AttemptAsync(() => ListAsync(listId));
        output.WriteLine($"when: ListItems({listId}) -> {outcome.Error?.Message ?? "ok"}");
    }

    [Then("the added item should be the open item {string}")]
    public void ThenTheAddedItemShouldBe(string title) => Assert.Equal(new ItemView(title, false), _added?.Item);

    [Then("the list {string} should hold {string}")]
    public async Task ThenTheListShouldHold(string listId, string titles) =>
        Assert.Equal(titles.Split('|'), (await ListAsync(listId)).Select(item => item.Title));

    [Then("nobody should have added to the list {string}")]
    public Task ThenNobodyShouldHaveAdded(string listId) => Assert.ThrowsAsync<ErrorKindException>(() => ListAsync(listId));

}
