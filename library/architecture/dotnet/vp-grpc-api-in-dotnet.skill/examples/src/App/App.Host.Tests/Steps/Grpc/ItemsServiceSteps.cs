using App.Host.Api.Grpc;
using App.Host.Api.Grpc.Endpoint;
using Contracts.Todo.V1;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Todo.Commands;
using Xunit;
using PlanItemsRequest = Contracts.Todo.V1.PlanItemsRequest;

namespace App.Host.Tests.Steps.Grpc;

/// <summary>The fake handler of PlanItems: answers the items the scenario set, records the command.</summary>
public sealed class FakeGrpcPlanItems : IRequestHandler<PlanItems, PlanItemsResult>
{
    public IReadOnlyList<string> Answer { get; set; } = [];
    public PlanItems? Received { get; private set; }

    public Task<PlanItemsResult> Handle(PlanItems command, CancellationToken cancellationToken)
    {
        Received = command;
        return Task.FromResult(new PlanItemsResult(Answer.Select(title => new ItemView(title, false)).ToList()));
    }
}

/// <summary>The application's own services (Services.cs, Endpoint/) over fake handlers.</summary>
[Binding]
public sealed class ItemsServiceSteps(GrpcWorld world)
{
    private readonly FakeGrpcPlanItems _planItems = new();
    private PlanItemsResponse? _response;

    // The services as the composition root fills them.
    public static void ServeApplicationServices(GrpcWorld world) =>
        world.Services = sender => new GrpcEndpoints(Items: new ItemsHandler(sender)).Services;

    [Given("PlanItems returns the items {string} over gRPC")]
    public void GivenPlanItemsReturns(string titles)
    {
        _planItems.Answer = titles.Split('|');
        world.Handlers.AddSingleton<IRequestHandler<PlanItems, PlanItemsResult>>(_planItems);
        ServeApplicationServices(world);
    }

    [When("I call ItemsService.PlanItems with the titles {string}")]
    public async Task WhenICallPlanItems(string titles)
    {
        var request = new PlanItemsRequest();
        request.Titles.AddRange(titles.Split('|'));
        _response = await new ItemsService.ItemsServiceClient(await world.ChannelAsync()).PlanItemsAsync(request);
    }

    [Then("the gRPC response should hold the items {string}, none done")]
    public void ThenTheGrpcResponseShouldHoldTheItems(string titles)
    {
        Assert.Equal(titles.Split('|'), _response!.Items.Select(item => item.Title));
        Assert.All(_response.Items, item => Assert.False(item.Done));
    }

    [Then("PlanItems should have received the titles {string} over gRPC")]
    public void ThenPlanItemsShouldHaveReceived(string titles) => Assert.Equal(titles.Split('|'), _planItems.Received?.Titles);
}
