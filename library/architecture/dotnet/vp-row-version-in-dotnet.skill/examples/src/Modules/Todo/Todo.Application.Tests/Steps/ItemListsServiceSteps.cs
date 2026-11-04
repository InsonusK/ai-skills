using Postgres.Abstractions;
using Postgres.Abstractions.Rows;
using Postgres.Testing;
using Reqnroll;
using Shared.Errors;
using Todo.Application.Services;
using Todo.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Application.Tests.Steps;

[Binding]
public sealed class ItemListsServiceSteps(ITestOutputHelper output)
{
    private readonly InMemoryRepository<ItemRow> _rows = new();
    private IReadOnlyList<KeptItem>? _items;
    private Exception? _error;

    private ItemListsService Service() => new(_rows);

    [Given("the store answers a duplicate key")]
    public void GivenTheStoreAnswersADuplicateKey() => _rows.Failure = new DuplicateKeyException("duplicate key");

    [Given("the store answers that the database is unavailable")]
    public void GivenTheStoreAnswersUnavailable() => _rows.Failure = new StoreUnavailableException("no connection");

    [Given("the store answers a changed row")]
    public void GivenTheStoreAnswersAChangedRow() => _rows.Failure = new RowChangedException("the row is gone or was changed meanwhile");

    [When("I save the item {long} of the list {string} at the version {string} through the service")]
    public async Task WhenISave(long itemId, string listId, string version)
    {
        try
        {
            await Service().SaveAsync(listId, new KeptItem(itemId, version, new Item("buy milk")), CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: Save({listId}, {itemId} at {version}) -> {_error?.Message ?? "saved"}");
    }

    [When("I read the item {long} of the list {string} through the service")]
    public async Task WhenIReadTheItem(long itemId, string listId)
    {
        try
        {
            await Service().ItemAsync(listId, itemId, CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
    }

    [Given("the store fails with {string}")]
    public void GivenTheStoreFailsWith(string message) => _rows.Failure = new IOException(message);

    [When("I add {string} to the list {string} through the service")]
    public async Task WhenIAdd(string title, string listId)
    {
        try
        {
            await Service().AddAsync(listId, new Item(title), CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: Add({listId}, {title}) -> {_error?.Message ?? "ok"}");
    }

    [When("I read the list {string} through the service")]
    public async Task WhenIRead(string listId)
    {
        try
        {
            _items = await Service().ItemsAsync(listId, CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: Items({listId}) -> {_error?.Message ?? $"{_items!.Count} items"}");
    }

    [Then("the service should give the items {string}")]
    public void ThenTheServiceShouldGiveTheItems(string titles) => Assert.Equal(titles.Split('|'), _items!.Select(item => item.Item.Title));

    [Then("the service should fail with the kind {string} mentioning {string}")]
    public void ThenTheServiceShouldFailWithTheKind(string kind, string text)
    {
        Assert.Equal(Enum.Parse<ErrorKind>(kind.Replace(" ", ""), ignoreCase: true), ErrorKindException.KindOf(_error));
        Assert.Contains(text, _error!.Message);
    }

    [Then("the service should fail with no kind")]
    public void ThenTheServiceShouldFailWithNoKind()
    {
        Assert.NotNull(_error);
        Assert.Null(ErrorKindException.KindOf(_error));
    }
}
