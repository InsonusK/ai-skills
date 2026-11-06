using Ardalis.Specification;
using Reqnroll;
using Shared.Errors;
using Shared.Persistence;
using Xunit;
using Xunit.Abstractions;

namespace Memory.Tests.Steps;

/// <summary>A root of these scenarios only.</summary>
public sealed class Note(string title) : IAggregateRoot
{
    public string Title { get; private set; } = title;

    public void Rename(string title) => Title = title;
}

public sealed class NoteTitled : Specification<Note>, ISingleResultSpecification<Note>
{
    public NoteTitled(string title) => Query.Where(note => note.Title == title);
}

public sealed class NotesByTitle : Specification<Note>
{
    public NotesByTitle(int page, int size) => Query.OrderBy(note => note.Title).Skip((page - 1) * size).Take(size);
}

/// <summary>A root of these scenarios with a key the caller sets, as a list has its name.</summary>
public sealed class Shelf(string id) : IAggregateRoot
{
    public string Id { get; } = id;
}

public sealed class ShelfById : Specification<Shelf>
{
    public ShelfById(string id) => Query.Where(shelf => shelf.Id == id);
}

[Binding]
public sealed class RepositorySteps(ITestOutputHelper output)
{
    private readonly MemoryRepository<Shelf> _shelves = new(new MemoryStore());

    private readonly MemoryRepository<Note> _notes = new(new MemoryStore());
    private Exception? _error;

    [Given("the notes {string} are added")]
    public async Task GivenTheNotesAreAdded(string titles)
    {
        foreach (var title in titles.Split('|'))
        {
            await _notes.AddAsync(new Note(title));
        }
    }

    [When("I pick the note {string}, rename it {string}, and update it")]
    public async Task WhenIPickRenameAndUpdate(string title, string renamed)
    {
        var note = await _notes.SingleOrDefaultAsync(new NoteTitled(title));
        note!.Rename(renamed);
        await _notes.UpdateAsync(note);
    }

    [When("I delete the notes titled {string}")]
    public Task WhenIDeleteTheNotesTitled(string title) => _notes.DeleteRangeAsync(new NoteTitled(title));

    [When("I {word} through a specification with a page")]
    public async Task WhenICallThroughASpecificationWithAPage(string call)
    {
        var paged = new NotesByTitle(1, 2);
        try
        {
            _ = call switch
            {
                "count" => await _notes.CountAsync(paged),
                "ask" => await _notes.AnyAsync(paged) ? 1 : 0,
                "delete" => await _notes.DeleteRangeAsync(paged),
                _ => throw new ArgumentOutOfRangeException(nameof(call), call, "no such call"),
            };
        }
        catch (ArgumentException error) when (error is not ArgumentOutOfRangeException)
        {
            _error = error;
        }
        output.WriteLine($"{call} with a page -> {_error?.Message ?? "ok"}");
    }

    [Then("the note picked by the title {string} should be {string}")]
    public async Task ThenTheNotePickedShouldBe(string title, string expected) =>
        Assert.Equal(expected, (await _notes.SingleOrDefaultAsync(new NoteTitled(title)))?.Title);

    [Then("the notes ordered by title, page {int} of size {int}, should be {string}")]
    public async Task ThenTheNotesOrderedByTitleShouldBe(int page, int size, string titles) =>
        Assert.Equal(titles.Split('|'), (await _notes.ListAsync(new NotesByTitle(page, size))).Select(note => note.Title));

    [Then("{int} notes should be left")]
    public async Task ThenNotesShouldBeLeft(int count) => Assert.Equal(count, await _notes.CountAsync());

    [When("I add the list {string} twice")]
    public async Task WhenIAddTwice(string id)
    {
        await _shelves.AddAsync(new Shelf(id));
        try
        {
            await _shelves.AddAsync(new Shelf(id));
        }
        catch (ErrorKindException error)
        {
            _error = error;
        }
    }

    [Then("the second add should fail with the kind {string}")]
    public void ThenTheSecondAddShouldFail(string kind) =>
        Assert.Equal(Enum.Parse<ErrorKind>(kind, ignoreCase: true), ErrorKindException.KindOf(_error));

    [Then("{int} list {string} should be kept")]
    public async Task ThenListsShouldBeKept(int count, string id) => Assert.Equal(count, await _shelves.CountAsync(new ShelfById(id)));

    [Then("the call should be refused")]
    public void ThenTheCallShouldBeRefused() => Assert.IsType<ArgumentException>(_error);
}
