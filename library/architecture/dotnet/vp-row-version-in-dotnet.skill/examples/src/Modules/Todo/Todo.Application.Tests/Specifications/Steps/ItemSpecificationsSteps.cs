using Postgres.Testing;
using Reqnroll;
using Todo.Application.Specifications;
using Todo.Domain.Entities;
using Xunit;

namespace Todo.Application.Tests.Specifications.Steps;

/// <summary>
/// The specifications of the items, evaluated over the repository in memory, as
/// the handlers' scenarios run them; the database's evaluation is proved by
/// Postgres.Tests (list-items-in-dotnet.feature).
/// </summary>
[Binding]
public sealed class ItemSpecificationsSteps
{
    private readonly InMemoryRepository<Item> _items = new();
    private List<Item> _picked = [];

    [Given("the stored items:")]
    public async Task GivenTheStoredItems(Table items)
    {
        foreach (var row in items.Rows)
        {
            await _items.AddAsync(Item.InList(row["list"], row["title"]));
        }
    }

    [When("I pick the items of the list {string}")]
    public async Task WhenIPickTheItemsOfTheList(string listId) => _picked = await _items.ListAsync(new ItemsOfList(listId));

    [When("I pick the item {string} of the list {string}")]
    public async Task WhenIPickTheItem(string title, string listId) => _picked = await _items.ListAsync(new ItemOfListTitled(listId, title));

    [Then("the picked titles should be {string}")]
    public void ThenThePickedTitlesShouldBe(string titles) => Assert.Equal(titles.Split('|'), _picked.Select(item => item.Title));

    [Then("the picked items should be {string}")]
    public void ThenThePickedItemsShouldBe(string items) =>
        Assert.Equal(items.Split('|'), _picked.Select(item => $"{item.ListId}: {item.Title}"));

    [Then("nothing should be picked")]
    public void ThenNothingShouldBePicked() => Assert.Empty(_picked);
}
