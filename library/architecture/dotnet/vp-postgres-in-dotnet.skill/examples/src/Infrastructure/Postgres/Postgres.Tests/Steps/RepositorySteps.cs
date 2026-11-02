using System.Globalization;
using Postgres.Abstractions;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace Postgres.Tests.Steps;

/// <summary>
/// The reads of a selector with its row type erased, so one step serves the rows
/// of a table and a projection. <see cref="First"/>, <see cref="Take"/>, and
/// <see cref="Page"/> stay null for an unordered selector, which has none of them.
/// </summary>
internal sealed record Reads(
    Func<Task<IEnumerable<object>>> All,
    Func<Task<object?>> Single,
    Func<Task<int>> Count,
    Func<Task<bool>> Any,
    Func<Task<object?>>? First = null,
    Func<int, Task<IEnumerable<object>>>? Take = null,
    Func<int, int, Task<IEnumerable<object>>>? Page = null)
{
    public static Reads Of<TResult>(ISelection<TResult> selection) where TResult : class => new(
        async () => await selection.AllAsync(CancellationToken.None),
        async () => await selection.SingleAsync(CancellationToken.None),
        () => selection.CountAsync(CancellationToken.None),
        () => selection.AnyAsync(CancellationToken.None));

    public static Reads OfOrdered<TResult>(IOrderedSelection<TResult> selection) where TResult : class => Of<TResult>(selection) with
    {
        First = async () => await selection.FirstAsync(CancellationToken.None),
        Take = async count => await selection.TakeAsync(count, CancellationToken.None),
        Page = async (page, size) => await selection.PageAsync(page, size, CancellationToken.None),
    };
}

/// <summary>The steps of the shared scenarios on the repository, the selector, the specification, and the outcomes.</summary>
[Binding]
public sealed class RepositorySteps(PostgresWorld world, ITestOutputHelper output)
{
    private const long NoOwner = -1;

    private readonly Dictionary<string, long> _owners = [];
    private Exception? _error;
    private TaskRow? _read;
    private IRowSelector<TaskRow>? _selector;
    private Reads? _reads;
    private Reads? _firstOrder;
    private Reads? _secondOrder;
    private int _updated;
    private int _deleted;

    private long OwnerId(string name) => _owners[name];

    private async Task AttemptAsync(string what, Func<Task> action)
    {
        _error = null;
        try
        {
            await action();
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: {what} -> {_error?.GetType().Name ?? "ok"}");
    }

    private void Select(IRowSelector<TaskRow> selector)
    {
        _selector = selector;
        _reads = Reads.Of(selector);
    }

    private void SelectOrdered(IOrderedRowSelector<TaskRow> selector)
    {
        _selector = selector;
        _reads = Reads.OfOrdered(selector);
    }

    /// <summary>Reads each field the table names from the row object itself, so a text standing in for the rows fails.</summary>
    private static void AssertRow(DataTableRow expected, object? row)
    {
        Assert.NotNull(row);
        foreach (var (column, value) in expected)
        {
            var field = row.GetType().GetProperty(char.ToUpperInvariant(column[0]) + column[1..]);
            Assert.NotNull(field);
            Assert.Equal(value, Convert.ToString(field.GetValue(row), CultureInfo.InvariantCulture)!.ToLowerInvariant());
        }
    }

    private static void AssertRows(Table expected, IEnumerable<object> rows)
    {
        var answered = rows.ToList();
        Assert.Equal(expected.RowCount, answered.Count);
        foreach (var (row, index) in expected.Rows.Select((row, index) => (row, index)))
        {
            AssertRow(row, answered[index]);
        }
    }

    private static void AssertOneRow(Table expected, object? row) => AssertRow(Assert.Single(expected.Rows), row);

    // ---- the tables

    [Given("a database with the sample tables")]
    public Task GivenADatabaseWithTheSampleTables() => world.CreateSampleTablesAsync();

    [Given("the owner {string}")]
    public async Task GivenTheOwner(string name)
    {
        var owner = new OwnerRow { Name = name };
        await world.Repository<OwnerRow>().InsertAsync(owner, CancellationToken.None);
        _owners[name] = owner.Id;
    }

    [Given("the task {string} of {string} is open")]
    public Task GivenTheTaskIsOpen(string title, string owner) =>
        world.Tasks().InsertAsync(new TaskRow { OwnerId = OwnerId(owner), Title = title }, CancellationToken.None);

    [Given("the task {string} of {string} is done")]
    public Task GivenTheTaskIsDone(string title, string owner) =>
        world.Tasks().InsertAsync(new TaskRow { OwnerId = OwnerId(owner), Title = title, Done = true }, CancellationToken.None);

    [Given("one session for the whole scenario")]
    public void GivenOneSession() => world.KeepOneSession();

    // ---- one row

    [When("I insert the task {string} of {string}")]
    public Task WhenIInsertTheTask(string title, string owner) =>
        AttemptAsync($"insert {title}", () => world.Tasks().InsertAsync(new TaskRow { OwnerId = OwnerId(owner), Title = title }, CancellationToken.None));

    [When("I insert the task {string} of an owner nobody has")]
    public Task WhenIInsertTheTaskOfNobody(string title) =>
        AttemptAsync($"insert {title}", () => world.Tasks().InsertAsync(new TaskRow { OwnerId = NoOwner, Title = title }, CancellationToken.None));

    [Given("I read the task {string}")]
    public async Task GivenIReadTheTask(string title) =>
        _read = await world.Tasks().SelectBy(new TaskByTitle(title)).SingleAsync(CancellationToken.None);

    [When("I read the task {string} and update it as done")]
    public async Task WhenIReadAndUpdate(string title)
    {
        await GivenIReadTheTask(title);
        _read!.Done = true;
        await AttemptAsync("update", () => world.Tasks().UpdateAsync(_read, CancellationToken.None));
    }

    [When("I read the task {string} and rename it {string}")]
    public async Task WhenIReadAndRename(string title, string newTitle)
    {
        await GivenIReadTheTask(title);
        _read!.Title = newTitle;
        await AttemptAsync("rename", () => world.Tasks().UpdateAsync(_read, CancellationToken.None));
    }

    [When("I read the task {string} and delete it")]
    public async Task WhenIReadAndDelete(string title)
    {
        await GivenIReadTheTask(title);
        await WhenIDeleteThatRow();
    }

    [When("I delete that row")]
    public Task WhenIDeleteThatRow() => AttemptAsync("delete", () => world.Tasks().DeleteAsync(_read!, CancellationToken.None));

    [Given("another connection deletes the task {string}")]
    public Task GivenAnotherConnectionDeletes(string title) => world.ExecuteAsync($"DELETE FROM tasks WHERE title = '{title}'");

    [When("another connection adds the open task {string} of {string}")]
    public Task WhenAnotherConnectionAdds(string title, string owner) =>
        world.ExecuteAsync($"INSERT INTO tasks (owner_id, title, done) VALUES ({OwnerId(owner)}, '{title}', false)");

    [Then("the table {string} should hold {int} rows for any other connection")]
    public async Task ThenTheTableShouldHold(string table, int rows) =>
        Assert.Equal(rows, await world.CountAsync($"SELECT count(*) FROM {table}"));

    [Then("the table {string} should hold {int} done rows for any other connection")]
    public async Task ThenTheTableShouldHoldDone(string table, int rows) =>
        Assert.Equal(rows, await world.CountAsync($"SELECT count(*) FROM {table} WHERE done"));

    [Then("the task {string} should be done for any other connection")]
    public async Task ThenTheTaskShouldBeDone(string title) =>
        Assert.Equal(1, await world.CountAsync($"SELECT count(*) FROM tasks WHERE title = '{title}' AND done"));

    [Then("no transaction should be open")]
    public void ThenNoTransactionShouldBeOpen() => Assert.Null(world.LastSession!.Database.CurrentTransaction);

    // ---- selecting

    [When("I select the unclosed tasks of {string}")]
    public void WhenISelectUnclosed(string owner) => Select(world.Tasks().SelectBy(new UnclosedTasks(owner)));

    [When("I select the unclosed tasks of {string} ordered by title")]
    public void WhenISelectUnclosedByTitle(string owner) =>
        SelectOrdered(world.Tasks().SelectBy(new UnclosedTasks(owner)).OrderBy(task => task.Title));

    [When("I select the unclosed tasks of {string} ordered by title descending")]
    public void WhenISelectUnclosedByTitleDescending(string owner) =>
        SelectOrdered(world.Tasks().SelectBy(new UnclosedTasks(owner)).OrderByDescending(task => task.Title));

    [When("I select every open task ordered by owner, then by title")]
    public void WhenISelectEveryOpenTask() =>
        SelectOrdered(world.Tasks().SelectBy(new OpenTasks()).OrderBy(task => task.OwnerId).ThenBy(task => task.Title));

    [When("I select the task {string} by its id")]
    public async Task WhenISelectTheTaskById(string title)
    {
        var id = await world.CountAsync($"SELECT id FROM tasks WHERE title = '{title}'");
        Select(world.Tasks().SelectBy(new TaskById(id)));
    }

    [When("I select a task by an id nobody has")]
    public void WhenISelectByAnUnknownId() => Select(world.Tasks().SelectBy(new TaskById(NoOwner)));

    [When("I select the titles with their owners of {string}")]
    public void WhenISelectTitlesWithOwners(string owner) =>
        _reads = Reads.Of(world.Tasks().SelectBy(new TitlesWithOwners(owner)));

    [When("I select the titles with their owners of {string} ordered by title descending")]
    public void WhenISelectTitlesWithOwnersOrdered(string owner) =>
        _reads = Reads.OfOrdered(world.Tasks().SelectBy(new TitlesWithOwners(owner)).OrderByDescending(row => row.Title));

    [When("I order that selector by title and, apart, by title descending")]
    public void WhenIOrderTwice()
    {
        _firstOrder = Reads.OfOrdered(_selector!.OrderBy(task => task.Title));
        _secondOrder = Reads.OfOrdered(_selector!.OrderByDescending(task => task.Title));
    }

    [When("I give one specification of {string} to two selectors")]
    public void WhenIGiveOneSpecification(string owner)
    {
        var unclosed = new UnclosedTasks(owner);
        _reads = Reads.Of(world.Tasks().SelectBy(unclosed));
        _selector = world.Tasks().SelectBy(unclosed);
    }

    [When("I select through a specification that holds a page")]
    public Task WhenISelectThroughAPage() => AttemptAsync("page", () => Task.FromResult(world.Tasks().SelectBy(new WithPage())));

    [When("I select through a specification that holds an order")]
    public Task WhenISelectThroughAnOrder() => AttemptAsync("order", () => Task.FromResult(world.Tasks().SelectBy(new WithOrder())));

    [When("I count the unclosed tasks of {string}")]
    public Task WhenICount(string owner) =>
        AttemptAsync("count", () => world.Tasks().SelectBy(new UnclosedTasks(owner)).CountAsync(CancellationToken.None));

    [When("I update them as done")]
    public async Task WhenIUpdateThemAsDone() => _updated = await _selector!.UpdateAsync(task => task.Done = true, CancellationToken.None);

    [When("I update them as done, renaming {string} to {string}")]
    public Task WhenIUpdateThemRenaming(string from, string to) =>
        AttemptAsync("update", () => _selector!.UpdateAsync(
            task =>
            {
                task.Done = true;
                task.Title = task.Title == from ? to : task.Title;
            },
            CancellationToken.None));

    [When("I delete them")]
    public async Task WhenIDeleteThem() => _deleted = await _selector!.DeleteAsync(CancellationToken.None);

    // ---- what a selector answers

    [Then("All should answer the rows:")]
    public async Task ThenAllShouldAnswer(Table rows) => AssertRows(rows, await _reads!.All());

    [Then("All should answer an empty list")]
    public async Task ThenAllShouldAnswerEmpty() => Assert.Empty(await _reads!.All());

    [Then("Count should answer {int}")]
    public async Task ThenCountShouldAnswer(int count) => Assert.Equal(count, await _reads!.Count());

    [Then("Any should answer true")]
    public async Task ThenAnyShouldAnswerTrue() => Assert.True(await _reads!.Any());

    [Then("Any should answer false")]
    public async Task ThenAnyShouldAnswerFalse() => Assert.False(await _reads!.Any());

    [Then("Single should answer one row:")]
    public async Task ThenSingleShouldAnswer(Table row) => AssertOneRow(row, await _reads!.Single());

    [Then("Single should answer no row")]
    public async Task ThenSingleShouldAnswerNoRow() => Assert.Null(await _reads!.Single());

    [Then("Single should fail with no outcome of the repository")]
    public async Task ThenSingleShouldFail() => await Assert.ThrowsAsync<InvalidOperationException>(_reads!.Single);

    [Then("First should answer one row:")]
    public async Task ThenFirstShouldAnswer(Table row) => AssertOneRow(row, await _reads!.First!());

    [Then("First should answer no row")]
    public async Task ThenFirstShouldAnswerNoRow() => Assert.Null(await _reads!.First!());

    [Then("Take {int} should answer the rows:")]
    public async Task ThenTakeShouldAnswer(int count, Table rows) => AssertRows(rows, await _reads!.Take!(count));

    [Then("Take {int} should be refused")]
    public async Task ThenTakeShouldBeRefused(int count) =>
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _reads!.Take!(count));

    [Then("Page {int} of size {int} should answer the rows:")]
    public async Task ThenPageShouldAnswer(int page, int size, Table rows) => AssertRows(rows, await _reads!.Page!(page, size));

    [Then("Page {int} of size {int} should answer an empty list")]
    public async Task ThenPageShouldAnswerEmpty(int page, int size) => Assert.Empty(await _reads!.Page!(page, size));

    [Then("Page {int} of size {int} should be refused")]
    public async Task ThenPageShouldBeRefused(int page, int size) =>
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _reads!.Page!(page, size));

    [Then("First of the first order should answer one row:")]
    public async Task ThenFirstOfTheFirstOrder(Table row) => AssertOneRow(row, await _firstOrder!.First!());

    [Then("First of the second order should answer one row:")]
    public async Task ThenFirstOfTheSecondOrder(Table row) => AssertOneRow(row, await _secondOrder!.First!());

    [Then("the update should answer {int}")]
    public void ThenTheUpdateShouldAnswer(int rows) => Assert.Equal(rows, _updated);

    [Then("the delete should answer {int}")]
    public void ThenTheDeleteShouldAnswer(int rows) => Assert.Equal(rows, _deleted);

    [Then("the first should count {int}")]
    public async Task ThenTheFirstShouldCount(int rows) => Assert.Equal(rows, await _reads!.Count());

    [Then("the second should delete {int}")]
    public async Task ThenTheSecondShouldDelete(int rows) => Assert.Equal(rows, await _selector!.DeleteAsync(CancellationToken.None));

    [Then("the selection should be refused")]
    public void ThenTheSelectionShouldBeRefused() => Assert.IsType<ArgumentException>(_error);

    [Then("no failure should be reported")]
    public void ThenNoFailure() => Assert.Null(_error);

    // ---- outcomes

    [When("the database answers the failure {string}")]
    public Task WhenTheDatabaseAnswers(string sqlState) =>
        AttemptAsync($"failure {sqlState}", () => world.Tasks().FailWithAsync(sqlState, CancellationToken.None));

    [When("I insert the task {string} of {string} unless it is there")]
    public Task WhenIInsertUnlessThere(string title, string owner) =>
        AttemptAsync($"upsert {title}", () => world.Tasks().InsertUnlessThereAsync(OwnerId(owner), title, CancellationToken.None));

    [When("I rename the task {string} to {string} in one statement")]
    public Task WhenIRename(string from, string to) =>
        AttemptAsync("rename", () => world.Tasks().RenameAsync(from, to, CancellationToken.None));

    [Then("the repository should report a duplicate key")]
    public void ThenADuplicateKey() => Assert.IsType<DuplicateKeyException>(_error);

    [Then("the repository should report a missing reference")]
    public void ThenAMissingReference() => Assert.IsType<MissingReferenceException>(_error);

    [Then("the repository should report a changed row")]
    public void ThenAChangedRow() => Assert.IsType<RowChangedException>(_error);

    [Then("the repository should report the database is unavailable")]
    public void ThenUnavailable() => Assert.IsType<StoreUnavailableException>(_error);

    [Then("the failure should reach the caller with no outcome of the repository")]
    public void ThenNoOutcome() => Assert.IsType<Npgsql.PostgresException>(_error);
}
