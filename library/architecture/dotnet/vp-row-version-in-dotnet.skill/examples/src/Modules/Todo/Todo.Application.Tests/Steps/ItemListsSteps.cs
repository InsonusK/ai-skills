using Reqnroll;
using Shared.Errors;
using Todo.Application.Commands;
using Todo.Application.Services;
using Todo.Commands;
using Todo.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Application.Tests.Steps;

/// <summary>
/// The item lists the handlers use, faked by hand: what they were given, what
/// they answer, the item they were asked to save.
/// </summary>
public sealed class FakeItemLists : IItemLists
{
    /// <summary>The version the fake answers for an item it keeps or saves.</summary>
    public const string KeptVersion = "8";

    public Dictionary<string, List<KeptItem>> Lists { get; } = [];
    public ErrorKind? Failure { get; set; }
    public ErrorKind? SaveFailure { get; set; }
    public KeptItem? Saved { get; private set; }
    public bool Asked { get; private set; }

    public Task<KeptItem> AddAsync(string listId, Item item, CancellationToken cancellationToken)
    {
        Asked = true;
        ThrowIfFailing();
        Lists.TryAdd(listId, []);
        var kept = new KeptItem(Lists[listId].Count + 1, KeptVersion, item);
        Lists[listId].Add(kept);
        return Task.FromResult(kept);
    }

    public Task<IReadOnlyList<KeptItem>> ItemsAsync(string listId, CancellationToken cancellationToken)
    {
        Asked = true;
        ThrowIfFailing();
        return Task.FromResult<IReadOnlyList<KeptItem>>(Lists[listId]);
    }

    public Task<KeptItem> ItemAsync(string listId, long itemId, CancellationToken cancellationToken)
    {
        Asked = true;
        ThrowIfFailing();
        var kept = Lists.GetValueOrDefault(listId)?.Find(item => item.Id == itemId)
            ?? throw new ErrorKindException(ErrorKind.NotFound, $"no item {itemId}");
        // A copy, as a service reads a new entity each time.
        var item = new Item(kept.Item.Title);
        if (kept.Item.Done)
        {
            item.Complete();
        }
        return Task.FromResult(kept with { Item = item });
    }

    public Task<KeptItem> SaveAsync(string listId, KeptItem item, CancellationToken cancellationToken)
    {
        // What the command gave, before the answer's new version.
        Saved = item with { };
        if (SaveFailure is { } kind)
        {
            throw new ErrorKindException(kind, "the item lists said no");
        }
        return Task.FromResult(item with { Version = KeptVersion });
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
    private CompleteItemResult? _completed;
    private Exception? _error;

    private static ErrorKind KindNamed(string name) => Enum.Parse<ErrorKind>(name.Replace(" ", ""), ignoreCase: true);

    [Given("the item lists answer the kind {string}")]
    public void GivenTheItemListsAnswerTheKind(string kind) => _lists.Failure = KindNamed(kind);

    [Given("the item lists hold {string} for the list {string}")]
    public void GivenTheItemListsHold(string titles, string listId) =>
        _lists.Lists[listId] = titles.Split('|').Select((title, i) => new KeptItem(i + 1, "7", new Item(title))).ToList();

    [Given("the item lists hold the item {long} {string} at the version {string}")]
    public void GivenTheItemListsHoldTheItem(long itemId, string title, string version)
    {
        _lists.Lists.TryAdd("home", []);
        _lists.Lists["home"].Add(new KeptItem(itemId, version, new Item(title)));
    }

    [Given("the item {long} is done already")]
    public void GivenTheItemIsDoneAlready(long itemId) => _lists.Lists["home"].Single(item => item.Id == itemId).Item.Complete();

    [Given("the item lists answer the kind {string} to a save")]
    public void GivenTheItemListsAnswerToASave(string kind) => _lists.SaveFailure = KindNamed(kind);

    [When("I send CompleteItem with the list {string}, the item {long}, and the version {string}")]
    public async Task WhenISendCompleteItem(string listId, long itemId, string version)
    {
        try
        {
            _completed = await new CompleteItemHandler(_lists).Handle(new CompleteItemCommand(listId, itemId, version), CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: CompleteItem({listId}, {itemId}, {version}) -> {_error?.Message ?? _completed!.ToString()}");
    }

    [Then("the item lists should have saved the item {long} done, under the version {string}")]
    public void ThenTheItemListsShouldHaveSaved(long itemId, string version)
    {
        var saved = Assert.IsType<KeptItem>(_lists.Saved);
        Assert.Equal((itemId, true, version), (saved.Id, saved.Item.Done, saved.Version));
    }

    [Then("the completed item should be done, with the version {string}")]
    public void ThenTheCompletedItemShouldBeDone(string version) => Assert.Equal((true, version), (_completed!.Item.Done, _completed.Item.Version));

    [Then("the item lists should not have saved")]
    public void ThenTheItemListsShouldNotHaveSaved() => Assert.Null(_lists.Saved);

    [Then("the added item should carry the id and the version the item lists answered")]
    public void ThenTheAddedItemShouldCarry() => Assert.Equal((1L, FakeItemLists.KeptVersion), (_added!.Item.Id, _added.Item.Version));

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
    public void ThenTheAddedItemShouldBe(string title) => Assert.Equal((title, false), (_added?.Item.Title, _added?.Item.Done));

    [Then("the item lists should have been given {string} for the list {string}")]
    public void ThenTheItemListsShouldHaveBeenGiven(string title, string listId) => Assert.Equal(title, Assert.Single(_lists.Lists[listId]).Item.Title);

    [Then("the item lists should not have been asked")]
    public void ThenTheItemListsShouldNotHaveBeenAsked() => Assert.False(_lists.Asked);

    [Then("the command should fail with the kind {string}")]
    public void ThenTheCommandShouldFailWithTheKind(string kind) => Assert.Equal(KindNamed(kind), ErrorKindException.KindOf(_error));

    [Then("the listed items should be {string}")]
    public void ThenTheListedItemsShouldBe(string titles) =>
        Assert.Equal(titles.Split('|'), _listed!.Items.Select(item => item.Title));
}
