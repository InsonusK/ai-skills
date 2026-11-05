using Client.Abstractions;
using Reqnroll;
using Shared.Errors;
using Todo.Application.Services;
using Todo.Domain.Entities;
using TodoApi.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Application.Tests.Steps;

/// <summary>The todo API's adapter faked: the items it answers or the technical outcome it throws.</summary>
public sealed class FakeTodoApi : ITodoApi
{
    public List<TodoApiItem> Items { get; } = [];
    public Exception? Error { get; set; }

    public Task<IReadOnlyList<TodoApiItem>> PlanItemsAsync(IReadOnlyList<string> titles, CancellationToken cancellationToken) =>
        Error is null ? Task.FromResult<IReadOnlyList<TodoApiItem>>(Items) : throw Error;
}

[Binding]
public sealed class RemotePlannerSteps(ITestOutputHelper output)
{
    private readonly FakeTodoApi _api = new();
    private IReadOnlyList<Item>? _items;
    private Exception? _error;

    [Given("the todo API plans {string}")]
    public void GivenTheTodoApiPlans(string titles) => _api.Items.AddRange(titles.Split('|').Select(title => new TodoApiItem(title, false)));

    [Given("the todo API is unavailable")]
    public void GivenTheTodoApiIsUnavailable() => _api.Error = new ApiUnavailableException("todo-api", "connection refused");

    // The status by its canonical name: the service reads the code, whatever transport answered it.
    [Given("the todo API answers {word} with the message {string}")]
    public void GivenTheTodoApiAnswers(string code, string message) =>
        _api.Error = new ApiStatusException("todo-api", Enum.Parse<ApiStatusCode>(code.Replace("_", ""), ignoreCase: true), message);

    [When("I plan {string} remotely")]
    public async Task WhenIPlanRemotely(string titles)
    {
        try
        {
            _items = await new RemotePlannerService(_api).PlanAsync(titles.Split('|'), CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: Plan({titles}) -> {_items?.Count} items, error {_error?.Message}");
    }

    [Then("the planned items should be the open items {string}")]
    public void ThenThePlannedItemsShouldBe(string titles)
    {
        Assert.Null(_error);
        Assert.Equal(titles.Split('|'), _items!.Select(item => item.Title));
        Assert.All(_items!, item => Assert.False(item.Done));
    }

    [Then("planning should fail with the kind {string}")]
    public void ThenPlanningShouldFailWithTheKind(string kind) =>
        Assert.Equal(Enum.Parse<ErrorKind>(kind, ignoreCase: true), ErrorKindException.KindOf(_error));

    [Then("planning should fail with the kind {string} mentioning {string}")]
    public void ThenPlanningShouldFailMentioning(string kind, string text)
    {
        ThenPlanningShouldFailWithTheKind(kind);
        Assert.Contains(text, _error!.Message);
    }

    [Then("planning should fail with the kind {string} and the message {string}")]
    public void ThenPlanningShouldFailWithTheMessage(string kind, string message)
    {
        ThenPlanningShouldFailWithTheKind(kind);
        Assert.Equal(message, _error!.Message);
    }

    [Then("the cause of the failure should mention {string}")]
    public void ThenTheCauseShouldMention(string text) =>
        Assert.Contains(text, ErrorKindException.CauseOf(_error) ?? "");

    [Then("planning should fail with no kind")]
    public void ThenPlanningShouldFailWithNoKind()
    {
        Assert.NotNull(_error);
        Assert.Null(ErrorKindException.KindOf(_error));
    }
}
