using Reqnroll;
using Sample.Domain.Entities;
using Sample.Domain.ValueObjects;
using Shared.Exceptions;
using Xunit;

namespace Sample.Domain.Tests.StepDefinitions;

[Binding]
public sealed class TodoItemSteps
{
    private TodoItem? _item;
    private DomainException? _error;

    [Given("a completed item")]
    public void GivenCompleted()
    {
        _item = TodoItem.Create(new ItemTitle("x"));
        _item.Complete();
    }

    [When("it is renamed")]
    public void WhenRenamed() => _error = Record.Exception(() => _item!.Rename(new ItemTitle("y"))) as DomainException;

    [When("it is completed again")]
    public void WhenCompletedAgain() => _error = Record.Exception(() => _item!.Complete()) as DomainException;

    [When("a title \"(.*)\" is constructed")]
    public void WhenTitle(string value) => _error = Record.Exception(() => new ItemTitle(value)) as DomainException;

    [Then("a domain error \"(.*)\" is raised")]
    public void ThenError(string code)
    {
        Assert.NotNull(_error);
        Assert.Equal(code, _error!.Code);
    }
}
