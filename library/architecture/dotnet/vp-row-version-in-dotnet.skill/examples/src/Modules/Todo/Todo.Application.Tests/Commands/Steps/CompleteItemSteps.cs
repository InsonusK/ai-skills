using Ardalis.Specification;
using Postgres.Testing;
using Reqnroll;
using Todo.Application.Commands;
using Todo.Application.Tests.Common.Steps;
using Todo.Commands;
using Todo.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Application.Tests.Commands.Steps;

/// <summary>
/// The items in memory, recording what a handler read and saved (the handlers'
/// scenarios run over a subclass of the store's in-memory repository, never a
/// hand fake of Ardalis's contract): the version comparison itself is the
/// database's, proven by the shared scenarios and by persistence.feature.
/// </summary>
public sealed class RecordingItemsRepository : InMemoryRepository<Item>
{
    public int Reads { get; private set; }

    public List<Item> Saved { get; } = [];

    public override Task<Item?> FirstOrDefaultAsync(ISpecification<Item> specification, CancellationToken cancellationToken = default)
    {
        Reads++;
        return base.FirstOrDefaultAsync(specification, cancellationToken);
    }

    public override Task<int> UpdateAsync(Item entity, CancellationToken cancellationToken = default)
    {
        Saved.Add(entity);
        return base.UpdateAsync(entity, cancellationToken);
    }
}

/// <summary>
/// The steps of complete-item-in-dotnet.feature: the handler over the items in
/// memory. The store never moves a version here, so an item's id stays 0 and a
/// saved version is the caller's own; the answer's version after a real save is
/// proven by persistence.feature.
/// </summary>
[Binding]
public sealed class CompleteItemSteps(CommandOutcome outcome, ITestOutputHelper output)
{
    private readonly RecordingItemsRepository _items = new();
    private long _itemId;
    private int _savedBefore;
    private CompleteItemResult? _completed;

    [Given("the list {string} holds the item {string}")]
    public async Task GivenTheListHoldsTheItem(string listId, string title)
    {
        var added = await new AddItemHandler(_items).Handle(new AddItemCommand(listId, title), CancellationToken.None);
        _itemId = added.Item.Id ?? 0;
    }

    [Given("the item is done already")]
    public async Task GivenTheItemIsDoneAlready()
    {
        await new CompleteItemHandler(_items).Handle(new CompleteItemCommand("home", _itemId, "1"), CancellationToken.None);
        _items.Saved.Clear();
    }

    [When("I send CompleteItem with the list {string}, its item, and the version {string}")]
    public Task WhenISendCompleteItemForItsItem(string listId, string version) => AttemptAsync(listId, _itemId, version);

    [When("I send CompleteItem with the list {string}, the item {long}, and the version {string}")]
    public Task WhenISendCompleteItemForTheItem(string listId, long itemId, string version) => AttemptAsync(listId, itemId, version);

    private async Task AttemptAsync(string listId, long itemId, string version)
    {
        _savedBefore = _items.Saved.Count;
        await outcome.AttemptAsync(async () =>
            _completed = await new CompleteItemHandler(_items).Handle(new CompleteItemCommand(listId, itemId, version), CancellationToken.None));
        output.WriteLine($"when: CompleteItem({listId}, {itemId}, {version}) -> {outcome.Error?.Message ?? _completed!.ToString()}");
    }

    [Then("no failure should have come back")]
    public void ThenNoFailureShouldHaveComeBack() => Assert.Null(outcome.Error);

    [Then("the item should be done")]
    public async Task ThenTheItemShouldBeDone() => Assert.True((await ReadTheItem()).Done);

    [Then("the item should be open")]
    public async Task ThenTheItemShouldBeOpen() => Assert.False((await ReadTheItem()).Done);

    [Then("the item should have been saved under the version {string}")]
    public void ThenTheItemShouldHaveBeenSavedUnderTheVersion(string version)
    {
        var saved = Assert.Single(_items.Saved);
        Assert.Equal(version, saved.Version.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Then("the completed item should come back done, with its id and its version")]
    public void ThenTheCompletedItemShouldComeBack()
    {
        Assert.NotNull(_completed);
        Assert.True(_completed.Item.Done);
        Assert.NotNull(_completed.Item.Id);
        Assert.NotNull(_completed.Item.Version);
    }

    [Then("the items should not have been read")]
    public void ThenTheItemsShouldNotHaveBeenRead() => Assert.Equal(0, _items.Reads);

    [Then("nothing should have been saved meanwhile")]
    public void ThenNothingShouldHaveBeenSaved() => Assert.Equal(_savedBefore, _items.Saved.Count);

    private async Task<Item> ReadTheItem() =>
        (await _items.ListAsync(CancellationToken.None)).Single(item => item.Id == _itemId);
}
