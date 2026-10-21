using App.Host.Api.Http;
using App.Host.Api.Http.Endpoint;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Todo.Interfaces.Commands;
using Xunit;

namespace App.Host.Tests.Steps.Http;

/// <summary>The fake handler of PlanItems: answers the items the scenario set, records the command.</summary>
public sealed class FakePlanItems : IRequestHandler<PlanItems, PlanItemsResult>
{
    public IReadOnlyList<string> Answer { get; set; } = [];
    public PlanItems? Received { get; private set; }

    public Task<PlanItemsResult> Handle(PlanItems command, CancellationToken cancellationToken)
    {
        Received = command;
        return Task.FromResult(new PlanItemsResult(Answer.Select(title => new ItemView(title, false)).ToList()));
    }
}

/// <summary>The application's own routes (Routes.cs, Endpoint/) over fake handlers.</summary>
[Binding]
public sealed class ItemsSteps(HttpWorld world)
{
    private readonly FakePlanItems _planItems = new();

    // The routes as the composition root fills them.
    public static void MapApplicationRoutes(HttpWorld world) =>
        world.Routes = (routes, sender) => new HttpEndpoints(Items: new ItemsHandler(sender)).Map(routes);

    [Given("PlanItems returns the items {string}")]
    public void GivenPlanItemsReturns(string titles)
    {
        _planItems.Answer = titles == "" ? [] : titles.Split('|');
        world.Handlers.AddSingleton<IRequestHandler<PlanItems, PlanItemsResult>>(_planItems);
        MapApplicationRoutes(world);
    }

    [Then("PlanItems should have received the titles {string}")]
    public void ThenPlanItemsShouldHaveReceived(string titles) => Assert.Equal(titles.Split('|'), _planItems.Received?.Titles);

    [Then("PlanItems should have received no titles")]
    public void ThenPlanItemsShouldHaveReceivedNoTitles() => Assert.Empty(_planItems.Received!.Titles);
}
