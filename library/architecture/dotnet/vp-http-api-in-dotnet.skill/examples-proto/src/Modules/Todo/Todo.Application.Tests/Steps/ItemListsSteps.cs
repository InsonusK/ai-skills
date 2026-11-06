using Ardalis.Specification;
using Memory;
using Reqnroll;
using Shared.Errors;
using Todo.Application.Commands;
using Todo.Commands;
using Todo.Domain.Entities;
using Todo.Domain.Specifications;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Application.Tests.Steps;

/// <summary>
/// The in-memory repository, "the item lists" of the shared scenarios, with what
/// they ask of a store besides:
/// whether a handler asked it, and a failure it answers every read with.
/// </summary>
public sealed class ScenarioLists() : MemoryRepository<ItemList>(new MemoryStore())
{
    public ErrorKind? Failure { get; set; }
    public bool Asked { get; private set; }

    public override Task<ItemList?> SingleOrDefaultAsync(ISingleResultSpecification<ItemList> specification, CancellationToken cancellationToken = default)
    {
        Asked = true;
        if (Failure is { } kind)
        {
            throw new ErrorKindException(kind, "the store said no");
        }
        return base.SingleOrDefaultAsync(specification, cancellationToken);
    }
}

[Binding]
public sealed class ItemListsSteps(ITestOutputHelper output)
{
    private readonly ScenarioLists _lists = new();
    private AddItemResult? _added;
    private ListItemsResult? _listed;
    private Exception? _error;

    private static ErrorKind KindNamed(string name) => Enum.Parse<ErrorKind>(name.Replace(" ", ""), ignoreCase: true);

    [Given("the item lists answer the kind {string}")]
    public void GivenTheStoreAnswersTheKind(string kind) => _lists.Failure = KindNamed(kind);

    [Given("the item lists hold {string} for the list {string}")]
    public async Task GivenTheItemListsHold(string titles, string listId)
    {
        var list = new ItemList(listId);
        foreach (var title in titles.Split('|'))
        {
            list.Add(new Item(title));
        }
        await _lists.AddAsync(list);
    }

    [When("I send AddItem with the list {string} and the title {string}")]
    public async Task WhenISendAddItem(string listId, string title)
    {
        try
        {
            _added = await new AddItemHandler(_lists).Handle(new AddItemCommand(listId, title), CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: AddItem({listId}, {title}) -> {_error?.Message ?? _added!.ToString()}");
    }

    [When("I send ListItems with the list {string}")]
    public async Task WhenISendListItems(string listId)
    {
        try
        {
            _listed = await new ListItemsHandler(_lists).Handle(new ListItemsCommand(listId), CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: ListItems({listId}) -> {_error?.Message ?? $"{_listed!.Items.Count} items"}");
    }

    [Then("the added item should be the open item {string}")]
    public void ThenTheAddedItemShouldBe(string title) => Assert.Equal(new ItemView(title, false), _added?.Item);

    [Then("the item lists should hold {string} for the list {string}")]
    [Then("the item lists should have been given {string} for the list {string}")]
    public async Task ThenTheItemListsShouldHold(string titles, string listId)
    {
        var list = await _lists.SingleOrDefaultAsync(new ListById(listId));
        Assert.Equal(titles.Split('|'), list!.Items.Select(item => item.Title));
    }

    [Then("the item lists should not have been asked")]
    public void ThenTheStoreShouldNotHaveBeenAsked() => Assert.False(_lists.Asked);

    [Then("the command should fail with the kind {string}")]
    public void ThenTheCommandShouldFailWithTheKind(string kind) => Assert.Equal(KindNamed(kind), ErrorKindException.KindOf(_error));

    [Then("the listed items should be {string}")]
    public void ThenTheListedItemsShouldBe(string titles) =>
        Assert.Equal(titles.Split('|').Select(title => new ItemView(title, false)), _listed!.Items);
}
