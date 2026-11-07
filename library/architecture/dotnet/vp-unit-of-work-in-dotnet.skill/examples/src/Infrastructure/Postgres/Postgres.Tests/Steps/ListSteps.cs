using Reqnroll;
using Todo.Application.Specifications;
using Todo.Domain.Entities;
using Xunit;

namespace Postgres.Tests.Steps;

/// <summary>The items of the application, kept and read through the repository as the handlers do.</summary>
[Binding]
public sealed class ListSteps(PostgresWorld world, RepositorySteps outcomes)
{
    [Given("I keep {string} in the list {string}")]
    [When("I keep {string} in the list {string}")]
    [Given("another command keeps {string} in the list {string}")]
    public async Task WhenIKeep(string titles, string listId)
    {
        foreach (var title in titles.Split('|'))
        {
            await world.Items().AddAsync(Item.InList(listId, title));
        }
    }

    [Given("a command finds no {string} in the list {string}")]
    public async Task GivenACommandFindsNo(string title, string listId) =>
        Assert.False(await world.Items().AnyAsync(new ItemOfListTitled(listId, title)));

    [When("the first command keeps {string} in the list {string}")]
    public Task WhenTheFirstCommandKeeps(string title, string listId) =>
        outcomes.AttemptAsync("keep the first command's item", () => world.Items().AddAsync(Item.InList(listId, title)));

    [Then("the list {string} read back should hold {string}")]
    public async Task ThenTheListReadBackShouldHold(string listId, string titles) =>
        Assert.Equal(titles.Split('|'), (await world.Items().ListAsync(new ItemsOfList(listId))).Select(item => item.Title));
}
