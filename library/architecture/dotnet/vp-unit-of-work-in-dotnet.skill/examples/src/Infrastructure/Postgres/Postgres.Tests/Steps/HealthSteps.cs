using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Reqnroll;
using Xunit;

namespace Postgres.Tests.Steps;

[Binding]
public sealed class HealthSteps(PostgresWorld world)
{
    private HealthCheckResult _result;

    [Given("a database that does not answer")]
    public void GivenADatabaseThatDoesNotAnswer() =>
        world.Use(new PostgresConfig { Dsn = "Host=localhost;Port=1;Database=none;Username=none;Timeout=1", MigrateOnStart = false });

    [When("the readiness check runs")]
    public async Task WhenTheReadinessCheckRuns()
    {
        // The check as the health entry point finds it: by name, in the registry
        // AddPostgres filled, with no type of the health variation point.
        var services = new ServiceCollection();
        services.AddPostgres(world.Config!);
        await using var provider = services.BuildServiceProvider();
        var registration = Assert.Single(
            provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations,
            registration => registration.Name == "database");
        _result = await registration.Factory(provider).CheckHealthAsync(new HealthCheckContext { Registration = registration });
    }

    [Then("the readiness check should be {string}")]
    public void ThenTheReadinessCheckShouldBe(string status) =>
        Assert.Equal(Enum.Parse<HealthStatus>(status, ignoreCase: true), _result.Status);
}
