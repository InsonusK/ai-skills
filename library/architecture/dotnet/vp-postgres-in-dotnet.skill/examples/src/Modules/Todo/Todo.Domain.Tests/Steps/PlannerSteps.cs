using Reqnroll;
using Shared.Errors;
using Todo.Domain.Entities;
using Todo.Domain.Services;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Domain.Tests.Steps;

[Binding]
public sealed class PlannerSteps(ITestOutputHelper output)
{
    private IReadOnlyList<Item>? _items;
    private ErrorKindException? _error;

    [When("I plan the titles {string}")]
    public void WhenIPlanTheTitles(string titles) => Plan(titles.Split('|'));

    [When("I plan no titles")]
    public void WhenIPlanNoTitles() => Plan([]);

    private void Plan(string[] titles)
    {
        try
        {
            _items = new Planner().Plan(titles);
        }
        catch (ErrorKindException error)
        {
            _error = error;
        }
        output.WriteLine($"when: Plan([{string.Join(", ", titles)}]) -> {_error?.Message ?? $"{_items!.Count} items"}");
    }

    [Then("planning should succeed with the titles {string}")]
    public void ThenPlanningShouldSucceedWith(string titles)
    {
        Assert.Null(_error);
        Assert.Equal(titles.Split('|'), _items!.Select(item => item.Title));
        Assert.All(_items!, item => Assert.False(item.Done));
    }

    [Then("planning should fail with {string}")]
    public void ThenPlanningShouldFailWith(string reason)
    {
        Assert.NotNull(_error);
        Assert.Equal(reason, RuleErrors.NameOf(_error));
    }
}
