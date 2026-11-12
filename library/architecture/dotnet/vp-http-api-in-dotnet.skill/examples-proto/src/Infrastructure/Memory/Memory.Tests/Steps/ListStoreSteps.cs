using Memory.Abstractions;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace Memory.Tests.Steps;

[Binding]
public sealed class ListStoreSteps(ITestOutputHelper output)
{
    private readonly MemoryListStore _store = new();
    private DuplicateKeyException? _duplicate;

    [Given("the list {string} holds {string}")]
    public Task GivenTheListHolds(string listId, string title) => Insert(listId, title);

    [When("I insert {string} into the list {string}")]
    public Task WhenIInsert(string title, string listId) => Insert(listId, title);

    private async Task Insert(string listId, string title)
    {
        try
        {
            await _store.InsertAsync(listId, new ListRow(title, false), CancellationToken.None);
        }
        catch (DuplicateKeyException error)
        {
            _duplicate = error;
        }
        output.WriteLine($"insert {title} into {listId} -> {_duplicate?.Message ?? "ok"}");
    }

    [Then("the list {string} should hold {string}")]
    public async Task ThenTheListShouldHold(string listId, string titles) =>
        Assert.Equal(titles.Split('|'), (await _store.SelectAsync(listId, CancellationToken.None))!.Select(row => row.Title));

    [Then("the insert should fail with a duplicate key")]
    public void ThenTheInsertShouldFailWithADuplicateKey() => Assert.NotNull(_duplicate);

    [Then("selecting the list {string} should give no rows")]
    public async Task ThenSelectingTheListShouldGiveNoRows(string listId) =>
        Assert.Null(await _store.SelectAsync(listId, CancellationToken.None));
}
