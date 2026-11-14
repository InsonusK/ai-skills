using Reqnroll;
using Shared.Errors;
using Todo.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Domain.Tests.Steps;

[Binding]
public sealed class ItemSteps(ITestOutputHelper output)
{
    private Item? _item;
    private string _outcome = "";
    private ErrorKindException? _completeError;

    [When("I create an item titled {string}")]
    public void WhenICreateAnItemTitled(string title)
    {
        try
        {
            _item = new Item(title);
            _outcome = "created";
        }
        catch (ErrorKindException error)
        {
            _item = null;
            _outcome = RuleErrors.NameOf(error);
        }
        output.WriteLine($"when: new Item({title}) -> {_outcome}");
    }

    [When("I create an item with a title of {int} characters")]
    public void WhenICreateAnItemWithATitleOf(int length) => WhenICreateAnItemTitled(new string('x', length));

    [Then("the item outcome should be {string}")]
    public void ThenTheItemOutcomeShouldBe(string outcome) => Assert.Equal(outcome, _outcome);

    [Then("the item title should be {string}")]
    public void ThenTheItemTitleShouldBe(string title) => Assert.Equal(title, _item?.Title ?? "");

    [Given("an open item titled {string}")]
    public void GivenAnOpenItemTitled(string title) => _item = new Item(title);

    [Given("the item is completed")]
    public void GivenTheItemIsCompleted() => _item!.Complete();

    [When("I complete the item")]
    public void WhenICompleteTheItem()
    {
        try
        {
            _item!.Complete();
        }
        catch (ErrorKindException error)
        {
            _completeError = error;
        }
        output.WriteLine($"when: Complete() -> {_completeError?.Message ?? "done"}");
    }

    [Then("the item should be done")]
    public void ThenTheItemShouldBeDone()
    {
        Assert.Null(_completeError);
        Assert.True(_item!.Done);
    }

    [Then("completing should fail with {string}")]
    public void ThenCompletingShouldFailWith(string reason)
    {
        Assert.NotNull(_completeError);
        Assert.Equal(reason, RuleErrors.NameOf(_completeError));
    }
}
