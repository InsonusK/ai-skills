using Reqnroll;
using Shared.Errors;
using Todo.Application.Commands;
using Todo.Application.Services;
using Todo.Commands;
using Todo.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Application.Tests.Steps;

/// <summary>The item lists the handlers use, faked by hand: what they were given, what they answer.</summary>
public sealed class FakeItemLists : IItemLists
{
    public Dictionary<string, List<Item>> Lists { get; } = [];
    public ErrorKind? Failure { get; set; }
    public bool Asked { get; private set; }

    public Task AddAsync(string listId, Item item, CancellationToken cancellationToken)
    {
        Asked = true;
        ThrowIfFailing();
        Lists.TryAdd(listId, []);
        Lists[listId].Add(item);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Item>> ItemsAsync(string listId, CancellationToken cancellationToken)
    {
        Asked = true;
        ThrowIfFailing();
        return Task.FromResult<IReadOnlyList<Item>>(Lists[listId]);
    }

    private void ThrowIfFailing()
    {
        if (Failure is { } kind)
        {
            throw new ErrorKindException(kind, "the item lists said no");
        }
    }
}

[Binding]
public sealed class ItemListsSteps(ITestOutputHelper output)
{
    private readonly FakeItemLists _lists = new();
    private AddItemResult? _added;
    private ListItemsResult? _listed;
    private Exception? _error;

    private static ErrorKind KindNamed(string name) => Enum.Parse<ErrorKind>(name.Replace(" ", ""), ignoreCase: true);

    [Given("the item lists answer the kind {string}")]
    public void GivenTheItemListsAnswerTheKind(string kind) => _lists.Failure = KindNamed(kind);

    [Given("the item lists hold {string} for the list {string}")]
    public void GivenTheItemListsHold(string titles, string listId) => _lists.Lists[listId] = titles.Split('|').Select(title => new Item(title)).ToList();

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

    [Then("the item lists should have been given {string} for the list {string}")]
    public void ThenTheItemListsShouldHaveBeenGiven(string title, string listId) => Assert.Equal(title, Assert.Single(_lists.Lists[listId]).Title);

    [Then("the item lists should not have been asked")]
    public void ThenTheItemListsShouldNotHaveBeenAsked() => Assert.False(_lists.Asked);

    [Then("the command should fail with the kind {string}")]
    public void ThenTheCommandShouldFailWithTheKind(string kind) => Assert.Equal(KindNamed(kind), ErrorKindException.KindOf(_error));

    [Then("the listed items should be {string}")]
    public void ThenTheListedItemsShouldBe(string titles) =>
        Assert.Equal(titles.Split('|').Select(title => new ItemView(title, false)), _listed!.Items);
}
