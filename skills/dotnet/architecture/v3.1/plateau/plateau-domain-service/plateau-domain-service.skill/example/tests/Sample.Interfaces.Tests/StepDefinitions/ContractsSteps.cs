using Ardalis.Result;
using Reqnroll;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;
using Shared.Concurrency;
using Shared.MediatR;
using Shared.Timestamps;
using Xunit;

namespace Sample.Interfaces.Tests.StepDefinitions;

[Binding]
public sealed class ContractsSteps
{
    private AddItemCommand? _add;
    private RenameItemCommand? _rename;

    [When("an AddItemCommand is created")]
    public void WhenAdd() => _add = new AddItemCommand(new SoftItemTitle("x"), DateTimeOffset.UtcNow);

    [Then("it implements ICommand of Result of AddItemResult")]
    public void ThenAddShape() => Assert.IsAssignableFrom<ICommand<Result<AddItemResult>>>(_add);

    [Then("it implements ICommandWithTimestamp")]
    public void ThenTimestamp() => Assert.IsAssignableFrom<ICommandWithTimestamp>(_add);

    [When("a RenameItemCommand is created with expected version (.*)")]
    public void WhenRename(uint v) =>
        _rename = new RenameItemCommand(7, new SoftItemTitle("x"), DateTimeOffset.UtcNow, v);

    [Then("its Versions map holds TodoItem id-to-version (.*)")]
    public void ThenVersions(uint v)
    {
        var command = Assert.IsAssignableFrom<IHasVersions>(_rename);
        Assert.Equal(v, command.Versions["TodoItem"][7]);
    }
}
