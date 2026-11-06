using Reqnroll;
using Shared.Errors;
using Todo.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Domain.Tests.Steps;

[Binding]
public sealed class ItemListSteps(ITestOutputHelper output)
{
    private ItemList? _list;
    private ErrorKindException? _error;

    [Given("a list {string}")]
    public void GivenAList(string listId) => _list = new ItemList(listId);

    [Given("I add {string} to the list")]
    [When("I add {string} to the list")]
    public void WhenIAddToTheList(string title)
    {
        try
        {
            _list!.Add(new Item(title));
        }
        catch (ErrorKindException error)
        {
            _error = error;
        }
        output.WriteLine($"add {title} -> {_error?.Message ?? "ok"}");
    }

    [Then("the list should hold {string}")]
    public void ThenTheListShouldHold(string titles) => Assert.Equal(titles.Split('|'), _list!.Items.Select(item => item.Title));

    [Then("the add should fail with the kind {string} and the message {string}")]
    public void ThenTheAddShouldFail(string kind, string message)
    {
        Assert.Equal(Enum.Parse<ErrorKind>(kind.Replace(" ", ""), ignoreCase: true), _error?.Kind);
        Assert.Equal(message, _error!.Message);
    }
}
