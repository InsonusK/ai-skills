using App.Host.Api.Cli;
using App.Host.Api.Cli.Endpoint;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Todo.Commands;
using Xunit;

namespace App.Host.Tests.Steps.Cli;

/// <summary>PlanItems faked: it returns the configured items and records the titles it was asked.</summary>
public sealed class FakePlanItemsHandler : IRequestHandler<PlanItemsCommand, PlanItemsResult>
{
    private IReadOnlyList<ItemView> _items = [];

    public IReadOnlyList<string>? ReceivedTitles { get; private set; }

    public void Returns(IReadOnlyList<ItemView> items) => _items = items;

    public Task<PlanItemsResult> Handle(PlanItemsCommand command, CancellationToken cancellationToken)
    {
        ReceivedTitles = command.Titles;
        return Task.FromResult(new PlanItemsResult(_items));
    }
}

/// <summary>
/// The steps of items.feature (a copy never edited): the items resource over
/// the command handlers replaced by fakes, as the CLI variation point says -
/// a resource's scenarios check only its own flags and output. Scoped to the
/// feature, as its "I run" builds the endpoints over the fake.
/// </summary>
[Binding]
[Scope(Feature = "items commands")]
public sealed class CliItemsSteps(CliWorld world)
{
    private readonly FakePlanItemsHandler _planItems = new();

    private CliEndpoints Endpoints()
    {
        var provider = new ServiceCollection()
            .AddMediatR(mediatr => mediatr.RegisterServicesFromAssemblyContaining<FakePlanItemsHandler>())
            .AddSingleton<IRequestHandler<PlanItemsCommand, PlanItemsResult>>(_planItems)
            .BuildServiceProvider();
        return new CliEndpoints(new ItemsHandler(provider.GetRequiredService<ISender>()));
    }

    [Given("PlanItems returns the items {string}")]
    public void GivenPlanItemsReturns(string titles) =>
        _planItems.Returns(titles.Split('|').Select(title => new ItemView(title, false)).ToList());

    [When("I run {string}")]
    public Task WhenIRun(string args) => world.RunAsync("todo", "1.2.3", args, Endpoints().Resources());

    [Then("PlanItems should have received the titles {string}")]
    public void ThenPlanItemsShouldHaveReceived(string titles) =>
        Assert.Equal(titles.Split('|'), _planItems.ReceivedTitles);
}
