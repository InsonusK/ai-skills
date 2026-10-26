using Reqnroll;
using Shared.Errors;
using Todo.Application.Commands;
using Todo.Application.Services;
using Todo.Commands;
using Todo.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Application.Tests.Steps;

/// <summary>The remote planner faked: the titles it plans or the error it throws, and the titles it was given.</summary>
public sealed class FakeRemotePlanner : IRemotePlanner
{
    public IReadOnlyList<string> Titles { get; set; } = [];
    public Exception? Error { get; set; }
    public IReadOnlyList<string>? Given { get; private set; }

    public Task<IReadOnlyList<Item>> PlanAsync(IReadOnlyList<string> titles, CancellationToken cancellationToken)
    {
        Given = titles;
        if (Error is not null)
        {
            throw Error;
        }
        return Task.FromResult<IReadOnlyList<Item>>(Titles.Select(title => new Item(title)).ToList());
    }
}

[Binding]
public sealed class PlanRemoteItemsSteps(ITestOutputHelper output)
{
    private readonly FakeRemotePlanner _planner = new();
    private PlanRemoteItemsResult? _result;
    private Exception? _error;

    [Given("the remote planner plans {string}")]
    public void GivenTheRemotePlannerPlans(string titles) => _planner.Titles = titles.Split('|');

    [Given("the remote planner fails with the kind {string}")]
    public void GivenTheRemotePlannerFails(string kind) =>
        _planner.Error = new ErrorKindException(Enum.Parse<ErrorKind>(kind, ignoreCase: true), "answered by the scenario");

    [When("I send PlanRemoteItems with the titles {string}")]
    public async Task WhenISendPlanRemoteItems(string titles)
    {
        try
        {
            _result = await new PlanRemoteItemsHandler(_planner).Handle(new PlanRemoteItemsCommand(titles.Split('|')), CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: PlanRemoteItems({titles}) -> {_error?.Message ?? $"{_result!.Items.Count} items"}");
    }

    [Then("the remote result should list the open items {string}")]
    public void ThenTheRemoteResultShouldList(string titles)
    {
        Assert.Null(_error);
        Assert.Equal(titles.Split('|').Select(title => new ItemView(title, false)), _result!.Items);
    }

    [Then("the remote planner should have been given {string}")]
    public void ThenTheRemotePlannerShouldHaveBeenGiven(string titles) => Assert.Equal(titles.Split('|'), _planner.Given);

    [Then("the command should fail with the kind {string}")]
    public void ThenTheCommandShouldFailWithTheKind(string kind) =>
        Assert.Equal(Enum.Parse<ErrorKind>(kind, ignoreCase: true), ErrorKindException.KindOf(_error));
}
