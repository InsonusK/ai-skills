using Memory.Abstractions;
using Reqnroll;
using Shared.Errors;
using Todo.Application.Services;
using Todo.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Application.Tests.Steps;

/// <summary>The store the service uses, faked by hand: rows kept per list, or the failure the scenario set.</summary>
public sealed class FakeListStore : IListStore
{
    private readonly Dictionary<string, List<ListRow>> _rows = [];
    public Exception? Failure { get; set; }

    public Task InsertAsync(string listId, ListRow row, CancellationToken cancellationToken)
    {
        if (Failure is not null)
        {
            throw Failure;
        }
        _rows.TryAdd(listId, []);
        _rows[listId].Add(row);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ListRow>?> SelectAsync(string listId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ListRow>?>(_rows.TryGetValue(listId, out var rows) ? rows : null);
}

[Binding]
public sealed class ItemListsServiceSteps(ITestOutputHelper output)
{
    private readonly FakeListStore _store = new();
    private IReadOnlyList<Item>? _items;
    private Exception? _error;

    private ItemListsService Service() => new(_store);

    [Given("the store answers a duplicate key")]
    public void GivenTheStoreAnswersADuplicateKey() => _store.Failure = new DuplicateKeyException("duplicate key");

    [Given("the store fails with {string}")]
    public void GivenTheStoreFailsWith(string message) => _store.Failure = new IOException(message);

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
