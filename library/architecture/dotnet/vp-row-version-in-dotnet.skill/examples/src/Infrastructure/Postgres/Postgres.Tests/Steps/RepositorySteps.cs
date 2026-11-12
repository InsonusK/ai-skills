using Reqnroll;
using Shared.Errors;
using Xunit;
using Xunit.Abstractions;

namespace Postgres.Tests.Steps;

/// <summary>
/// The steps of the shared scenarios on the repository and its outcomes. In
/// dotnet the repository is Ardalis's over EF Core, and an outcome is what
/// <see cref="PostgresFailures"/> tells of the failure, as the store failures
/// middleware does for a command.
/// </summary>
[Binding]
public sealed class RepositorySteps(PostgresWorld world, ITestOutputHelper output)
{
    private const long NoOwner = -1;

    private readonly Dictionary<string, long> _owners = [];
    private Exception? _error;
    private OwnerTask? _read;
    private UnclosedTasks? _selected;

    private long OwnerId(string name) => _owners[name];

    public async Task AttemptAsync(string what, Func<Task> action)
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

    private ErrorKind? Told() => new PostgresFailures().KindOf(_error!)?.Kind;

    // ---- the tables

    [Given("a database with the sample tables")]
    public Task GivenADatabaseWithTheSampleTables() => world.CreateSampleTablesAsync();

    [Given("the owner {string}")]
    public async Task GivenTheOwner(string name)
    {
        var owner = new Owner { Name = name };
        await world.Repository<Owner>().AddAsync(owner);
        _owners[name] = owner.Id;
    }

    [Given("the task {string} of {string} is open")]
    public Task GivenTheTaskIsOpen(string title, string owner) =>
        world.Tasks().AddAsync(new OwnerTask { OwnerId = OwnerId(owner), Title = title });

    [Given("the task {string} of {string} is done")]
    public Task GivenTheTaskIsDone(string title, string owner) =>
        world.Tasks().AddAsync(new OwnerTask { OwnerId = OwnerId(owner), Title = title, Done = true });

    [Given("one session for the whole scenario")]
    public void GivenOneSession() => world.KeepOneSession();

    // ---- one root

    [When("I insert the task {string} of {string}")]
    public Task WhenIInsertTheTask(string title, string owner) =>
        AttemptAsync($"insert {title}", () => world.Tasks().AddAsync(new OwnerTask { OwnerId = OwnerId(owner), Title = title }));

    [When("I insert the task {string} of an owner nobody has")]
    public Task WhenIInsertTheTaskOfNobody(string title) =>
        AttemptAsync($"insert {title}", () => world.Tasks().AddAsync(new OwnerTask { OwnerId = NoOwner, Title = title }));

    [Given("I read the task {string}")]
    public async Task GivenIReadTheTask(string title) => _read = await world.Tasks().SingleOrDefaultAsync(new TaskByTitle(title));

    [When("I read the task {string} and update it as done")]
    public async Task WhenIReadAndUpdate(string title)
    {
        await GivenIReadTheTask(title);
        _read!.Done = true;
        await AttemptAsync("update", () => world.Tasks().UpdateAsync(_read));
    }

    [When("I read the task {string} and rename it {string}")]
    public async Task WhenIReadAndRename(string title, string newTitle)
    {
        await GivenIReadTheTask(title);
        _read!.Title = newTitle;
        await AttemptAsync("rename", () => world.Tasks().UpdateAsync(_read));
    }

    [When("I read the task {string} and delete it")]
    public async Task WhenIReadAndDelete(string title)
    {
        await GivenIReadTheTask(title);
        await WhenIDeleteThatRow();
    }

    [When("I delete that row")]
    public Task WhenIDeleteThatRow() => AttemptAsync("delete", () => world.Tasks().DeleteAsync(_read!));

    [Given("another connection deletes the task {string}")]
    public Task GivenAnotherConnectionDeletes(string title) => world.ExecuteAsync($"DELETE FROM tasks WHERE title = '{title}'");

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

    // ---- several roots by a specification

    [When("I select the unclosed tasks of {string}")]
    public void WhenISelectUnclosed(string owner) => _selected = new UnclosedTasks(owner);

    [When("I update them as done, renaming {string} to {string}")]
    public Task WhenIUpdateThemRenaming(string from, string to) =>
        AttemptAsync("update", async () =>
        {
            var tasks = world.Tasks();
            var picked = await tasks.ListAsync(_selected!);
            foreach (var task in picked)
            {
                task.Done = true;
                task.Title = task.Title == from ? to : task.Title;
            }
            await tasks.UpdateRangeAsync(picked);
        });

    [When("I count the unclosed tasks of {string}")]
    public Task WhenICount(string owner) => AttemptAsync("count", () => world.Tasks().CountAsync(new UnclosedTasks(owner)));

    [When(@"^I (count|ask|delete) the unclosed tasks of ""(.*)"" through a specification with a page$")]
    public Task WhenICallThroughAPage(string call, string owner) =>
        AttemptAsync($"{call} with a page", () => call switch
        {
            "count" => world.Tasks().CountAsync(new FirstOfUnclosedTasks(owner)),
            "ask" => world.Tasks().AnyAsync(new FirstOfUnclosedTasks(owner)),
            "delete" => world.Tasks().DeleteRangeAsync(new FirstOfUnclosedTasks(owner)),
            _ => throw new ArgumentOutOfRangeException(nameof(call), call, "no such call"),
        });

    [When("I list the unclosed tasks of {string} through a specification with a page")]
    public Task WhenIListThroughAPage(string owner) =>
        AttemptAsync("list with a page", async () => _listed = await world.Tasks().ListAsync(new FirstOfUnclosedTasks(owner)));

    private List<OwnerTask>? _listed;

    [Then("the list should hold the task {string}")]
    public void ThenTheListShouldHold(string title) => Assert.Equal(title, Assert.Single(_listed!).Title);

    [Then("the call should be refused")]
    public void ThenTheCallShouldBeRefused() => Assert.IsType<ArgumentException>(_error);

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
    public void ThenADuplicateKey() => Assert.Equal(ErrorKind.Conflict, Told());

    [Then("the repository should report a missing reference")]
    public void ThenAMissingReference() => Assert.Equal(ErrorKind.NotFound, Told());

    [Then("the repository should report a changed row")]
    public void ThenAChangedRow() => Assert.Equal(ErrorKind.ConcurrentChange, Told());

    [Then("the repository should report the database is unavailable")]
    public void ThenUnavailable() => Assert.Equal(ErrorKind.Unavailable, Told());

    [Then("the failure should reach the caller with no outcome of the repository")]
    public void ThenNoOutcome()
    {
        Assert.IsType<Npgsql.PostgresException>(_error);
        Assert.Null(Told());
    }
}
