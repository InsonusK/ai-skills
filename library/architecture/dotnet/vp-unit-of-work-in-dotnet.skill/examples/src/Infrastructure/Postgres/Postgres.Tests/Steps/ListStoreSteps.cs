using Postgres.Abstractions;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace Postgres.Tests.Steps;

[Binding]
public sealed class ListStoreSteps(PostgresWorld world, ITestOutputHelper output)
{
    private Exception? _error;
    private IReadOnlyList<ListRow>? _rows;

    [Given("a migrated database")]
    public Task GivenAMigratedDatabase() => world.CreateMigratedDatabaseAsync();

    [Given("one context for the whole scenario")]
    public void GivenOneContext() => world.KeepOneContext();

    [Given("the list {string} holds {string}")]
    public Task GivenTheListHolds(string listId, string title) =>
        world.Store().InsertAsync(listId, new ListRow(title, false), CancellationToken.None);

    [When("I insert {string} into the list {string}")]
    public async Task WhenIInsert(string title, string listId)
    {
        _error = null;
        try
        {
            await (world.OneContext ?? world.Store()).InsertAsync(listId, new ListRow(title, false), CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: insert {title} -> {_error?.GetType().Name ?? "ok"}");
    }

    [When("I select the list {string}")]
    public async Task WhenISelect(string listId)
    {
        _error = null;
        try
        {
            _rows = await world.Store().SelectAsync(listId, CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: select {listId} -> {_error?.GetType().Name ?? $"{_rows?.Count} rows"}");
    }

    [Then("the list {string} should hold {string}")]
    public async Task ThenTheListShouldHold(string listId, string titles)
    {
        var rows = await world.Store().SelectAsync(listId, CancellationToken.None);
        Assert.Equal(titles.Split('|'), rows!.Select(row => row.Title));
    }

    [Then("the insert should fail with a duplicate key")]
    public void ThenTheInsertShouldFailWithADuplicateKey() => Assert.IsType<DuplicateKeyException>(_error);

    [Then("the store should answer the database is unavailable")]
    public void ThenTheStoreShouldAnswerUnavailable() => Assert.IsType<StoreUnavailableException>(_error);

    [Then("the select should give no rows")]
    public void ThenTheSelectShouldGiveNoRows()
    {
        Assert.Null(_error);
        Assert.Null(_rows);
    }
}
