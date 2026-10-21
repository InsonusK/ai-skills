using App.Host.Api.Management;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Management;

/// <summary>A dependency's check that answers what the scenario set.</summary>
public sealed class FakeHealthCheck(HealthStatus? status) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        status is { } answer
            ? Task.FromResult(new HealthCheckResult(answer, $"{context.Registration.Name} is {answer}"))
            : throw new InvalidOperationException("connection refused");
}

/// <summary>The management endpoints on a test server, with the readiness checks of the scenario.</summary>
[Binding]
public sealed class HealthSteps(ITestOutputHelper output) : IDisposable
{
    private readonly List<ReadinessCheck> _checks = [];
    private readonly CapturedLogs _logs = new();
    private string _version = "0.1.0";
    private WebApplication? _app;
    private HttpResponseMessage? _response;
    private string _body = "";

    private void Check(string name, HealthStatus? status) => _checks.Add(new ReadinessCheck(name, new FakeHealthCheck(status)));

    [Given("a readiness check {string} that passes")]
    public void GivenAReadinessCheckThatPasses(string name) => Check(name, HealthStatus.Healthy);

    [Given("a readiness check {string} that fails")]
    public void GivenAReadinessCheckThatFails(string name) => Check(name, HealthStatus.Unhealthy);

    [Given("a readiness check {string} that is degraded")]
    public void GivenAReadinessCheckThatIsDegraded(string name) => Check(name, HealthStatus.Degraded);

    [Given("a readiness check {string} that throws")]
    public void GivenAReadinessCheckThatThrows(string name) => Check(name, null);

    [Given("the service version is {string}")]
    public void GivenTheServiceVersionIs(string version) => _version = version;

    [When("I GET {string} from the management port")]
    public async Task WhenIGetFromTheManagementPort(string path)
    {
        _app = ManagementServer.Build(new ManagementConfig(8081), _version, _logs.Factory, host => host.UseTestServer(), [.. _checks]);
        await _app.StartAsync();
        _response = await _app.GetTestClient().GetAsync(path);
        _body = await _response.Content.ReadAsStringAsync();
        output.WriteLine($"when: GET {path} -> {(int)_response.StatusCode} {_body}");
    }

    [Then("the management status should be {int}")]
    public void ThenTheManagementStatusShouldBe(int status) => Assert.Equal(status, (int)_response!.StatusCode);

    [Then("the management response body should be {string}")]
    public void ThenTheManagementResponseBodyShouldBe(string body) => Assert.Equal(body, _body);

    [Then("the failed readiness check {string} should be logged")]
    public void ThenTheFailedReadinessCheckShouldBeLogged(string name)
    {
        var record = Assert.Single(_logs.Records, record => record.Message == "readiness check failed");
        Assert.Equal(name, record.Attributes["check"]);
    }

    public void Dispose()
    {
        _app?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _logs.Factory.Dispose();
    }
}
