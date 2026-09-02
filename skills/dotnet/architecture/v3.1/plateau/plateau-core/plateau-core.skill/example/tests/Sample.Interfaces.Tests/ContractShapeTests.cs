using Sample.Interfaces.Commands;
using Sample.Interfaces.Queries;
using Sample.Interfaces.Events;
using Shared.MediatR;
using Xunit;

namespace Sample.Interfaces.Tests;

public class ContractShapeTests
{
    [Fact]
    public void Command_implements_ICommand() =>
        Assert.True(typeof(ICommand<Ardalis.Result.Result<GreetResult>>).IsAssignableFrom(typeof(GreetCommand)));

    [Fact]
    public void Query_implements_IQuery() =>
        Assert.True(typeof(IQuery<Ardalis.Result.Result<GreetingDto>>).IsAssignableFrom(typeof(GetLastGreetingQuery)));

    [Fact]
    public void Event_implements_INotificationEvent_and_is_past_tense() =>
        Assert.True(typeof(INotificationEvent).IsAssignableFrom(typeof(Greeted)));
}
