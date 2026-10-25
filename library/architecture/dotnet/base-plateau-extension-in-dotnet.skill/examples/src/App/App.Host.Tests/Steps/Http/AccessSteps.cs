using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Access;
using App.Host.Api.Http;
using App.Host.Api.Http.Access;
using App.Host.Api.Http.Pipeline;
using BuildingBlocks.Pipeline;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Todo.Application;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Http;

/// <summary>
/// The steps of Access.feature: the application as Program.cs builds it with
/// the access variation point attached - the same decorators and middleware in
/// the same order, over the real module and routes - served on a test server.
/// </summary>
[Binding]
public sealed class AccessSteps(ITestOutputHelper output) : IDisposable
{
    private readonly CapturedLogs _logs = new();
    private ServiceProvider? _services;
    private WebApplication? _app;
    private HttpClient? _client;
    private HttpResponseMessage? _response;
    private string _body = "";

    [Given("the callers {string} and the batch limit {int}")]
    public async Task GivenTheCallers(string tokens, int limit)
    {
        Environment.SetEnvironmentVariable("ACCESS_TOKENS", tokens);
        var config = AccessConfig.Load() with { MaxBatch = limit };
        Environment.SetEnvironmentVariable("ACCESS_TOKENS", null);

        _services = new ServiceCollection()
            .AddLogging(logging => logging.SetMinimumLevel(LogLevel.Trace).AddProvider(_logs))
            .AddSingleton(config)
            .AddTodoModule()
            .AddCommandDecorators(typeof(LoggingBehavior<,>), typeof(AccessCheckBehavior<,>), typeof(BatchLimitBehavior<,>))
            .BuildServiceProvider();
        _app = HttpServer.Build(
            new HttpConfig(8080, "", false),
            new HttpEndpoints(_services.GetRequiredService<ISender>()).Map,
            _logs.Factory,
            host => host.UseTestServer(),
            HttpServer.Activate(_services, [typeof(Authentication)]));
        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    [When("{string} POSTs {string} with the body {string}")]
    public Task WhenACallerPosts(string token, string path, string body) => PostAsync(token, path, body);

    [When("nobody POSTs {string} with the body {string}")]
    public Task WhenNobodyPosts(string path, string body) => PostAsync("", path, body);

    private async Task PostAsync(string token, string path, string body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        if (token != "")
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        _response = await _client!.SendAsync(request);
        _body = await _response.Content.ReadAsStringAsync();
        output.WriteLine($"when: POST {path} {body} as '{token}' -> {(int)_response.StatusCode} {_body}");
    }

    [Then("the response status should be {int}")]
    public void ThenTheResponseStatusShouldBe(int status) => Assert.Equal(status, (int)_response!.StatusCode);

    [Then("the error body should have code {int} and the message {string}")]
    public void ThenTheErrorBodyShouldBe(int code, string message)
    {
        var status = JsonDocument.Parse(_body).RootElement;
        Assert.Equal((code, message), (status.GetProperty("code").GetInt32(), status.GetProperty("message").GetString()));
    }

    [Then("the records should be, in order, {string}")]
    public void ThenTheRecordsShouldBeInOrder(string messages) =>
        Assert.Equal(messages, string.Join('|', _logs.Records.Select(record => record.Message)));

    public void Dispose()
    {
        _client?.Dispose();
        _app?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _services?.Dispose();
        _logs.Factory.Dispose();
    }
}
