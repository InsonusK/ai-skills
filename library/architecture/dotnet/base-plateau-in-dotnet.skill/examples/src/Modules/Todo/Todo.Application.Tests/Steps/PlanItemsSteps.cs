using Reqnroll;
using Shared.Errors;
using Todo.Application.Commands;
using Todo.Interfaces.Commands;
using Xunit;
using Xunit.Abstractions;

namespace Todo.Application.Tests.Steps;

[Binding]
public sealed class PlanItemsSteps(ITestOutputHelper output)
{
    private PlanItemsResult? _result;
    private Exception? _error;

    [When("I send PlanItems with the titles {string}")]
    public async Task WhenISendPlanItems(string titles)
    {
        try
        {
            _result = await new PlanItemsHandler().Handle(new PlanItems(titles.Split('|')), CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: PlanItems({titles}) -> {_error?.Message ?? $"{_result!.Items.Count} items"}");
    }

    [Then("the result should list the open items {string}")]
    public void ThenTheResultShouldList(string titles)
    {
        Assert.Null(_error);
        Assert.Equal(titles.Split('|').Select(title => new ItemView(title, false)), _result!.Items);
    }

    [Then("the command should be rejected as invalid")]
    public void ThenTheCommandShouldBeRejectedAsInvalid() =>
        Assert.Equal(ErrorKind.Invalid, ErrorKindException.KindOf(_error));
}
