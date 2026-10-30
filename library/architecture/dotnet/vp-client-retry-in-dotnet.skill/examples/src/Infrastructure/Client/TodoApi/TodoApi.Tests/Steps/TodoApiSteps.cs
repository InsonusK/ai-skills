using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Client.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Reqnroll;
using TodoApi.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace TodoApi.Tests.Steps;

[Binding]
public sealed class TodoApiSteps(ITestOutputHelper output) : IDisposable
{
    private readonly HttpListener _server = new();
    private string _answer = "";
    private bool _mocked;
    private (string Method, string Path, string Body)? _received;
    private IReadOnlyList<TodoApiItem>? _items;
    private Exception? _error;

    [Given("the todo API answers {string}")]
    public void GivenTheTodoApiAnswers(string answer) => _answer = answer;

    [Given("the todo API is mocked")]
    public void GivenTheTodoApiIsMocked() => _mocked = true;

    /// <summary>The adapter against an HTTP server of the todo API serving the scenario's answer, or, mocked, against its own emulator.</summary>
    private TodoApiClient Client()
    {
        if (_mocked)
        {
            return new TodoApiClient(new HttpApiConfig("todo-api", HttpApiConfig.Mock, 80, false, TimeSpan.FromSeconds(1)), NullLogger.Instance, []);
        }
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        _server.Prefixes.Add($"http://127.0.0.1:{port}/");
        _server.Start();
        _ = Task.Run(async () =>
        {
            var context = await _server.GetContextAsync();
            using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
            _received = (context.Request.HttpMethod, context.Request.Url!.AbsolutePath, await reader.ReadToEndAsync());
            context.Response.ContentType = "application/json";
            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(_answer));
            context.Response.Close();
        });
        return new TodoApiClient(new HttpApiConfig("todo-api", "127.0.0.1", port, false, TimeSpan.FromSeconds(1)), NullLogger.Instance, []);
    }

    [When("I call PlanItems with the titles {string}")]
    public async Task WhenICallPlanItems(string titles)
    {
        try
        {
            _items = await Client().PlanItemsAsync(titles.Split('|'), CancellationToken.None);
        }
        catch (Exception error)
        {
            _error = error;
        }
        output.WriteLine($"when: PlanItems {titles} -> {string.Join(", ", _items ?? [])}, error {_error?.Message}");
    }

    [Then("the todo API should have received {word} {string} with {string}")]
    public void ThenTheTodoApiShouldHaveReceived(string method, string path, string body) =>
        Assert.Equal((method, path, body), _received);

    [Then("the items should be {string}")]
    public void ThenTheItemsShouldBe(string items)
    {
        Assert.Null(_error);
        Assert.Equal(items, string.Join("|", _items!.Select(item => $"{item.Title}:{(item.Done ? "done" : "open")}")));
    }

    [Then("the adapter should call exactly these operations, each in the contract:")]
    public void ThenTheAdapterShouldCallExactly(Table table)
    {
        var expected = table.Rows.Select(row => (row["method"], row["path"], row["retryable"] == "yes")).ToList();
        Assert.Equal(expected, TodoApiClient.Operations.Select(operation => (operation.Method.Method, operation.Path, operation.Retryable)).ToList());
        using var contract = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "contract", "openapi.json")));
        var paths = contract.RootElement.GetProperty("paths");
        foreach (var (method, path, _) in TodoApiClient.Operations)
        {
            Assert.True(paths.TryGetProperty(path, out var operations) && operations.TryGetProperty(method.Method.ToLowerInvariant(), out _),
                $"{method} {path} is no operation of the contract");
        }
    }

    public void Dispose() => _server.Close();
}
