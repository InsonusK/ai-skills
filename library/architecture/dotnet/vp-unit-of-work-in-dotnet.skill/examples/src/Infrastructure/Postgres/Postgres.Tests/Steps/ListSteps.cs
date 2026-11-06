using Reqnroll;
using Todo.Domain.Entities;
using Todo.Domain.Specifications;
using Xunit;

namespace Postgres.Tests.Steps;

/// <summary>The list aggregate of the application, saved and loaded through the repository as a handler does.</summary>
[Binding]
public sealed class ListSteps(PostgresWorld world, RepositorySteps outcomes)
{
    private ItemList? _loaded;
    private Repositories.PostgresRepository<ItemList>? _loadedBy;

    [Given("I save a new list {string} with {string}")]
    [When("I save a new list {string} with {string}")]
    public async Task WhenISaveANewList(string listId, string titles)
    {
        var list = new ItemList(listId);
        foreach (var title in titles.Split('|'))
        {
            list.Add(new Item(title));
        }
        await world.Lists().AddAsync(list);
    }

    [When("I load the list {string}, add {string}, and save it")]
    public async Task WhenILoadAddAndSave(string listId, string title)
    {
        var lists = world.Lists();
        var list = await lists.SingleOrDefaultAsync(new ListById(listId));
        list!.Add(new Item(title));
        await lists.UpdateAsync(list);
    }

    [Given("a command loads the list {string}")]
    public async Task GivenACommandLoads(string listId)
    {
        _loadedBy = world.Lists();
        _loaded = await _loadedBy.SingleOrDefaultAsync(new ListById(listId));
    }

    [Given("another command adds {string} to the list {string}")]
    public Task GivenAnotherCommandAdds(string title, string listId) => WhenILoadAddAndSave(listId, title);

    [When("the first command adds {string} and saves the list")]
    public Task WhenTheFirstCommandAdds(string title)
    {
        _loaded!.Add(new Item(title));
        return outcomes.AttemptAsync("save the first command's list", () => _loadedBy!.UpdateAsync(_loaded));
    }

    [Then("the list {string} read back should hold {string}")]
    public async Task ThenTheListReadBackShouldHold(string listId, string titles)
    {
        var list = await world.Lists().SingleOrDefaultAsync(new ListById(listId));
        Assert.Equal(titles.Split('|'), list!.Items.Select(item => item.Title));
    }
}
