using Postgres.Abstractions;
using Postgres.Abstractions.Rows;
using Postgres.Testing;
using Reqnroll;
using Shared.Errors;
using Todo.Application.Services;
using Todo.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Application.Tests.Services.Steps;

[Binding]
public sealed class ItemListsServiceSteps(ITestOutputHelper output)
{
    private readonly InMemoryRepository<ItemRow> _rows = new();
    private IReadOnlyList<Item>? _items;
    private Exception? _error;

    private ItemListsService Service() => new(_rows);

    [Given("the store answers a duplicate key")]
    public void GivenTheStoreAnswersADuplicateKey() => _rows.Failure = new DuplicateKeyException("duplicate key");

    [Given("the store answers that the database is unavailable")]
    public void GivenTheStoreAnswersUnavailable() => _rows.Failure = new StoreUnavailableException("no connection");

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
    public void ThenTheServiceShouldGiveTheItems(string titles) => Assert.Equal(titles.Split('|'), _items!.Select(item => item.Title));

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
