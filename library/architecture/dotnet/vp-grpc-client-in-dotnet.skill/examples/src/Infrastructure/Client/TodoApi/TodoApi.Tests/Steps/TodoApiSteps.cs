using System.Net;
using System.Net.Sockets;
using Client.Grpc;
using Contracts.Todo.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Reqnroll;
using TodoApi.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace TodoApi.Tests.Steps;

/// <summary>The todo API as a scenario sets it: it answers PlanItems with the scenario's answer and records what it received.</summary>
internal sealed class ScenarioTodoApi : ItemsService.ItemsServiceBase
{
    public PlanItemsResponse Answer { get; set; } = new();
    public (string Method, PlanItemsRequest Request)? Received { get; private set; }

    public override Task<PlanItemsResponse> PlanItems(PlanItemsRequest request, ServerCallContext context)
    {
        Received = (context.Method, request);
        return Task.FromResult(Answer);
    }
}

[Binding]
public sealed class TodoApiSteps(ITestOutputHelper output) : IAsyncDisposable
{
    private readonly ScenarioTodoApi _api = new();
    private WebApplication? _server;
    private TodoApiClient? _client;
    private bool _mocked;
    private IReadOnlyList<TodoApiItem>? _items;
    private Exception? _error;

    [Given("the todo API answers {string}")]
    public void GivenTheTodoApiAnswers(string answer) => _api.Answer = JsonParser.Default.Parse<PlanItemsResponse>(answer);

    [Given("the todo API is mocked")]
    public void GivenTheTodoApiIsMocked() => _mocked = true;

    /// <summary>The adapter against a gRPC server of the scenario's todo API, or, mocked, against its own emulator.</summary>
    private async Task<TodoApiClient> Client()
    {
        if (_mocked)
        {
            return new TodoApiClient(new GrpcApiConfig("todo-api", GrpcApiConfig.Mock, 80, false, TimeSpan.FromSeconds(1)), NullLogger.Instance);
        }
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(kestrel => kestrel.Listen(IPAddress.Loopback, port, listen => listen.Protocols = HttpProtocols.Http2));
        builder.Services.AddGrpc();
        builder.Services.AddSingleton(_api);
        _server = builder.Build();
        _server.MapGrpcService<ScenarioTodoApi>();
        await _server.StartAsync();
        return new TodoApiClient(new GrpcApiConfig("todo-api", "127.0.0.1", port, false, TimeSpan.FromSeconds(5)), NullLogger.Instance);
    }

    [When("I call PlanItems with the titles {string}")]
    public async Task WhenICallPlanItems(string titles)
    {
        _client = await Client();
        try
        {
            _items = await _client.PlanItemsAsync(titles.Split('|'), CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: PlanItems {titles} -> {string.Join(", ", _items ?? [])}, error {_error?.Message}");
    }

    [Then("the todo API should have received {string} with {string}")]
    public void ThenTheTodoApiShouldHaveReceived(string method, string request) =>
        Assert.Equal((method, JsonParser.Default.Parse<PlanItemsRequest>(request)), _api.Received);

    [Then("the items should be {string}")]
    public void ThenTheItemsShouldBe(string items)
    {
        Assert.Null(_error);
        Assert.Equal(items, string.Join("|", _items!.Select(item => $"{item.Title}:{(item.Done ? "done" : "open")}")));
    }

    [Then("the adapter should call exactly these methods, each in the contract:")]
    public void ThenTheAdapterShouldCallExactly(Table table)
    {
        Assert.Equal(table.Rows.Select(row => row["method"]), TodoApiClient.Methods);
        // The descriptor Grpc.Tools compiled from the copied file at this build.
        var contract = TodoReflection.Descriptor.Services
            .SelectMany(service => service.Methods.Select(method => $"/{service.FullName}/{method.Name}"))
            .ToList();
        output.WriteLine($"then: adapter [{string.Join(", ", TodoApiClient.Methods)}], contract [{string.Join(", ", contract)}]");
        Assert.All(TodoApiClient.Methods, method => Assert.Contains(method, contract));
    }

    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        if (_server is not null)
        {
            await _server.DisposeAsync();
        }
    }
}
